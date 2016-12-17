namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;
    using SpriteCompiler.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class StateHelpers
    {
        public static SpriteByte? TryGetStackByte(this SpriteGeneratorState state, IDictionary<ushort, SpriteByte> data)
        {
            SpriteByte top;
            if (state.S.IsScreenOffset && data.TryGetValue((ushort)state.S.Value, out top))
            {
                return top;
            }

            return null;
        }

        public static SpriteWord? TryGetStackWord(this SpriteGeneratorState state, IDictionary<ushort, SpriteByte> data)
        {
            return TryGetStackWord(state, data, 0);
        }

        public static SpriteWord? TryGetStackWord(this SpriteGeneratorState state, IDictionary<ushort, SpriteByte> data, int offset)
        {
            // When we get a word, it's permissible for either the high or low byte to not exist, but not both
            SpriteByte high;
            SpriteByte low;

            // Make sure the range is within bounds
            if (state.S.IsScreenOffset && (state.S.Value + offset) > 0)            
            {
                // Try to get the high byte
                byte high_data = 0x00;
                byte high_mask = 0xFF;
                ushort high_offset = (ushort)(state.S.Value + offset);

                if (data.TryGetValue(high_offset, out high))
                {
                    high_data = high.Data;
                    high_mask = high.Mask;
                }

                // Try to get the low byte
                byte low_data = 0x00;
                byte low_mask = 0xFF;
                ushort low_offset = (ushort)(state.S.Value + offset - 1);

                if (data.TryGetValue(low_offset, out low))
                {
                    low_data = low.Data;
                    low_mask = low.Mask;
                }
            
                // At least some data need to be visible
                if (high_mask != 0xFF || low_mask != 0xFF)
                {
                    var word_data = (ushort)(low_data + (high_data << 8));
                    var word_mask = (ushort)(low_mask + (high_mask << 8));

                    return new SpriteWord(word_data, word_mask, low_offset);
                }
            }

            return null;
        }

        public static Tuple<CodeSequence, SpriteGeneratorState> Apply(this SpriteGeneratorState state, CodeSequence code)
        {
            return Tuple.Create(code, code.Apply(state));
        }
    }

    public sealed class SpriteGeneratorSuccessorFunction : ISuccessorFunction<CodeSequence, SpriteGeneratorState>
    {
        public IEnumerable<Tuple<CodeSequence, SpriteGeneratorState>> Successors(SpriteGeneratorState state)
        {
            // Get the list of remaining bytes by removing the closed list from the global sprite dataset
            var open = state.RemainingBytes();

            // If the open list is empty, there can be only one reaons -- we're in an 8-bit
            // mode.
            if (!open.Any())
            {
                yield return state.Apply(new LONG_M());
                yield break;
            }

            // Get the first byte -- if we are expanding a state we can safely assume there
            // is still data to expand.
            var firstByte = open.First();

            // Identify the first run of solid bytes
            var firstSolid = FirstSolidRun(open);

            // In an initial state, the stack has not been set, but the accumulator contains a screen
            // offset address.  There are three possible options
            //
            // 1. Set the stack to the accumulator value.  This is optimal when there are very few
            //    data bytes to set and the overhead of moving the stack negates any benefit from
            //    using stack push instructions
            //
            // 2. Set the stack to the first offset of the open set. This can be optimal if there are a few
            //    bytes and moving the stack forward a bit can allow the code to reach them without needing
            //    a second stack adjustment.
            //
            // 3. Set the stack to the first, right-most offset that end a sequence of solid bytes
            if (!state.S.IsScreenOffset && state.A.IsScreenOffset)
            {
                // If the first byte is within 255 bytes of the accumulator, propose setting
                // the stack to the accumulator value
                var delta = firstByte.Offset - state.A.Value;
                if (delta >= 0 && delta < 256)
                {
                    yield return state.Apply(new MOVE_STACK(0));
                }

                // If the first byte offset is not equal to the accumulator value, propose that
                if (delta > 0)
                {
                    yield return state.Apply(new MOVE_STACK(delta));
                }

                // Find the first edge of a solid run....TODO
                if (firstSolid != null && firstSolid.Count >= 2)
                {
                    yield return state.Apply(new MOVE_STACK(firstSolid.Last.Offset - state.A.Value));
                }

                yield break;
            }

            // If the first byte is 256 bytes or more ahead of the current stack location,
            // then we need to advance
            var firstByteDistance = firstByte.Offset - state.S.Value;
            if (state.S.IsScreenOffset && firstByteDistance >= 256)
            {
                // Go to the next byte, or the first solid edge
                yield return state.Apply(new MOVE_STACK(firstByteDistance));

                yield break;
            }
            
            var bytes = open.ToDictionary(x => x.Offset, x => x);
           
            // Get the current byte and current word that exist at the current stack location
            var topByte = state.TryGetStackByte(bytes);
            var topWord = state.TryGetStackWord(bytes);

            // If the top of the stack is a solid work, we can always emit a PEA regardless of 8/16-bit mode
            if (topWord.HasValue && topWord.Value.Mask == 0x000)
            {
                yield return state.Apply(new PEA(topWord.Value.Data));
            }

            // First set of operations for when the accumulator is in 8-bit mode.  We are basically limited
            // to either
            //
            // 1. Switching to 16-bit mode
            // 3. Using a PHA to push an 8-bit solid or masked value on the stack
            // 4. Using a STA 0,s to store an 8-bit solid or masked value on the stack

            if (!state.LongA)
            {
                // The byte to be stored at the top of the stack has some special methods available
                if (topByte.HasValue)
                {
                    var datum = topByte.Value;

                    // Solid byte
                    if (datum.Mask == 0x00)
                    {
                        if (state.A.IsLiteral && ((state.A.Value & 0xFF) == datum.Data))
                        {
                            yield return state.Apply(new PHA_8());
                        }
                        else
                        {
                            yield return state.Apply(new LOAD_8_BIT_IMMEDIATE_AND_PUSH(datum.Data));
                            yield return state.Apply(new STACK_REL_8_BIT_IMMEDIATE_STORE(datum.Data, 0));
                        }
                    }

                    // Masked byte
                    else
                    {
                        yield return state.Apply(new STACK_REL_8_BIT_READ_MODIFY_PUSH(datum.Data, datum.Mask));
                    }
                }

                // Otherwise, just store the next byte closest to the stack
                if (state.S.IsScreenOffset)
                {
                    var addr = state.S.Value;

                    // We can LDA #$XX / STA X,s for any values within 256 bytes of the current address
                    foreach (var datum in open.Where(WithinRangeOf(addr, 256)))
                    {
                        var offset = (byte)(datum.Offset - addr);

                        // Easy case when mask is empty
                        if (datum.Mask == 0x00)
                        {
                            if (datum.Data == (state.A.Value & 0xFF))
                            {
                                yield return state.Apply(new STACK_REL_8_BIT_STORE(offset));
                            }
                            else
                            {
                                yield return state.Apply(new STACK_REL_8_BIT_IMMEDIATE_STORE(datum.Data, offset));
                            }
                        }

                        // Otherwise there is really only one choice LDA / AND / ORA / STA sequence
                        else
                        {
                            yield return state.Apply(new STACK_REL_8_BIT_READ_MODIFY_WRITE(datum.Data, datum.Mask, offset));
                        }
                    }
                }

                if (state.AllowModeChange)
                {
                    yield return state.Apply(new LONG_M());
                }
            }

            // Now consider what can be done when the accumulator is 16-bit.  All of the stack
            // manipulation happens here, too.
            if (state.LongA)
            {
                // Handle the special case of a value sitting right on top of the stack
                if (topWord.HasValue)
                {
                    var datum = topWord.Value;

                    // First, the simple case -- the data has no mask
                    if (datum.Mask == 0x000)
                    {
                        if (state.A.IsLiteral && state.A.Value == datum.Data)
                        {
                            yield return state.Apply(new PHA_16());
                        }
                        else
                        {
                            // Only consider this if the value appear in the sprite more than once
                            if (SpriteGeneratorState.DATASET_SOLID_WORDS[topWord.Value.Data] > 1)
                            {
                                yield return state.Apply(new LOAD_16_BIT_IMMEDIATE_AND_PUSH(topWord.Value.Data));
                            }
                        }
                    }
                }

                // If the stack is set, find the next word to store (just one to reduce branching factor)
                if (state.S.IsScreenOffset)
                {
                    var addr = state.S.Value;

                    var local = open.Where(WithinRangeOf(addr, 256)).ToList();
                    var words = local
                        .Where(x => SpriteGeneratorState.DATASET_BY_OFFSET.ContainsKey(x.Offset + 1))
                        .Select(x => new { Low = x, High = SpriteGeneratorState.DATASET_BY_OFFSET[x.Offset + 1] })
                        .ToList();

                    foreach (var word in words)
                    {
                        var offset = (byte)(word.Low.Offset - addr);
                        var data = (ushort)(word.Low.Data + (word.High.Data << 8));
                        var mask = (ushort)(word.Low.Mask + (word.High.Mask << 8));
                        // Easy case when mask is empty
                        if (mask == 0x0000)
                        {
                            if (data == state.A.Value)
                            {
                                yield return state.Apply(new STACK_REL_16_BIT_STORE(offset));
                            }
                            else
                            {
                                yield return state.Apply(new STACK_REL_16_BIT_IMMEDIATE_STORE(data, offset));
                            }
                        }

                        // Otherwise there is really only one choice LDA / AND / ORA / STA sequence
                        else
                        {
                            yield return state.Apply(new STACK_REL_16_BIT_READ_MODIFY_WRITE(data, mask, offset));
                        }
                    }
                }

                if (state.AllowModeChange)
                {
                    yield return state.Apply(new SHORT_M());
                }
            }

        Done:
            var z = 0; z += 1;
        }

        private static bool IsSolidPair(Tuple<SpriteByte, SpriteByte> pair)
        {
            return
                (pair.Item1.Offset == (pair.Item2.Offset - 1)) &&
                pair.Item1.Mask == 0x00 &&
                pair.Item2.Mask == 0x00;
        }

        private class SolidRun
        {
            private readonly SpriteByte first;
            private readonly SpriteByte last;
        
            public SolidRun(SpriteByte first, SpriteByte last)
            {
                this.first = first;
                this.last = last;
            }

            public SpriteByte First { get { return first; } }
            public SpriteByte Last { get { return last; } }
            public int Count { get { return last.Offset - first.Offset + 1; } }
        }

        private SolidRun FirstSolidRun(IEnumerable<SpriteByte> open)
        {
            bool trigger = false;
            SpriteByte first = default(SpriteByte);
            SpriteByte last = default(SpriteByte);
            
            foreach (var item in open)
            {
                if (item.Mask == 0x00 && !trigger)
                {
                    first = item;
                    trigger = true;
                }

                if (item.Mask != 0x00 && trigger)
                {
                    return new SolidRun(first, last);
                }

                last = item;
            }

            // If we get to the end and are still sold, great
            if (last.Mask == 0x00 && trigger)
            {
                return new SolidRun(first, last);
            }

            return null;
        }

        private Func<SpriteByte, bool> WithinRangeOf(int addr, int range)
        {
            return x => (x.Offset >= addr) && ((x.Offset - addr) < range);
        }
    }
}
