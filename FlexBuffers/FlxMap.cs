﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FlexBuffers
{
    public struct FlxMap : IEnumerable<KeyValuePair<string, FlxValue>>
    {
        private readonly byte[] _buffer;
        private readonly int _offset;
        private readonly int _length;
        private readonly byte _byteWidth;

        internal FlxMap(byte[] buffer, int offset, byte byteWidth, int length)
        {
            _buffer = buffer;
            _offset = offset;
            _byteWidth = byteWidth;
            _length = length;
        }

        public int Length => _length;

        private FlxVector Keys
        {
            get
            {
                var keysOffset = _offset - _byteWidth * 3;
                var indirectOffset = FlxValue.ComputeIndirectOffset(_buffer, keysOffset, _byteWidth);
                var bWidth = FlxValue.ReadLong(_buffer, keysOffset + _byteWidth, _byteWidth);
                return new FlxVector(_buffer, indirectOffset, (byte)bWidth, Type.VectorKey, _length);
            }
        }

        private FlxVector Values => new FlxVector(_buffer, _offset, _byteWidth, Type.Vector, _length);

        public FlxValue this[string key]
        {
            get
            {
                var index = KeyIndex(key);
                if (index < 0)
                {
                    throw new Exception($"No key '{key}' could be found");
                }
                return Values[index];
            }
        }

        public FlxValue ValueByIndex(int keyIndex)
        {
            if (keyIndex < 0 || keyIndex >= Length)
            {
                throw new Exception($"Bad Key index {keyIndex}");
            }

            return Values[keyIndex];
        }

        public string ToJson
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("{");
                var keys = Keys;
                var values = Values;
                for (var i = 0; i < _length; i++)
                {
                    builder.Append($"{keys[i].ToJson}:{values[i].ToJson}");
                    if (i < _length - 1)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("}");
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
            builder.Append("{\n");
            var keys = Keys;
            var values = Values;
            for (var i = 0; i < _length; i++)
            {
                builder.Append($"{left}  {keys[i].ToPrettyJson()} : {values[i].ToPrettyJson($"{left}  ", true)}");
                if (i < _length - 1)
                {
                    builder.Append(",");
                }

                builder.Append("\n");
            }
            builder.Append(left);
            builder.Append("}");
            return builder.ToString();
        }

        public int KeyIndex(string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var low = 0;
            var high = _length - 1;
            while (low <= high)
            {
                var mid = (high + low) >> 1;
                var dif = Comp(mid, keyBytes);
                if (dif == 0)
                {
                    return mid;
                }
                if (dif < 0)
                {
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }

            return -1;
        }

        private int Comp(int i, string key)
        {
            // TODO: keep it so we can profile it against byte comparison
            var key2 = Keys[i].AsString;
            return string.Compare(key, key2, StringComparison.Ordinal);
        }

        private int Comp(int i, byte[] key)
        {
            var key2 = Keys[i];
            var indirectOffset = key2.IndirectOffset;
            for (int j = 0; j < key.Length; j++)
            {
                var dif = key[j] - key2.Buffer[indirectOffset + j];
                if (dif != 0)
                {
                    return dif;
                }
            }
            // keys are zero terminated
            return key2.Buffer[indirectOffset + key.Length] == 0 ? 0 : -1;
        }

        public IEnumerator<KeyValuePair<string, FlxValue>> GetEnumerator()
        {
            var keys = Keys;
            var values = Values;
            for (var i = 0; i < _length; i++)
            {
                yield return new KeyValuePair<string, FlxValue>(keys[i].AsString, values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}