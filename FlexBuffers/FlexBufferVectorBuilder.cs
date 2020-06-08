using System;

namespace FlexBuffers
{
    internal struct FlexBufferVectorBuilder : IFlexBufferVectorBuilder
    {
        private readonly FlexBuffer _buffer;

        internal FlexBufferVectorBuilder(FlexBuffer buffer)
        {
            _buffer = buffer;
        }

        public void AddNull()
        {
            _buffer.AddNull();
        }

        public void Add(long value, bool indirect = false)
        {
            if (indirect)
            {
                _buffer.AddIndirect(value);
            }
            else
            {
                _buffer.Add(value);
            }
        }

        public void Add(long x, long y)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.EndVector(start, true, true);
        }

        public void Add(long x, long y, long z)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.EndVector(start, true, true);
        }

        public void Add(long x, long y, long z, long w)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.Add(w);
            _buffer.EndVector(start, true, true);
        }

        public void Add(ulong value, bool indirect = false)
        {
            if (indirect)
            {
                _buffer.AddIndirect(value);
            }
            else
            {
                _buffer.Add(value);
            }
        }

        public void Add(ulong x, ulong y)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.EndVector(start, true, true);
        }

        public void Add(ulong x, ulong y, ulong z)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.EndVector(start, true, true);
        }

        public void Add(ulong x, ulong y, ulong z, ulong w)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.Add(w);
            _buffer.EndVector(start, true, true);
        }

        public void Add(double value, bool indirect = false)
        {
            if (indirect)
            {
                _buffer.AddIndirect(value);
            }
            else
            {
                _buffer.Add(value);
            }
        }

        public void Add(double x, double y)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.EndVector(start, true, true);
        }

        public void Add(double x, double y, double z)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.EndVector(start, true, true);
        }

        public void Add(double x, double y, double z, double w)
        {
            var start = _buffer.StartVector();
            _buffer.Add(x);
            _buffer.Add(y);
            _buffer.Add(z);
            _buffer.Add(w);
            _buffer.EndVector(start, true, true);
        }

        public void Add(bool value)
        {
            _buffer.Add(value);
        }

        public void Add(string value)
        {
            _buffer.Add(value);
        }

        public void Add(byte[] value)
        {
            _buffer.Add(value);
        }

        public void Map(Action<IFlexBufferMapBuilder> map)
        {
            var start = _buffer.StartVector();
            var builder = new FlexBufferMapBuilder(_buffer);
            map(builder);
            _buffer.SortAndEndMap(start);
        }

        public void Vector(Action<IFlexBufferVectorBuilder> vector)
        {
            var start = _buffer.StartVector();
            var builder = new FlexBufferVectorBuilder(_buffer);
            vector(builder);
            _buffer.EndVector(start, false, false);
        }
    }
}
