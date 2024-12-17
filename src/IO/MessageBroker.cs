using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Oxide.IO.Serialization;
using Oxide.IO.TransportMethods;
using Oxide.Pooling;

namespace Oxide.IO
{
    public sealed class MessageBroker<TMessage> : IDisposable
    {
        public event Action<TMessage> OnMessageReceived;

        private ITransportReceiver Receiever { get; }

        private ITransportTransmitter Transmitter { get; }

        private ISerializer<byte[], TMessage> Serializer { get; }

        private IDeserializer<TMessage, byte[]> Deserializer { get; }

        private IArrayPool<byte> Pool { get; }

        private Queue<TMessage> MessageQueue { get; }

        private Thread WorkerThread { get; }

        private bool Disposed { get; set; }

        private volatile bool running;

        public MessageBroker(ITransportTransmitter transmitter, ITransportReceiver receiver, ISerializer<byte[], TMessage> serializer, IDeserializer<TMessage, byte[]> deserializer, IArrayPool<byte> pool = null)
        {
            Transmitter = transmitter ?? throw new ArgumentNullException(nameof(transmitter));
            Receiever = receiver ?? throw new ArgumentNullException(nameof(receiver));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            Deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            Pool = pool ?? ArrayPool<byte>.Shared;
            MessageQueue = new Queue<TMessage>();
            running = true;
            WorkerThread = new Thread(WorkerMethod)
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                Name = $"{GetType().FullName}+{Environment.TickCount}",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            WorkerThread.Start();
        }

        public void SendMessage(TMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            lock (MessageQueue)
            {
                MessageQueue.Enqueue(message);
            }
        }

        private void WriteMessage(TMessage message)
        {
            Span<byte> data = new Span<byte>()
            byte[] data = Serializer.Serialize(message);
            byte[] payload = Pool.Take(data.Length + sizeof(int));

            try
            {
                int startPos = data.Length.WriteBigEndian(payload, 0);
                Array.Copy(data, 0, payload, startPos, data.Length);
                Transmitter.Write(payload, 0, payload.Length);
            }
            finally
            {
                Pool.Return(payload);
            }
        }

        private TMessage ReadMessage()
        {
            byte[] lengthBuffer = Pool.Take(sizeof(int));
            int read = 0;
            try
            {
                while (read < lengthBuffer.Length)
                {
                    read += Receiever.Read(lengthBuffer, read, lengthBuffer.Length - read);

                    if (read == 0)
                    {
                        return default;
                    }
                }

                int length = lengthBuffer.ReadBigEndian(0);
                byte[] buffer = Pool.Take(length);
                read = 0;
                try
                {
                    while (read < length)
                    {
                        read += Receiever.Read(buffer, read, length - read);
                    }

                    return Deserializer.Deserialize(buffer);
                }
                finally
                {
                    Pool.Return(buffer);
                }
            }
            finally
            {
                Pool.Return(lengthBuffer);
            }
        }

        private void WorkerMethod()
        {
            while (running)
            {
                bool ranOperation = false;

                lock (MessageQueue)
                {
                    try
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (MessageQueue.Count == 0)
                            {
                                break;
                            }

                            TMessage message = MessageQueue.Dequeue();
                            WriteMessage(message);
                            ranOperation = true;
                        }
                    }
                    catch
                    {
                        // TODO: Log Exception
                    }
                }


                if (OnMessageReceived != null)
                {
                    try
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            TMessage message = ReadMessage();

                            if (message == null)
                            {
                                break;
                            }

                            OnMessageReceived(message);
                            ranOperation = true;
                        }
                    }
                    catch
                    {
                        // TODO: Log Exception
                    }
                }

                if (!ranOperation)
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                running = false;
                Disposed = true;
                if (disposing)
                {
                    MessageQueue.Clear();
                    if (Receiever is IDisposable rDispose)
                    {
                        rDispose.Dispose();
                    }

                    if (Transmitter is IDisposable tDispose)
                    {
                        tDispose.Dispose();
                    }

                    if (Serializer is IDisposable sDispose)
                    {
                        sDispose.Dispose();
                    }

                    if (Deserializer is IDisposable dDispose)
                    {
                        dDispose.Dispose();
                    }
                }

                try
                {
                    if (!WorkerThread.Join(10000))
                    {
                        WorkerThread.Abort();
                    }
                }
                catch
                {
                }
            }
        }

        ~MessageBroker() => Dispose(disposing: false);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
