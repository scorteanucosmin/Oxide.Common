using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Oxide.IO.TransportMethods
{
    public class StandardInputOutput : ITransportProtocol
    {
        protected Stream Input { get; }

        protected Stream Output { get; }

        protected Encoding Encoding { get; }

        public StandardInputOutput(Stream incoming, Stream outgoing, Encoding encoding = null)
        {
            Input = incoming ?? throw new ArgumentNullException(nameof(incoming));
            Output = outgoing ?? throw new ArgumentNullException(nameof(outgoing));
            Encoding = encoding ?? Encoding.UTF8;
        }

        public string Receive()
        {
            // TODO: Make memory efficient
            string payload = Reader.ReadLine();
            return payload;
        }

        public void Send(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // TODO: Make memory efficient
            byte[] payload = Encoding.GetBytes(data);
            int length = payload.Length;
            byte[] lengthPrefix = BitConverter.GetBytes(length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthPrefix);
            }

            Output.Write(lengthPrefix, 0, lengthPrefix.Length);
            Output.Write(payload, 0, payload.Length);
            Output.Flush();
        }
    }
}

