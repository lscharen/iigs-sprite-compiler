namespace SpriteCompiler.Problem
{
    using System;

    public sealed class Register
    {
        public static readonly Register UNINITIALIZED = new Register(0, DataType.UNINITIALIZED);
        public static readonly Register INITIAL_OFFSET = new Register(0, DataType.SCREEN_OFFSET);

        public enum DataType
        {
            UNINITIALIZED,
            SCREEN_OFFSET,
            LITERAL
        }

        private Register(int value, DataType tag)
        {
            Value = value;
            Tag = tag;
        }

        public Register Clone()
        {
            return new Register(Value, Tag);
        }

        public Register Add(int offset)
        {
            if (IsUninitialized)
            {
                throw new ArgumentException("Cannot add value to uninitialized registers");
            }

            // Adding a value does not change the tag
            return new Register(Value + offset, Tag);
        }

        public Register LoadConstant(int value)
        {
            return new Register(value, DataType.LITERAL);
        }

        public bool IsUninitialized { get { return DataType.UNINITIALIZED.Equals(Tag); } }
        public bool IsScreenOffset { get { return DataType.SCREEN_OFFSET.Equals(Tag); } }
        public bool IsLiteral { get { return DataType.LITERAL.Equals(Tag); } }

        public DataType Tag { get; private set; }
        public int Value { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Tag, Value.ToString("X4"));
        }
    }
}
