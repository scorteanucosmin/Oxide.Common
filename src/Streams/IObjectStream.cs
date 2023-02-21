using System;

namespace Oxide.Streams
{
    public interface IObjectStream<TObject> where TObject : class
    {
        /// <remarks>
        /// <para>First <see cref="StreamState"/> is the new state</para>
        /// <para>Second <see cref="StreamState"/> is the old state</para>
        /// </remarks>
        event Action<IObjectStream<TObject>, StreamState, StreamState> OnStateChanged;

        event Action<IObjectStream<TObject>, TObject> OnMessage;

        bool CanRead { get; }

        bool CanWrite { get; }

        StreamState State { get; }

        void PushMessage(TObject graph);

        void Close();
    }
}
