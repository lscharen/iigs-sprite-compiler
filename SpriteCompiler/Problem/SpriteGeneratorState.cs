namespace SpriteCompiler.Problem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SpriteGeneratorState
    {
        public SpriteGeneratorState()
            : this(new SpriteByte[0])
        {
        }

        public SpriteGeneratorState(byte[] data)
            : this(data.Select((x, i) => new SpriteByte(x, (ushort)i)))
        {
        }

        public SpriteGeneratorState(IEnumerable<SpriteByte> bytes)
        {
            Bytes = bytes.ToList();

            // Initialize the CPU state
            A = Register.INITIAL_OFFSET; // the address to draw the sprite is passed in, this is a run-time value

            X = Register.UNINITIALIZED; // the other registered are also undefined
            Y = Register.UNINITIALIZED;
            D = Register.UNINITIALIZED;
            S = Register.UNINITIALIZED;

            P = 0x30; // Start in native mode (16 bit A/X/Y) with the carry clear
        }

        private SpriteGeneratorState(SpriteGeneratorState other)
        {
            Bytes = new List<SpriteByte>(other.Bytes);
            A = other.A;
            X = other.X;
            Y = other.Y;
            D = other.D;
            S = other.S;
            P = other.P;
        }

        public void RemoveWord(ushort offset)
        {
            var total = Bytes.RemoveAll(x => x.Offset == offset || x.Offset == (offset + 1));
            if (total != 2)
            {
                throw new ArgumentException(string.Format("Cannot remove word at {0}", offset));
            }
        }

        public void RemoveByte(ushort offset)
        {
            var total = Bytes.RemoveAll(x => x.Offset == offset);
            if (total != 1)
            {
                throw new ArgumentException(string.Format("Cannot remove byte at {0}", offset));
            }
        }

        public SpriteGeneratorState Clone(Action<SpriteGeneratorState> f = null)
        {
            var other = new SpriteGeneratorState(this);

            if (f != null)
            {
                f(other);
            }

            return other;
        }

        public List<SpriteByte> Bytes { get; private set; }
        public bool IsEmpty { get { return Bytes.Count == 0; } }

        public bool LongA { get { return (P & 0x10) == 0x10; } }
        public bool LongI { get { return (P & 0x20) == 0x20; } }

        // Maintain the state of the execution
        public Register A { get; set; }  // Nullable because unknown values can be passed through the accumulator
        public Register X { get; set; }
        public Register Y { get; set; }
        public Register D { get; set; }
        public Register S { get; set; }  // S is always an offset, not a literal number

        public byte P { get; set; }
    }
}
