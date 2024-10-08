using System;
using System.IO;

namespace Oxide.IO.TransportMethods
{
    public class StreamTransportProtocol : ITransportTransmitter, ITransportReceiver, IDisposable
    {
        #region Default Settings

        public const int DEFAULT_MAXBUFFERSIZE = 1024;

        #endregion

        #region Properties

        protected Stream Stream { get; }

        protected int MaxBufferSize { get; }

        protected bool Disposed { get; private set; }

        protected virtual bool CanRead => Stream.CanRead;

        protected virtual bool CanWrite => Stream.CanWrite;

        protected virtual object ReadLock => Stream;

        protected virtual object WriteLock => Stream;

        #endregion

        public StreamTransportProtocol(Stream stream, int maxBufferSize = DEFAULT_MAXBUFFERSIZE)
        {
            Stream = stream;
            MaxBufferSize = maxBufferSize;
            Disposed = false;
        }

        ~StreamTransportProtocol() => Dispose(false);

        public void Write(byte[] buffer, int index, int count)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (!CanWrite)
            {
                throw new InvalidOperationException("Underlying stream does not allow writing");
            }

            Validate(buffer, index, count);

            lock (WriteLock)
            {
                int remaining = count;
                int written = 0;
                while (remaining > 0)
                {
                    int toWrite = Math.Min(MaxBufferSize, remaining);
                    OnWrite(buffer, index + written, toWrite);
                    remaining -= toWrite;
                    written += toWrite;
                    Stream.Flush();
                }
            }
        }

        public int Read(byte[] buffer, int index, int count)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (!CanRead)
            {
                throw new InvalidOperationException("Underlying stream does not allow reading");
            }

            Validate(buffer, index, count);

            int read = 0;
            lock (ReadLock)
            {
                int remaining = count;
                while (remaining > 0)
                {
                    int toRead = Math.Min(MaxBufferSize, remaining);
                    int r = OnRead(buffer, index + read, toRead);

                    if (r == 0 && read == 0)
                    {
                        return 0;
                    }

                    read += r;
                    remaining -= r;
                }
            }
            return read;
        }

        protected virtual int OnRead(byte[] buffer, int index, int count) => Stream.Read(buffer, index, count);

        protected virtual void OnWrite(byte[] buffer, int index, int count) => Stream.Write(buffer, index, count);

        private static void Validate(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Value must be zero or greater");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Value must be zero or greater");
            }

            if (index + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(index)} + {nameof(count)}", "Attempted to read more than buffer can allow");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Stream.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
