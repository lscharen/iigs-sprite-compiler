namespace SpriteCompiler.AI
{
    using System;
    using System.Collections.Generic;

    public interface ISuccessorFunction<A, S>
    {
        IEnumerable<Tuple<A, S>> Successors(S state);
    }
}
