using System;

namespace FlexBuffers
{
    public interface IFlexBufferVectorBuilder
    {
        void AddNull();
        void Add(long value, bool indirect = false);
        void Add(long x, long y);
        void Add(long x, long y, long z);
        void Add(long x, long y, long z, long w);
        void Add(ulong value, bool indirect = false);
        void Add(ulong x, ulong y);
        void Add(ulong x, ulong y, ulong z);
        void Add(ulong x, ulong y, ulong z, ulong w);
        void Add(double value, bool indirect = false);
        void Add(double x, double y);
        void Add(double x, double y, double z);
        void Add(double x, double y, double z, double w);
        void Add(bool value);
        void Add(string value);
        void Add(byte[] value);
        void Map(Action<IFlexBufferMapBuilder> map);
        void Vector(Action<IFlexBufferVectorBuilder> vector);
    }
}
