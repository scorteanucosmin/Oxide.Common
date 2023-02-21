using System;
using System.Collections.Generic;

namespace Oxide.CompilerServices.Messages
{
    [Serializable]
    public abstract class CompilerMessage
    {
        public Guid Id { get; }

        public MessageType Type { get; }

        public List<string> Messages { get; } = new List<string>();

        protected CompilerMessage(MessageType type) : this(Guid.NewGuid(), type)
        {
        }

        protected CompilerMessage(Guid id, MessageType type)
        {
            Id = id;
            Type = type;
        }
    }

    [Serializable]
    public abstract class CompilerMessage<TData> : CompilerMessage
    {
        public TData Data { get; }

        protected CompilerMessage(Guid id, MessageType type) : base(id, type)
        {
            Data = (TData)Activator.CreateInstance(typeof(TData));
        }

        protected CompilerMessage(MessageType type) : this(Guid.NewGuid(), type)
        {
        }

        protected CompilerMessage(MessageType type, TData data) : base(type)
        {
            Data = data;
        }

        protected CompilerMessage(Guid id, MessageType type, TData data) : base(id, type)
        {
            Data = data;
        }
    }
}
