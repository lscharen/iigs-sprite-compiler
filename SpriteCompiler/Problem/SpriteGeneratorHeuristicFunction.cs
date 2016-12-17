namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;
    using SpriteCompiler.Helpers;
    using System.Linq;
    using System;
    using System.Collections.Generic;

    public sealed class SpriteGeneratorHeuristicFunction : IHeuristicFunction<SpriteGeneratorState, IntegerCost>
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

        private const int MASKED_DATA = 0;
        private const int SOLID_DATA = 1;

        private int ClassifyChunk(IEnumerable<SpriteByte> chunk)
        {
            return (chunk.First().Mask == 0x00) ? SOLID_DATA : MASKED_DATA;
        }

        private bool AreEquivalent(SpriteByte left, SpriteByte right)
        {
            // Return true if the two bytes are in an equivalence class
            return
                // The have to be adjacent
                ((right.Offset - left.Offset) == 1) &&
                (
                    // First case: Both bytes are solid
                    ((left.Mask == 0x00) && (right.Mask == 0x00)) ||

                    // Second case: Both bytes are masked
                    ((left.Mask != 0x00) && (right.Mask != 0x00))
                );
        }

        public IntegerCost Eval(SpriteGeneratorState state)
        {
            // An admissible heuistic that calculates a cost based on the gaps and runs in a sprite
            //
            // An even-length run can be done, at best in 4 cycles/word
            // An odd-length run is even + 3 cycles/byte

            // Easy case -- no data means no code
            if (state.IsEmpty)
            {
                return 0;
            }

            // Get a list of all the bytes that have not been emitted
            var remaining = state.RemainingBytes();

            // Count the number of 
            //
            // solid words
            // solid bytes
            // masked words
            // masked bytes
            //
            // By grouping the remaining bytes into solid / masked runs
            var chunks = remaining.GroupAdjacent((x, y) => AreEquivalent(x, y));

            // Figure out the full range of offsets that need to be written
            var range = remaining.Last().Offset - remaining.First().Offset;

            // At best, we can write 257 bytes before needing to move the stack (0,s to 16-bit save to ff,s).
            var minStackMoves = range / 257;

            // Calculate a heuristic            
            var cost = 0;

            // If the stack is undefined, then there is at least a 2 cycle
            // cost to initialize it
            if (!state.S.IsScreenOffset)
            {
                cost += 2;
            }

            cost += minStackMoves * 5;

            // Iterate through each chunk, determine which equivalence class the chunk is
            // in and then assign a minimum score
            foreach (var chunk in chunks)
            {
                var len = chunk.Count();

                switch (ClassifyChunk(chunk))
                {
                    // Solid data costs at least 4 cycles / word + 3 cycles for a byte
                    case SOLID_DATA:
                        cost += 4 * (len / 2) + 3 * (len % 2);
                        break;

                    // Masked data costs at least 16 cycles / word + 11 cycles for a byte
                    case MASKED_DATA:
                        cost += 16 * (len / 2) + 11 * (len % 2);
                        break;
                }
            }

            return cost;
        }
    }
}
