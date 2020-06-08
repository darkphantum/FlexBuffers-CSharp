using System;

namespace FlexBuffers
{
    public class FlexBufferBuilder
    {
        public static byte[] Map(Action<IFlexBufferMapBuilder> map)
        {
            var buffer = new FlexBuffer();
            var start = buffer.StartVector();
            var builder = new FlexBufferMapBuilder(buffer);
            map(builder);
            buffer.SortAndEndMap(start);
            return buffer.Finish();
        }
        
        public static byte[] Vector(Action<IFlexBufferVectorBuilder> vector)
        {
            var buffer = new FlexBuffer();
            var start = buffer.StartVector();
            var builder = new FlexBufferVectorBuilder(buffer);
            vector(builder);
            buffer.EndVector(start, false, false);
            return buffer.Finish();
        }
    }
}