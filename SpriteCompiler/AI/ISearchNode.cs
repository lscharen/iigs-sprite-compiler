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
    public interface ISearchNode<A, S, T, C> where C : IComparable<C>
    {
        A Action { get; set; }
        C StepCost { get; set; }
        C PathCost { get; }
        int Depth { get; }
        S State { get; }
        T Parent { get; }
    }
}
