using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.Problem
{
    public struct SpriteByte
    {
        private static byte DataToMask(byte data)
        {
            return (byte)((((data & 0xF0) == 0x00) ? 0xF0 : 0x00) | (((data & 0x0F) == 0x00) ? 0x0F : 0x00));
        }

        public SpriteByte(byte data, ushort offset)
            : this(data, DataToMask(data), offset)
        {
        }

        public SpriteByte(byte data, byte mask, ushort offset)
        {
            Data = data;
            Mask = mask;
            Offset = offset;
        }

        public byte Data { get; private set; }
        public byte Mask { get; private set; }
        public ushort Offset { get; private set; }
    }
}
