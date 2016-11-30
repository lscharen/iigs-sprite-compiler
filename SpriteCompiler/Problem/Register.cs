namespace SpriteCompiler.Problem
{
    using System;

    public sealed class Register : IEquatable<Register>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as Register);
        }

        public bool Equals(Register other)
        {
            if (other == null)
                return false;

            // Unititialized is equal to unititialized. Otherwise, compare the value, too.
            return Tag.Equals(other.Tag) && (Tag.Equals(DataType.UNINITIALIZED) || Value == other.Value);
        }

        public static bool operator ==(Register reg1, Register reg2)
        {
            if (((object)reg1) == null || ((object)reg2) == null)
                return Object.Equals(reg1, reg2);

            return reg1.Equals(reg2);
        }

        public static bool operator !=(Register reg1, Register reg2)
        {
            if (((object)reg1) == null || ((object)reg2) == null)
                return ! Object.Equals(reg1, reg2);

            return ! (reg1.Equals(reg2));
        }
    }
}
