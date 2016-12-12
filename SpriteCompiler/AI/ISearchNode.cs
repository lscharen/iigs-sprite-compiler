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
    public interface ISearchNode<A, S, T, C> : ISearchNode<C> where C : ICost<C>
    {
        A Action { get; set; }
        S State { get; }
        T Parent { get; }
    }

    /// <summary>
    /// Simplest representation of a seach node that just has the costs.  This interface
    /// is useful for certain evaluation functions
    /// </summary>
    /// <typeparam name="C"></typeparam>
    public interface ISearchNode<C> where C : ICost<C>
    {
        C PathCost { get; }
        C StepCost { get; set; }
        C EstCost { get; }
        int Depth { get; }
    }
}
