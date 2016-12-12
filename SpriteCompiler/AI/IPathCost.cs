namespace SpriteCompiler.AI
{
    using System;

    public interface ICost<C> : IComparable<C>
    {
        C Add(C value);

        // Number theoretic values, i.e.   C + ZERO = C, C * ONE = C
        C Zero();
        C One();
        C Maximum();
    }
}
