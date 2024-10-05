using Oxide.Pooling;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Oxide.IO.TransportMethods
{
    public class StandardInputOutput : ITransportProtocol
    {
        private const int INT_SIZE = sizeof(int);

        protected Stream Input { get; }

        protected Stream Output { get; }

        protected Encoding Encoding { get; }

        protected IArrayPoolProvider<byte> Pool { get; }
        private byte[] LengthBufferFrom { get; }

        public StandardInputOutput(Stream incoming, Stream outgoing, IArrayPoolProvider<byte> pool, Encoding encoding = null)
        {
            Input = incoming ?? throw new ArgumentNullException(nameof(incoming));
            Output = outgoing ?? throw new ArgumentNullException(nameof(outgoing));
            Encoding = encoding ?? Encoding.UTF8;
            Pool = pool ?? throw new ArgumentNullException(nameof(pool));
            LengthBufferFrom = new byte[INT_SIZE];
        }

        public string Receive()
        {
            int read = 0;

            while (read != INT_SIZE)
            {
                read += Input.Read(LengthBufferFrom, read, INT_SIZE - read);

                if (read == 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
            }

            int bodyLength = LengthBufferFrom.ReadInt(0);
            byte[] package = Pool.Take(bodyLength);
            try
            {
                read = 0;
                while (read != bodyLength)
                {
                    read += Input.Read(package, read, bodyLength - read);

                    if (read == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    }
                }

                string msg = Encoding.GetString(package, 0, read);
                return msg;
            }
            finally
            {
                Pool.Return(package);
            }
        }

        public void Send(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int length = Encoding.GetByteCount(data);
            byte[] payload = Pool.Take(length + INT_SIZE);
            try
            {
                int written = length.ToArray(payload, 0);
                written += Encoding.GetBytes(data, 0, data.Length, payload, 4);
                Output.Write(payload, 0, written);
                Output.Flush();
            }
            finally
            {
                Pool.Return(payload);
            }
        }
    }
}

