using Oxide.CompilerServices.Messages;
using Oxide.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
#if NET40_OR_GREATER || NETSTANDARD
using System.Threading.Tasks;
#endif

namespace Oxide.CompilerServices
{
    public class CompilerStream : IObjectStream<CompilerMessage>
    {
        public event Action<IObjectStream<CompilerMessage>, StreamState, StreamState> OnStateChanged;
        public event Action<IObjectStream<CompilerMessage>, CompilerMessage> OnMessage;

        public StreamState State { get; protected set; }

        public bool CanRead => State == StreamState.Ready && _input.CanRead;
        public bool CanWrite => State == StreamState.Ready && _output.CanWrite;

        private readonly Process _compilerProcess;
        private readonly Stream _input;
        private readonly Stream _output;
        private readonly BinaryFormatter _formatter;
        private readonly Queue<CompilerMessage> _messageQueue;
        private readonly AutoResetEvent _autoReset;
#if NET35
        private readonly Thread _readThread;
        private readonly Thread _writeThread;
#else
        private readonly Task _readTask;
        private readonly Task _writeTask;
        private readonly CancellationTokenSource _cts;
#endif

        public CompilerStream(Process process) : this(process.StandardOutput.BaseStream, process.StandardInput.BaseStream)
        {
            _compilerProcess = process;
            process.Exited += (s, e) => OnProcessExit();
        }

        public CompilerStream(Stream input, Stream output)
        {
            _input = input;
            _output = output;
            _formatter = new BinaryFormatter();
            _messageQueue = new Queue<CompilerMessage>();
            _autoReset = new AutoResetEvent(false);
#if NET35
            _readThread = new Thread(ReadWorker) { IsBackground = true, Name = "Oxide.Compiler.Read" };
            _writeThread = new Thread(WriteWorker) { IsBackground = true, Name = "Oxide.Compiler.Write" };
            _readThread.Start();
            _writeThread.Start();
#else
            _cts = new CancellationTokenSource();
            _readTask = new Task(ReadWorker, _cts.Token, TaskCreationOptions.LongRunning);
            _writeTask = new Task(WriteWorker, _cts.Token, TaskCreationOptions.LongRunning);
            _readTask.Start();
            _writeTask.Start();
#endif
            SetState(StreamState.Ready);
        }

        private void ReadWorker()
        {
            while (CanRead)
            {
                int len = ReadLength();
                if (len == 0)
                {
                    continue;
                }
                ReadMessage(len);    
            }
        }

        private void WriteWorker()
        {
            bool wait = true;
            while (CanWrite)
            {
                if (wait) _autoReset.WaitOne();
                wait = false;
                if (!CanWrite)
                {
                    return;
                }

                CompilerMessage message;
                lock (_messageQueue)
                {
                    if (_messageQueue.Count == 0)
                    {
                        wait = true;
                        continue;
                    }

                    message = _messageQueue.Dequeue();
                }

                _formatter.Serialize(_output, message);
            }
        }

        private void ReadMessage(int length)
        {
            byte[] data = new byte[length];
            int count;
            int sum = 0;
            while (length - sum > 0 && (count = _input.Read(data, sum, length - sum)) > 0)
            {
                sum += count;
            }

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                CompilerMessage message = (CompilerMessage)_formatter.Deserialize(memoryStream);
                OnMessageReceived(message);
                OnMessage?.Invoke(this, message);
            }
        }

        private int ReadLength()
        {
            const int lensize = sizeof(int);
            byte[] lenbuf = new byte[lensize];
            int bytesRead = _input.Read(lenbuf, 0, lensize);
            if (bytesRead == 0)
            {
                return 0;
            }

            if (bytesRead != lensize)
            {
                // TODO: Hack to ignore BOM
                Array.Resize(ref lenbuf, Encoding.UTF8.GetPreamble().Length);
                if (Encoding.UTF8.GetPreamble().SequenceEqual(lenbuf))
                {
                    return ReadLength();
                }

                throw new IOException(string.Format("Expected {0} bytes but read {1}", lensize, bytesRead));
            }
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenbuf, 0));
        }

        public void PushMessage(CompilerMessage graph)
        {
            if (graph == null)
            {
                return;
            }

            lock (_messageQueue)
            {
                _messageQueue.Enqueue(graph);
            }
            _autoReset.Set();
        }

        private void SetState(StreamState state)
        {
            StreamState old = State;
            if (old == state)
            {
                return;
            }
            State = state;
            OnStateChange(state, old);
            OnStateChanged?.Invoke(this, state, old);
        }

        protected virtual void OnMessageReceived(CompilerMessage message)
        {
        }

        protected virtual void OnStateChange(StreamState newState, StreamState oldState)
        {
        }

        private void OnProcessExit() => Close();

        public virtual void Close()
        {
            if (State == StreamState.Closed)
            {
                return;
            }

            SetState(StreamState.Closed);

#if NET35
            _readThread.Join();
            _writeThread.Join();
#else
            _cts.Cancel();
            Task.WaitAll(_readTask, _writeTask);
#endif
            if (_compilerProcess != null)
            {
                if (!_compilerProcess.HasExited)
                {
                    _compilerProcess.Kill();
                }
                _compilerProcess.Dispose();
            }

            try
            {
                _input.Close();
                _input.Dispose();
            }
            catch(Exception) { }

            try
            {
                _output.Close();
                _output.Dispose();
            }
            catch(Exception) { }
        }
    }
}
