#if CONTROLLERS_PROFILER
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Playtika.Controllers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ByteBlock16
    {
        public byte B0; public byte B1; public byte B2; public byte B3;
        public byte B4; public byte B5; public byte B6; public byte B7;
        public byte B8; public byte B9; public byte B10; public byte B11;
        public byte B12; public byte B13; public byte B14; public byte B15;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FixedString
    {
        public ByteBlock16 Block0;
        public ByteBlock16 Block1;
        public ByteBlock16 Block2;
        public ByteBlock16 Block3;
        public ByteBlock16 Block4;
        public ByteBlock16 Block5;
        public ByteBlock16 Block6;
        public ByteBlock16 Block7;

        private Span<byte> AsSpan()
        {
            return MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Block0, 8));
        }

        public override string ToString()
        {
            var span = AsSpan();
            var length = span.IndexOf((byte)0);
            if (length < 0)
            {
                length = 128;
            }

            return Encoding.UTF8.GetString(span.Slice(0, length));
        }

        private void Set(string value)
        {
            var span = AsSpan();
            span.Clear();

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            var len = Math.Min(bytes.Length, 127);
            bytes.AsSpan(0, len).CopyTo(span);
        }

        public static implicit operator FixedString(string value)
        {
            var result = new FixedString();
            result.Set(value);
            return result;
        }

        public static implicit operator string(FixedString value)
        {
            return value.ToString();
        }
    }
}
#endif