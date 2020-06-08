using System;
using System.Globalization;
using System.Text;

namespace FlexBuffers
{
    public struct FlxValue
    {
        private readonly byte[] _buffer;
        private readonly int _offset;
        private readonly byte _parentWidth;
        private readonly byte _byteWidth;
        private readonly Type _type;

        internal FlxValue(byte[] buffer, int offset, byte parentWidth, byte packedType)
        {
            _buffer = buffer;
            _offset = offset;
            _parentWidth = parentWidth;
            _byteWidth = (byte) (1 << (packedType & 3));
            _type = (Type) (packedType >> 2);
        }
        
        internal FlxValue(byte[] buffer, int offset, byte parentWidth, byte byteWidth, Type type)
        {
            _buffer = buffer;
            _offset = offset;
            _parentWidth = parentWidth;
            _byteWidth = byteWidth;
            _type = type;
        }

        public static FlxValue FromBytes(byte[] bytes)
        {
            if (bytes.Length < 3)
            {
                throw new Exception($"Invalid buffer {bytes}");
            }

            var byteWidth = bytes[bytes.Length - 1];
            var packedType = bytes[bytes.Length - 2];
            var offset = bytes.Length - byteWidth - 2;
            return new FlxValue(bytes, offset, byteWidth, packedType);
        }

        public Type ValueType => _type;
        public int BufferOffset => _offset;

        public bool IsNull => _type == Type.Null;

        public long AsLong
        {
            get
            {
                if (_type == Type.Int)
                {
                    return ReadLong(_buffer, _offset, _parentWidth);    
                }

                if (_type == Type.IndirectInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    return ReadLong(_buffer, indirectOffset, _byteWidth);
                }

                if (_type == Type.Uint)
                {
                    var value = ReadULong(_buffer, _offset, _parentWidth);
                    if (value <= long.MaxValue)
                    {
                        return (long) value;
                    }
                }
                if (_type == Type.IndirectUInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var value = ReadULong(_buffer, indirectOffset, _byteWidth);
                    if (value <= long.MaxValue)
                    {
                        return (long) value;
                    }
                }
                throw new Exception($"Type {_type} is not convertible to long");
            }
        }
        
        public ulong AsULong
        {
            get
            {
                if (_type == Type.Uint)
                {
                    return ReadULong(_buffer, _offset, _parentWidth);    
                }
                
                if (_type == Type.IndirectUInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    return ReadULong(_buffer, indirectOffset, _byteWidth);
                }

                if (_type == Type.Int)
                {
                    var value = ReadLong(_buffer, _offset, _parentWidth);
                    if (value >= 0)
                    {
                        return (ulong) value;
                    }
                }
                
                if (_type == Type.IndirectInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var value = ReadLong(_buffer, indirectOffset, _byteWidth);
                    if (value >= 0)
                    {
                        return (ulong) value;
                    }
                }
                throw new Exception($"Type {_type} is not convertible to ulong");
            }
        }
        
        public double AsDouble
        {
            get
            {
                if (_type == Type.Float)
                {
                    return ReadDouble(_buffer, _offset, _parentWidth);    
                }
                if (_type == Type.Int)
                {
                    return ReadLong(_buffer, _offset, _parentWidth);    
                }
                if (_type == Type.Uint)
                {
                    return ReadULong(_buffer, _offset, _parentWidth);    
                }
                if (_type == Type.IndirectFloat)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    return ReadDouble(_buffer, indirectOffset, _byteWidth);
                }
                if (_type == Type.IndirectUInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    return ReadULong(_buffer, indirectOffset, _byteWidth);
                }
                if (_type == Type.IndirectInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    return ReadLong(_buffer, indirectOffset, _byteWidth);
                }
                throw new Exception($"Type {_type} is not convertible to double");
            }
        }
        
        public bool AsBool
        {
            get
            {
                if (_type == Type.Bool)
                {
                    return _buffer[_offset] != 0;
                }
                if (_type == Type.Int)
                {
                    return ReadLong(_buffer, _offset, _parentWidth) != 0;    
                }
                if (_type == Type.Uint)
                {
                    return ReadULong(_buffer, _offset, _parentWidth) != 0;    
                }
                throw new Exception($"Type {_type} is not convertible to bool");
            }
        }
        
        public string AsString
        {
            get
            {
                if (_type == Type.String)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var size = (int)ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                    var sizeWidth = (int)_byteWidth;
                    while (_buffer[indirectOffset + size] != 0)
                    {
                        sizeWidth <<= 1;
                        size = (int)ReadLong(_buffer, indirectOffset - sizeWidth, (byte)sizeWidth);
                    }
                    
                    return Encoding.UTF8.GetString(_buffer, indirectOffset, size);
                }

                if (_type == Type.Key)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var size = 0;
                    while (indirectOffset + size < _buffer.Length && _buffer[indirectOffset + size] != 0)
                    {
                        size++;
                    }
                    return Encoding.UTF8.GetString(_buffer, indirectOffset, size);
                }
                
