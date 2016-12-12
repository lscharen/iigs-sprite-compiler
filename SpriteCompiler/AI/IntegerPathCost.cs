using System;

namespace SpriteCompiler.AI
{
    /// <summary>
    /// Helper class to implement simple integer path costs
    /// </summary>
    public sealed class IntegerCost : ICost<IntegerCost>
    {
        private readonly int value;

        public IntegerCost()
            : this(0)
        {
        }

        private IntegerCost(int value)
        {
            this.value = value;
        }

        public static implicit operator int(IntegerCost obj)
        {
            return obj.value;
        }

        public static implicit operator IntegerCost(int value)
        {
            return new IntegerCost(value);
        }

        public IntegerCost Add(IntegerCost other)
        {
            return value + other.value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return value.Equals(((IntegerCost)obj).value);
        }

        public int CompareTo(IntegerCost other)
        {
            return value.CompareTo(other.value);
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public IntegerCost Zero()
        {
            return 0;
        }

        public IntegerCost One()
        {
            return 1;
        }

        public IntegerCost Maximum()
        {
            return int.MaxValue;
        }
    }
}
