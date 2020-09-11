using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexBuffers
{
    enum CachedType
    {
        None,
        Long,
        ULong,
        Double,
        String,
        Bool,
        Vector,
        Map
    }

    public class FlxValue
    {
        private readonly byte[] _buffer;
        private readonly int _offset;
        private readonly byte _parentWidth;
        private readonly byte _byteWidth;
        private readonly Type _type;

        // dont deal with unboxing
        private long _cachedLong;
        private ulong _cachedULong;
        private string _cachedString;
        private double _cachedDouble;
        private bool _cachedBool;
        private FlxMap _cachedMap;
        private FlxVector _cachedVector;

        private CachedType _cachedType = CachedType.None;

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
                if (_cachedType == CachedType.Long)
                {
                    return _cachedLong;
                }

                long val;
                if (_type == Type.FlexBlob)
                {
                    var bytes = this.AsFlexBlob;
                    val = long.Parse(Encoding.UTF8.GetString(bytes));
                }
                else if (_type == Type.Int)
                {
                    val = ReadLong(_buffer, _offset, _parentWidth);
                }
                else if (_type == Type.IndirectInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    val = ReadLong(_buffer, indirectOffset, _byteWidth);
                }
                else if (_type == Type.Uint)
                {
                    var value = ReadULong(_buffer, _offset, _parentWidth);
                    if (value <= long.MaxValue)
                    {
                        val = (long)value;
                    }
                    else
                    {
                        throw new Exception($"Type {_type} is not convertible to long");
                    }
                }
                else if (_type == Type.IndirectUInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var value = ReadULong(_buffer, indirectOffset, _byteWidth);
                    if (value <= long.MaxValue)
                    {
                        val = (long)value;
                    }
                    else
                    {
                        throw new Exception($"Type {_type} is not convertible to long");
                    }
                }
                else
                {
                    throw new Exception($"Type {_type} is not convertible to long");
                }

                _cachedType = CachedType.Long;
                _cachedLong = val;
                return val;
            }
        }
        
        public ulong AsULong
        {
            get
            {
                if (_cachedType == CachedType.ULong)
                {
                    return _cachedULong;
                }

                ulong val;
                if (_type == Type.FlexBlob)
                {
                    var bytes = this.AsFlexBlob;
                    val = ulong.Parse(Encoding.UTF8.GetString(bytes));
                }
                else if (_type == Type.Uint)
                {
                    val = ReadULong(_buffer, _offset, _parentWidth);
                }
                else if (_type == Type.IndirectUInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    val = ReadULong(_buffer, indirectOffset, _byteWidth);
                }
                else if (_type == Type.Int)
                {
                    var value = ReadLong(_buffer, _offset, _parentWidth);
                    if (value >= 0)
                    {
                        val = (ulong)value;
                    }
                    else
                    {
                        throw new Exception($"Type {_type} is not convertible to ulong");
                    }
                }
                else if (_type == Type.IndirectInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var value = ReadLong(_buffer, indirectOffset, _byteWidth);
                    if (value >= 0)
                    {
                        val = (ulong)value;
                    }
                    else
                    {
                        throw new Exception($"Type {_type} is not convertible to ulong");
                    }
                }
                else
                {
                    throw new Exception($"Type {_type} is not convertible to ulong");
                }

                _cachedULong = val;
                _cachedType = CachedType.ULong;
                return val;
            }
        }
        
        public double AsDouble
        {
            get
            {
                if (_cachedType == CachedType.Double)
                {
                    return _cachedDouble;
                }

                double val;
                if (_type == Type.FlexBlob)
                {
                    var bytes = this.AsFlexBlob;
                    val = double.Parse(Encoding.UTF8.GetString(bytes));
                }
                else if (_type == Type.Float)
                {
                    val = ReadDouble(_buffer, _offset, _parentWidth);
                }
                else if (_type == Type.Int)
                {
                    val = ReadLong(_buffer, _offset, _parentWidth);
                }
                else if (_type == Type.Uint)
                {
                    val = ReadULong(_buffer, _offset, _parentWidth);
                }
                else if (_type == Type.IndirectFloat)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    val = ReadDouble(_buffer, indirectOffset, _byteWidth);
                }
                else if (_type == Type.IndirectUInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    val = ReadULong(_buffer, indirectOffset, _byteWidth);
                }
                else if (_type == Type.IndirectInt)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    val = ReadLong(_buffer, indirectOffset, _byteWidth);
                }
                else
                {
                    throw new Exception($"Type {_type} is not convertible to double");
                }

                _cachedType = CachedType.Double;
                _cachedDouble = val;
                return val;
            }
        }
        
        public bool AsBool
        {
            get
            {
                if (_cachedType == CachedType.Bool)
                {
                    return _cachedBool;
                }

                bool val;
                if (_type == Type.Bool)
                {
                    val = _buffer[_offset] != 0;
                }
                else if (_type == Type.Int)
                {
                    val = ReadLong(_buffer, _offset, _parentWidth) != 0;
                }
                else if (_type == Type.Uint)
                {
                    val = ReadULong(_buffer, _offset, _parentWidth) != 0;
                }
                else
                {
                    throw new Exception($"Type {_type} is not convertible to bool");
                }

                _cachedBool = val;
                _cachedType = CachedType.Bool;
                return val;
            }
        }
        
        public string AsString
        {
            get
            {
                if (_cachedType == CachedType.String)
                {
                    return _cachedString;
                }


                string val;
                if (_type == Type.FlexStringBlob)
                {
                    var bytes = this.AsFlexBlob;
                    val = Encoding.UTF8.GetString(bytes);
                }
                else if (_type == Type.String)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var size = (int)ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                    var sizeWidth = (int)_byteWidth;
                    while (_buffer[indirectOffset + size] != 0)
                    {
                        sizeWidth <<= 1;
                        size = (int)ReadLong(_buffer, indirectOffset - sizeWidth, (byte)sizeWidth);
                    }

                    val = Encoding.UTF8.GetString(_buffer, indirectOffset, size);
                }
                else if (_type == Type.Key)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var size = 0;
                    while (indirectOffset + size < _buffer.Length && _buffer[indirectOffset + size] != 0)
                    {
                        size++;
                    }
                    val = Encoding.UTF8.GetString(_buffer, indirectOffset, size);
                }
                else
                {
                    throw new Exception($"Type {_type} is not convertible to string");
                }

                _cachedString = val;
                _cachedType = CachedType.String;
                return val;
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

                if (_cachedType == CachedType.Vector)
                {
                    return _cachedVector;
                }

                var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                var size = TypesUtil.IsFixedTypedVector(_type) 
                    ? TypesUtil.FixedTypedVectorElementSize(_type) 
                    : (int)ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                _cachedVector = new FlxVector(_buffer, indirectOffset, _byteWidth, _type, size);
                _cachedType = CachedType.Vector;
                return _cachedVector;
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

                if (_cachedType == CachedType.Map)
                {
                    return _cachedMap;
                }

                var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                var size = ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                _cachedMap = new FlxMap(_buffer, indirectOffset, _byteWidth, (int)size);
                _cachedType = CachedType.Map;
                return _cachedMap;
            }
        }

        public byte[] AsFlexBlob
        {
            get
            {
                if (_type == Type.Key)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var size = 0;
                    while (indirectOffset + size < _buffer.Length && _buffer[indirectOffset + size] != 0)
                    {
                        size++;
                    }

                    var blob = new byte[size];
                    System.Buffer.BlockCopy(_buffer, indirectOffset, blob, 0, (int)size);

                    return blob;
                }

                if (_type == Type.FlexBlob ||
                    _type == Type.FlexStringBlob)
                {
                    var indirectOffset = ComputeIndirectOffset(_buffer, _offset, _parentWidth);
                    var size = ReadLong(_buffer, indirectOffset - _byteWidth, _byteWidth);
                    var blob = new byte[size];
                    System.Buffer.BlockCopy(_buffer, indirectOffset, blob, 0, (int)size);
                    return blob;                    
                }

                throw new Exception($"Type {_type} is not a flexblob.");
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

        public async Task ConvertToJsonAsStreamAsync(Stream stream)
        {
            if (TypesUtil.IsAVector(_type))
            {
                await AsVector.ConvertToJsonAsStreamAsync(stream);
            }
            else if (_type == Type.Map)
            {
                await AsMap.ConvertToJsonAsStreamAsync(stream);
            }
            else if (_type == Type.Blob)
            {
                var base64Bytes = Encoding.UTF8.GetBytes($"\"{Convert.ToBase64String(AsBlob)}\"");
                await stream.WriteAsync(base64Bytes, 0, base64Bytes.Length);
            }
            else if (_type == Type.Bool)
            {
                var valueBytes = Encoding.UTF8.GetBytes(AsBool.ToString().ToLowerInvariant());
                await stream.WriteAsync(valueBytes, 0, valueBytes.Length);
            }
            else if (_type == Type.Null)
            {
                var valueBytes = Encoding.UTF8.GetBytes("null");
                await stream.WriteAsync(valueBytes, 0, valueBytes.Length);
            }
            else
            {
                var valueBytes = AsFlexBlob;
                if (_type == Type.FlexStringBlob || _type == Type.Key)
                {
                    var quotedBytes = new byte[valueBytes.Length + 2];
                    quotedBytes[0] = (byte)'"';
                    quotedBytes[quotedBytes.Length - 1] = (byte)'"';
                    System.Buffer.BlockCopy(valueBytes, 0, quotedBytes, 1, valueBytes.Length);
                    await stream.WriteAsync(quotedBytes, 0, quotedBytes.Length);
                }
                else
                {
                    await stream.WriteAsync(valueBytes, 0, valueBytes.Length);
                }
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