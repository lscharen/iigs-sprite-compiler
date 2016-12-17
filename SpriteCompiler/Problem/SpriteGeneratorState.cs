namespace SpriteCompiler.Problem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SpriteGeneratorState : IEquatable<SpriteGeneratorState>
    {
        // Single static reference to the original data set
        public static List<SpriteByte> DATASET = null;
        public static IDictionary<int, SpriteByte> DATASET_BY_OFFSET = null;

        // Histogram of the byte data
        public static IDictionary<byte, int> DATASET_SOLID_BYTES = null;

        // Histogram of all possible words -- includes overlaps, e.g. $11 $11 $11 $11 = ($1111, 3)
        public static IDictionary<ushort, int> DATASET_SOLID_WORDS = null;

        public static SpriteGeneratorState Init(IEnumerable<SpriteByte> bytes)
        {
            DATASET = bytes.OrderBy(x => x.Offset).ToList();
            DATASET_BY_OFFSET = DATASET.ToDictionary(x => (int)x.Offset, x => x);

            DATASET_SOLID_BYTES = DATASET
                .Where(x => x.Mask == 0x00)
                .GroupBy(x => x.Data)
                .ToDictionary(x => x.Key, x => x.Count())
                ;

            DATASET_SOLID_WORDS = DATASET
                .Zip(DATASET.Skip(1), (x, y) => new { Left = x, Right = y })
                .Where(x => (x.Left.Offset == (x.Right.Offset - 1)) && x.Left.Mask == 0x00 && x.Right.Mask == 0x00)
                .Select(x => (x.Right.Data << 8) | x.Left.Data)
                .GroupBy(x => x)
                .ToDictionary(x => (ushort)x.Key, x => x.Count())
                ;

            return new SpriteGeneratorState();
        }

        public static SpriteGeneratorState Init(IEnumerable<byte> data)
        {
            return Init(data.Select((x, i) => new SpriteByte(x, (ushort)i)));
        }

        public static SpriteGeneratorState Init(IEnumerable<byte> data, IEnumerable<byte> mask)
        {
            return Init(data.Zip(mask, (x, y) => new { Data = x, Mask = y })
                .Select((_, i) => new SpriteByte(_.Data, _.Mask, (ushort)i))
                .Where(_ => _.Mask != 0xFF));
        }

        public SpriteGeneratorState()
        {
            // The closed list contains all of the bytes that have been written
            Closed = new HashSet<ushort>();

            // Initialize the CPU state
            A = Register.INITIAL_OFFSET; // the address to draw the sprite is passed in, this is a run-time value

            X = Register.UNINITIALIZED; // the other registered are also undefined
            Y = Register.UNINITIALIZED;
            D = Register.UNINITIALIZED;
            S = Register.UNINITIALIZED;

            P = 0x30; // Start in native mode (16 bit A/X/Y) with the carry clear

            AllowModeChange = true;
        }

        private SpriteGeneratorState(SpriteGeneratorState other)
        {
            Closed = new HashSet<ushort>(other.Closed);
            A = other.A;
            X = other.X;
            Y = other.Y;
            D = other.D;
            S = other.S;
            P = other.P;
            AllowModeChange = other.AllowModeChange;
        }

        public override string ToString()
        {
            return String.Format("A = {0:X4}, X = {1}, Y = {2}, S = {3}, D = {4}, P = {5:X2}", A, X, Y, S, D, P);
        }

        public void RemoveWord(ushort offset)
        {
            Closed.Add(offset);
            Closed.Add((ushort)(offset + 1));
            AllowModeChange = true;
        }

        public void RemoveByte(ushort offset)
        {
            Closed.Add(offset);
            AllowModeChange = true;
        }

        public List<SpriteByte> RemainingBytes()
        {
            return DATASET
                .Where(x => !Closed.Contains(x.Offset))
                .ToList();                
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

        // A better state representation would be to have an array of offsets and a static 
        // data and mask array.  Then the state is just the locations and registers, rather
        // than a full copy of the data

        public ISet<ushort> Closed { get; private set; }
        public bool IsEmpty { get { return Closed.Count == DATASET.Count; } }

        public bool LongA { get { return (P & 0x10) == 0x10; } }
        public bool LongI { get { return (P & 0x20) == 0x20; } }

        // Maintain the state of the execution
        public Register A { get; set; }  // Nullable because unknown values can be passed through the accumulator
        public Register X { get; set; }
        public Register Y { get; set; }
        public Register D { get; set; }
        public Register S { get; set; }  // S is always an offset, not a literal number

        public byte P { get; set; }

        // Flag that is cleared whenever there is a switch from
        // 8/16-bit mode.  It is reset once a PHA or STA occurs.
        // A PEA instruction has no effect.  This gates allowable
        // state transition to prevent long REP/SEP seqences.
        public bool AllowModeChange { get; set; }

        public const byte LONG_A = 0x10;
        public const byte LONG_I = 0x20;

        public override bool Equals(object obj)
        {
            return Equals(obj as SpriteGeneratorState);
        }

        public bool Equals(SpriteGeneratorState other)
        {
            // Two states are equal if the bytes are the same and all registers are the same
            return Closed.SetEquals(other.Closed) &&
                A.Equals(other.A) &&
                X.Equals(other.X) &&
                Y.Equals(other.Y) &&
                D.Equals(other.D) &&
                S.Equals(other.S) &&
                P.Equals(other.P) &&
                AllowModeChange == other.AllowModeChange
                ;
        }

        public override int GetHashCode()
        {
            return
                A.GetHashCode() +
                X.GetHashCode() +
                Y.GetHashCode() +
                D.GetHashCode() +
                S.GetHashCode() +
                P.GetHashCode() +
                AllowModeChange.GetHashCode()
                ;
        }

        public static bool operator ==(SpriteGeneratorState state1, SpriteGeneratorState state2)
        {
            if (((object)state1) == null || ((object)state2) == null)
                return Object.Equals(state1, state2);

            return state1.Equals(state2);
        }

        public static bool operator !=(SpriteGeneratorState state1, SpriteGeneratorState state2)
        {
            if (((object)state1) == null || ((object)state2) == null)
                return !Object.Equals(state1, state2);

            return !(state1.Equals(state2));
        }
    }
}
