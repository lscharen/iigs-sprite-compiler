namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;
    using System.Linq;
    using System;

    public sealed class SpriteGeneratorHeuristicFunction : IHeuristicFunction<SpriteGeneratorState, IntegerPathCost>
    {
        private static int SpanAndGapCost(int stack, int start, int end, int next)
        {
            var len = end - start + 1;

            // If the span is within 255 bytes of the stack, there is no
            // gap penalty and we base the cost off of sta xx,s instructions

            var h1 = SpanAndGapCost(start, end, next);
            var h2 = int.MaxValue;

            if (stack <= end && (end - stack) < 256)
            {
                h2 = 5 * (len / 2) + 4 * (len % 2);
            }

            return Math.Min(h1, h2);
        }

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
            // An admissible heuistic that calculates a cost based on the gaps and runs in a sprite
            //
            // An even-length run can be done, at best in 4 cycles/word
            // An odd-length run is even + 3 cycles/byte

            if (state.IsEmpty) return 0;

            var offsets = state.RemainingBytes();
            var start = offsets[0];
            var stack = state.S.Value;
            var curr = start;
            var cost = 0;

            for (int i = 1; i < offsets.Count; i++)
            {
                var prev = curr;
                curr  = offsets[i];

                if (prev == (curr - 1))
                {
                    continue;
                }

                // Calculate the estimate cost
                if (state.S.IsScreenOffset)
                {
                    cost += SpanAndGapCost(stack, start, prev, curr);
                }
                else
                {
                    cost += SpanAndGapCost(start, prev, curr);
                }

                // Start a new sppan
                start = curr;    
            }

            // End with the span
            cost += SpanAndGapCost(start, curr, curr);

            return cost;
        }
    }
}
