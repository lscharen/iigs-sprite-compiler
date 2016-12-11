using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public interface ISearchStepInfo<T>
    {
        bool IsGoal { get; }
        T Node { get; }
        IEnumerable<T> Solution { get; }
    }

    public class SearchStepInfo<T> : ISearchStepInfo<T>
    {
        private readonly T node;
        private readonly IEnumerable<T> solution;

        public SearchStepInfo(T node, IEnumerable<T> solution)
        {
            this.solution = solution;
            this.node = node;
        }

        public IEnumerable<T> Solution { get { return solution; } }
        public bool IsGoal { get { return solution != null; } }
        public T Node { get { return node; } }
    }
}
