namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;
    using System.Linq;

    public sealed class SpriteGeneratorHeuristicFunction : IHeuristicFunction<SpriteGeneratorState, IntegerPathCost>
    {
        private static int SpanAndGapCost(int start, int end, int next)
        {
            // [start, end] is the span
            // (end, next) is the gap
            //
            // start <= end <= next
            var len = end - start + 1;
            
            // No gap, no penalty
            var gapCost = (end == next) ? 0 : 5;
            var spanCost = 4 * (len / 2) + 3 * (len % 2);

            return gapCost + spanCost;
        }

        public IntegerPathCost Eval(SpriteGeneratorState state)
        {
            // An admissible heuistic calculates a cost based on the gaps and runs in a sprite
            //
            // An even-length run can be done, at best in 4 cycles/word
            // An odd-length run is even + 3 cycles/byte
            //
            // Each gap needs at least 5 cycles to cover (ADC # / TCS)

            var count = state.Bytes.Count;

            if (count == 0) return 0;

            var offsets = state.Bytes.Select(x => x.Offset).OrderBy(x => x).ToList();
            var start = offsets[0];
            var curr = start;
            var cost = 0;

            for (int i = 1; i < count; i++)
            {
                var prev = curr;
                curr  = offsets[i];

                if (prev == (curr - 1))
                {
                    continue;
                }

                // Calculate the estimate cost
                cost += SpanAndGapCost(start, prev, curr);

                // Start a new sppan
                start = curr;    
            }

            // End with the span
            cost += SpanAndGapCost(start, curr, curr);

            return cost;
        }
    }
}
