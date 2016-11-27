namespace SpriteCompiler.AI
{
    using System.Collections.Generic;

    public interface ISuccessorFunction<A, S>
    {
        IDictionary<A, S> Successors(S state);
    }
}
