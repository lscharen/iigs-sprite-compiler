namespace SpriteCompiler.AI
{
    using System;

    public static class CostExtensions
    {
        public static C Max<C>(this C left, C right) where C : ICost<C>
        {
            return (left.CompareTo(right) >= 0)
                ? left
                : right;
        }
    }

    public interface ICost<C> : IComparable<C>
    {
        C Add(C value);

        // Number theoretic values, i.e. C + ZERO = C, C * ONE = C
        C Zero();
        C One();
        C Maximum();
    }
}