                throw new Exception($"Type {_type} is not convertible to string");
            }
        }
        
        public FlxValue this[int index] => AsVector[index];
        
        public FlxValue this[string key] => AsMap[key];

        public FlxVector AsVector
        {
            get
            {
                if (TypesUtil.IsAVector(_type) == false)
                {
                    throw new Exception($"Type {_type} is not a vector.");
                }

                var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                var size = TypesUtil.IsFixedTypedVector(_type) 
                    ? TypesUtil.FixedTypedVectorElementSize(_type) 
                    : (int)ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                return new FlxVector(_buffer, indirectOffset, _byteWidth, _type, size);
            }
        }

        public FlxMap AsMap
        {
            get
            {
                if (_type != Type.Map)
                {
                    throw new Exception($"Type {_type} is not a map.");
                }

                var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                var size = ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                return new FlxMap(_buffer, indirectOffset, _byteWidth, (int)size);
            }
        }

        public byte[] AsBlob
        {
            get
            {
                if (_type != Type.Blob)
                {
                    throw new Exception($"Type {_type} is not a blob.");
                }
                var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                var size = ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                var blob = new byte[size];
                System.Buffer.BlockCopy(_buffer, indirectOffset, blob, 0, (int)size);
                return blob;
            }
        }

        public string ToJson
        {
            get
            {
                if (IsNull)
                {
                    return "null";
                }

                if (_type == Type.Bool)
                {
                    return AsBool ? "true" : "false";
                }

                if (_type == Type.Int || _type == Type.IndirectInt)
                {
                    return AsLong.ToString();
                }

                if (_type == Type.Uint || _type == Type.IndirectUInt)
                {
                    return AsULong.ToString();
                }

                if (_type == Type.Float || _type == Type.IndirectFloat)
                {
                    return AsDouble.ToString(CultureInfo.CurrentCulture);
                }

                if (TypesUtil.IsAVector(_type))
                {
                    return AsVector.ToJson;
                }

                if (_type == Type.String || _type == Type.Key)
                {
                    var jsonConformString = AsString.Replace("\"", "\\\"")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t")
                        .Replace("/", "\\/");
                    return $"\"{jsonConformString}\"";
                }

                if (_type == Type.Map)
                {
                    return AsMap.ToJson;
                }

                if (_type == Type.Blob)
                {
                    return $"\"{Convert.ToBase64String(AsBlob)}\"";
                }
                
                throw new Exception($"Unexpected type {_type}");
            }
        }

        public string ToPrettyJson(string left = "", bool childrenOnly = false)
        {
            if (_type == Type.Map)
            {
                return AsMap.ToPrettyJson(left, childrenOnly);
            }
            if (TypesUtil.IsAVector(_type))
            {
                return AsVector.ToPrettyJson(left, childrenOnly);
            }

            if (childrenOnly)
            {
                return ToJson;
            }
            
            return $"{left}{ToJson}";
        }

        internal static long ReadLong(byte[] bytes, int offset, byte width)
        {
            if (offset < 0 || bytes.Length <= (offset + width) || (offset & (width - 1)) != 0)
            {
                throw new Exception("Bad offset");
            }

            if (width == 1)
            {
                return (sbyte)bytes[offset];
            }

            if (width == 2)
            {
                return BitConverter.ToInt16(bytes, offset);
            }

            if (width == 4)
            {
                return BitConverter.ToInt32(bytes, offset);
            }

            return BitConverter.ToInt64(bytes, offset);
        }
        
        internal static ulong ReadULong(byte[] bytes, int offset, byte width)
        {
            if (offset < 0 || bytes.Length <= (offset + width) || (offset & (width - 1)) != 0)
            {
                throw new Exception("Bad offset");
            }

            if (width == 1)
            {
                return bytes[offset];
            }

            if (width == 2)
            {
                return BitConverter.ToUInt16(bytes, offset);
            }

            if (width == 4)
            {
                return BitConverter.ToUInt32(bytes, offset);
            }

            return BitConverter.ToUInt64(bytes, offset);
        }
        
        internal static double ReadDouble(byte[] bytes, int offset, byte width)
        {
            if (offset < 0 || bytes.Length <= (offset + width) || (offset & (width - 1)) != 0)
            {
                throw new Exception("Bad offset");
            }

            if (width != 4 && width != 8)
            {
                throw new Exception($"Bad width {width}");
            }

            if (width == 4)
            {
                return BitConverter.ToSingle(bytes, offset);
            }

            return BitConverter.ToDouble(bytes, offset);
        }

        internal static int ComputeIndirectOffset(byte[] bytes, int offset, byte width)
        {
            var step = (int)ReadLong(bytes, offset, width);
            return offset - step;
        }
        
        internal byte[] Buffer => _buffer;
        internal int Offset => _offset;

        internal int IndirectOffset => ComputeIndirectOffset(_buffer, _offset, _parentWidth);
    }
}