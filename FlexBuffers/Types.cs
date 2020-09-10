using System;

namespace FlexBuffers
{
    public enum Type: byte
    {
        Null, Int, Uint, Float,
        Key, String, IndirectInt, IndirectUInt, IndirectFloat,
        Map, Vector, VectorInt, VectorUInt, VectorFloat, VectorKey, VectorString,
        VectorInt2, VectorUInt2, VectorFloat2,
        VectorInt3, VectorUInt3, VectorFloat3,
        VectorInt4, VectorUInt4, VectorFloat4,
        Blob, Bool, VectorBool = 36, 
        FlexBlob = 50,
        FlexStringBlob
    }
}