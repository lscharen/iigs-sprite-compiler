namespace SpriteCompiler.AI
{
    public interface INodeLimiter<T, C>
        where T : ISearchNode<C>
        where C : ICost<C>, new()
    {
        bool Cutoff(T node);
    }
}
