using System;

namespace Oxide.IO.Serialization.Binary
{
    public sealed class DateTimeTypeSerializer : IBinaryWriter<DateTime>, IBinaryReader<DateTime>
    {
        private PrimitiveTypeSerializer<long> LongTypeSerializer { get; }

        public DateTimeTypeSerializer(PrimitiveTypeSerializer<long> longSerializer)
        {
            LongTypeSerializer = longSerializer;
        }

        public bool CanRead(ReadOnlySpan<byte> buffer) => LongTypeSerializer.CanRead(buffer);

        public bool CanWrite(DateTime instance, ReadOnlySpan<byte> buffer) => LongTypeSerializer.CanWrite(instance.ToBinary(), buffer);

        public int Read(ReadOnlySpan<byte> buffer, ref DateTime instance)
        {
            long dt = 0;
            int read = LongTypeSerializer.Read(buffer, ref dt);
            instance = DateTime.FromBinary(dt);
            return read;
        }

        public int Write(DateTime instance, Span<byte> buffer)
        {
            long dt = instance.ToBinary();
            return LongTypeSerializer.Write(dt, buffer);
        }
    }
}
