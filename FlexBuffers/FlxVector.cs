using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexBuffers
{
    public struct FlxVector : IEnumerable<FlxValue>
    {
        private readonly byte[] _buffer;
        private readonly int _offset;
        private readonly int _length;
        private readonly byte _byteWidth;
        private readonly Type _type;

        internal FlxVector(byte[] buffer, int offset, byte byteWidth, Type type, int length)
        {
            _buffer = buffer;
            _offset = offset;
            _byteWidth = byteWidth;
            _type = type;
            _length = length;
        }

        public int Length => _length;

        public FlxValue this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                {
                    throw new Exception($"Bad index {index}, should be 0...{_length}");
                }

                if (TypesUtil.IsTypedVector(_type))
                {
                    var elemOffset = _offset + (index * _byteWidth);
                    return new FlxValue(_buffer, elemOffset, _byteWidth, 1, TypesUtil.TypedVectorElementType(_type));
                }

                if (TypesUtil.IsFixedTypedVector(_type))
                {
                    var elemOffset = _offset + (index * _byteWidth);
                    return new FlxValue(_buffer, elemOffset, _byteWidth, 1, TypesUtil.FixedTypedVectorElementType(_type));
                }

                if (_type == Type.Vector)
                {
                    var packedType = _buffer[_offset + _length * _byteWidth + index];
                    var elemOffset = _offset + (index * _byteWidth);
                    return new FlxValue(_buffer, elemOffset, _byteWidth, packedType);
                }
                throw new Exception($"Bad index {index}, should be 0...{_length}");
            }
        }

        public async Task ConvertToJsonAsStreamAsync(Stream stream)
        {
            await stream.WriteAsync(new byte[] { (byte)'[' }, 0, 1);
            for (var i = 0; i < _length; i++)
            {
                await this[i].ConvertToJsonAsStreamAsync(stream);
                if (i < _length - 1)
                {
                    await stream.WriteAsync(new byte[] { (byte)',' }, 0, 1);
                }
            }

            await stream.WriteAsync(new byte[] { (byte)']' }, 0, 1);
        }

        public string ToJson
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("[");
                for (var i = 0; i < _length; i++)
                {
                    builder.Append(this[i].ToJson);
                    if (i < _length - 1)
                    {
                        builder.Append(",");
                    }
                }

                builder.Append("]");

                return builder.ToString();
            }
        }

        public string ToPrettyJson(string left = "", bool childrenOnly = false)
        {
            var builder = new StringBuilder();
            if (childrenOnly == false)
            {
                builder.Append(left);
            }

            builder.Append("[\n");
            for (var i = 0; i < _length; i++)
            {
                builder.Append(this[i].ToPrettyJson($"{left}  "));
                if (i < _length - 1)
                {
                    builder.Append(",");
                }

                builder.Append("\n");
            }
            builder.Append(left);
            builder.Append("]");

            return builder.ToString();
        }

        public IEnumerator<FlxValue> GetEnumerator()
        {
            for (var i = 0; i < _length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
