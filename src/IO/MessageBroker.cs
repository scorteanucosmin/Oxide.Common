using Oxide.IO.Serialization;
using Oxide.IO.TransportMethods;

namespace Oxide.IO
{
    public class MessageBroker<TMessage>
    {
        /// <inheritdoc cref="ISerializer"/>
        protected ISerializer Serializer { get; }

        /// <inheritdoc cref="ITransportProtocol"/>
        protected ITransportProtocol Protocol { get; }

        public MessageBroker(ISerializer serializer, ITransportProtocol protocol)
        {
            Serializer = serializer;
            Protocol = protocol;
        }

        /// <summary>
        /// Send a message
        /// </summary>
        /// <typeparam name="T">The type to map to a string</typeparam>
        /// <param name="message">The message to send</param>
        public virtual void Send<T>(T message) where T : TMessage
        {
            string data = Serializer.Serialize<T>(message);
            Protocol.Send(data);
        }

        /// <summary>
        /// Receive a message
        /// </summary>
        /// <typeparam name="T">Type to map the string</typeparam>
        /// <returns>The message</returns>
        public virtual T Receive<T>() where T : TMessage
        {
            string data = Protocol.Receive();

            if (string.IsNullOrEmpty(data))
            {
                return default;
            }

            return Serializer.Deserialize<T>(data);
        }
    }
}
