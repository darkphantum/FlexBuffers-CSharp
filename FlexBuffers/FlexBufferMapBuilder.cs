using System;

namespace FlexBuffers
{
    internal struct FlexBufferMapBuilder : IFlexBufferMapBuilder
    {
        private readonly FlexBuffer _buffer;

        internal FlexBufferMapBuilder(FlexBuffer buffer)
        {
            _buffer = buffer;
        }

        public void AddNull(string key)
        {
            _buffer.AddKey(key);
            _buffer.AddNull();
        }

        public void Add(string key, long value, bool indirect = false)
        {
            _buffer.AddKey(key);
            if (indirect)
            {
                _buffer.AddIndirect(value);
            }
            else
            {
                _buffer.Add(value);
            }
        }

        public void Add(string key, long x, long y)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, long x, long y, long z)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, long x, long y, long z, long w)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.Add(w);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, ulong value, bool indirect = false)
        {
            _buffer.AddKey(key);
            if (indirect)
            {
                _buffer.AddIndirect(value);
            }
            else
            {
                _buffer.Add(value);
            }
        }

        public void Add(string key, ulong x, ulong y)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, ulong x, ulong y, ulong z)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, ulong x, ulong y, ulong z, ulong w)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.Add(w);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, double value, bool indirect = false)
        {
            _buffer.AddKey(key);
            if (indirect)
            {
                _buffer.AddIndirect(value);
            }
            else
            {
                _buffer.Add(value);
            }
        }

        public void Add(string key, double x, double y)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, double x, double y, double z)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, double x, double y, double z, double w)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.Add(w);
            _buffer.EndVector(start, true, true);
        }

        public void Add(string key, bool value)
        {
            _buffer.AddKey(key);
            _buffer.Add(value);
        }

        public void Add(string key, string value)
        {
            _buffer.AddKey(key);
            _buffer.Add(value);
        }

        public void Add(string key, byte[] value)
        {
            _buffer.AddKey(key);
            _buffer.Add(value);
        }

        public void Map(string key, Action<IFlexBufferMapBuilder> map)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            var builder = new FlexBufferMapBuilder(_buffer);
            map(builder);
            _buffer.SortAndEndMap(start);
        }

        public void Vector(string key, Action<IFlexBufferVectorBuilder> vector)
        {
            _buffer.AddKey(key);
            var start = _buffer.StartVector();
            var builder = new FlexBufferVectorBuilder(_buffer);
            vector(builder);
            _buffer.EndVector(start, false, false);
        }
    }
}
