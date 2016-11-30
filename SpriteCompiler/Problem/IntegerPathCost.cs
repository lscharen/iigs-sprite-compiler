namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class IntegerPathCost : IPathCost<IntegerPathCost>
    {
        private readonly int value;

        public IntegerPathCost()
            : this(0)
        {
        }

        private IntegerPathCost(int value)
        {
            this.value = value;
        }

        public static implicit operator int(IntegerPathCost obj)
        {
            return obj.value;
        }

        public static implicit operator IntegerPathCost(int value)
        {
            return new IntegerPathCost(value);
        }

        public IntegerPathCost Add(IntegerPathCost other)
        {
            return value + other.value;
        }

        public int CompareTo(IntegerPathCost other)
        {
            return value.CompareTo(other.value);
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
