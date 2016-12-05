namespace SpriteCompiler.AI
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="A">Action on a node</typeparam>
    /// <typeparam name="S">State of the search</typeparam>
    /// <typeparam name="T">Type of the parent</typeparam>
    /// <typeparam name="C">Cost type</typeparam>
    public interface ISearchNode<A, S, T, C> : ISearchNode<C> where C : IPathCost<C>
    {
        A Action { get; set; }
        C PathCost { get; }
        C StepCost { get; set; }
        int Depth { get; }
        S State { get; }
        T Parent { get; }
    }

    public interface ISearchNode<C>
    {
        C EstCost { get; }
    }
}
