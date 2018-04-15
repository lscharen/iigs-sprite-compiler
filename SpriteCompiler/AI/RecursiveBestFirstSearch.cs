namespace SpriteCompiler.AI
{
    using Queue;
    using System;
    using System.Linq;

#if False
    public class RecursiveBestFirstSearch<A, S, T, C> : BestFirstSearch<A, S, T, C>
        where T : IHeuristicSearchNodeWithMemory<A, S, T, C>
        where C : ICost<C>, new()
    {
        public RecursiveBestFirstSearch(ISearchStrategy<A, S, T, C> search, Func<IQueue<T>> fringe)
            : base(search, fringe)
        {
        }
    }

    public class RecursiveBestFirstSearchStrategy<A, S, T, C> : AbstractSearchStrategy<A, S, T, C> 
        where T : IHeuristicSearchNodeWithMemory<A, S, T, C>
        where C : ICost<C>, new()
    {
        private static readonly C Cost = new C();
        private ISearchProblem<A, S, C> problem;

        public RecursiveBestFirstSearchStrategy(INodeExpander<A, S, T, C> expander)
            : base(expander)
        {
        }

        public override System.Collections.Generic.IEnumerable<T> Search(ISearchProblem<A, S, C> problem, IQueue<T> fringe, S initialState)
        {
            RBFS(Expander.CreateNode(initialState), Cost.Zero(), Cost.Maximum());
        }

        private C RBFS(T node, C F_N, C bound)
        {
            var f_N = problem.Heuristic(node.State);
            
            if (f_N.CompareTo(bound) > 0)
            {
                return f_N;
            }

            if (problem.IsGoal(node.State))
            {
                throw new Exception();
            }

            var children = Expander.Expand(problem, node);
            if (!children.Any())
            {
                return Cost.Maximum();
            }

            foreach (var N_i in children)
            {
                if (f_N.CompareTo(F_N) < 0)
                {
                    N_i.F = F_N.Max(N_i.EstCost);
                }
                else
                {
                    N_i.F = N_i.EstCost;
                }
            }

            children = children.OrderBy(x => x.F);


            /*
            RBFS (node: N, value: F(N), bound: B)
            IF f(N)>B, RETURN f(N)
            IF N is a goal, EXIT algorithm
            IF N has no children, RETURN infinity
            FOR each child Ni of N,
                IF f(N)<F(N), F[i] := MAX(F(N),f(Ni))
                ELSE F[i] := f(Ni)
            sort Ni and F[i] in increasing order of F[i]
            IF only one child, F[2] := infinity
            WHILE (F[1] <= B and F[1] < infinity)
                F[1] := RBFS(N1, F[1], MIN(B, F[2]))
                insert Ni and F[1] in sorted order
            RETURN F[1]
            */
        }
    }
#endif
}
