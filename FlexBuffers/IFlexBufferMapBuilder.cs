using System;

namespace FlexBuffers
{
    public interface IFlexBufferMapBuilder
    {
        void AddNull(string key);
        void Add(string key, long value, bool indirect = false);
        void Add(string key, long x, long y);
        void Add(string key, long x, long y, long z);
        void Add(string key, long x, long y, long z, long w);
        void Add(string key, ulong value, bool indirect = false);
        void Add(string key, ulong x, ulong y);
        void Add(string key, ulong x, ulong y, ulong z);
        void Add(string key, ulong x, ulong y, ulong z, ulong w);
        void Add(string key, double value, bool indirect = false);
        void Add(string key, double x, double y);
        void Add(string key, double x, double y, double z);
        void Add(string key, double x, double y, double z, double w);
        void Add(string key, bool value);
        void Add(string key, string value);
        void Add(string key, byte[] value);
        void Map(string key, Action<IFlexBufferMapBuilder> map);
        void Vector(string key, Action<IFlexBufferVectorBuilder> vector);
    }
}
