namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;
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
            // This is the work-horse of the compiler.  For a given state we need to enumerate all of the
            // potential next operations.
            //
            // 1. If there are 16-bits of data at then current offset, we can
            //    a. Use one of the cached valued in A/X/Y/D if they match (4 cycles)
            //    b. Use a PEA to push immediate values (5 cycles)
            //    c. Load a value into A/X/Y and then push (7 cycles, only feasible if the value appears elsewhere in the sprite)
            //    d. Load the value into D and then push (9 cycles, and leaves A = D)
            //
            // 2. Move the stack
            //    a. Add a value directly (7 cycles, A = unknown)
            //    b. Skip 1 byte (6 cycles, A = unknown TSC/DEC/TSC)
            //    c. Multiple skips (LDA X,s/AND/ORA/STA = 16/byte,  ADC #/TCS/LDX #/PHX = 10/byte
            //
            // 3. Single-byte at the end of a solid run
            //    a. If no registers are 8-bit, LDA #Imm/STA 0,s (8 cycles, sets Acc)
            //    b. If any reg is already 8-bit, LDA #imm/PHA (6 cycles)
            //
            // We always try to return actions that write data since that moves us toward the goal state
            
            // Get the list of remaining bytes by removing the closed list from the global sprite
            var open = SpriteGeneratorState.DATASET.Where(x => !state.Closed.Contains(x.Offset)).ToList();
            var bytes = open.ToDictionary(x => x.Offset, x => x);

            // Get the current byte and current word that exist at the current stack location
            var topByte = state.TryGetStackByte(bytes);
            var topWord = state.TryGetStackWord(bytes);
            var nextWord = state.TryGetStackWord(bytes, -2); // Also get the next value below the current word

            // If there is some data at the top of the stack, see what we can do
            if (topWord.HasValue)
            {
                // First, the simple case -- the data has no mask
                if (topWord.Value.Mask == 0x000)
                {
                    // If any of the registers has the exact value we need, then it is always fastest to just push the value
                    if (state.LongA)
                    {
                        if (state.A.IsLiteral && state.A.Value == topWord.Value.Data)
                        {
                            yield return state.Apply(new PHA());
                        }
                        else
                        {
                            yield return state.Apply(new PEA(topWord.Value.Data));
                            yield return state.Apply(new LOAD_16_BIT_IMMEDIATE_AND_PUSH(topWord.Value.Data));
                        }
                    }
                    // Otherwise, the only alternative is a PEA instruction
                    else
                    {
                        yield return state.Apply(new PEA(topWord.Value.Data));
                    }
                }
            }

            // If there is a valid byte, then we can look for an 8-bit push, or an immediate mode LDA #XX/STA 0,s
            if (topByte.HasValue)
            {
                if (topByte.Value.Mask == 0x00)
                {
                    if (!state.LongA)
                    {
                        yield return state.Apply(new STACK_REL_8_BIT_IMMEDIATE_STORE(topByte.Value.Data, 0));
                    }
                }
            }

            // If the accumulator holds an offset then we could move to any byte position, but it is only beneficial to
            // either
            //
            // 1. Set the stack to the current accumulator value
            // 2. Set the stack to the start of a contiguous segment
            // 3. Set the stack to the end of a contiguous segment

            // move to the first or last byte of each span.  So , take the first byte and then look for any
            if (state.A.IsScreenOffset && !state.S.IsScreenOffset && state.LongA)
            {
                // If any of the open bytes are within 255 bytes of the accumulator, consider just 
                // setting the stack to the accumulator value
                if (open.Any(x => (x.Offset - state.A.Value) >= 0 && (x.Offset - state.A.Value) < 256))
                {
                    yield return state.Apply(new MOVE_STACK(0));
                }

                for (var i = 0; i < open.Count; i++)
                {
                    if (i == 0)
                    {
                        yield return state.Apply(new MOVE_STACK(open[i].Offset - state.A.Value));
                        continue;
                    }

                    if (i == open.Count - 1)
                    {
                        yield return state.Apply(new MOVE_STACK(open[i].Offset - state.A.Value));
                        continue;
                    }

                    if ((open[i].Offset - open[i - 1].Offset) > 1)
                    {
                        yield return state.Apply(new MOVE_STACK(open[i].Offset - state.A.Value));
                    }
                }
            }

            // It is always permissible to move to/from 16 bit mode
            if (state.LongA)
            {
                yield return state.Apply(new SHORT_M());

                // Add any possible 16-bit data manipulations
                if (state.S.IsScreenOffset)
                {
                    var addr = state.S.Value;

                    // Look for consecutive bytes. The second byte can come from the DATASET
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
                                yield return state.Apply(new STACK_REL_16_BIT_STORE(data, offset));
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
            }
            else
            {
                yield return state.Apply(new LONG_M());

                // Add any possible 8-bit manipulations
                if (state.S.IsScreenOffset)
                {
                    var addr = state.S.Value;

                    // We can LDA #$XX / STA X,s for any values within 256 bytes of the current address
                    foreach (var datum in open.Where(WithinRangeOf(addr, 256)))
                    {
                        var offset = datum.Offset - addr;
                        if (datum.Mask == 0x00)
                        {
                            yield return state.Apply(new STACK_REL_8_BIT_IMMEDIATE_STORE(datum.Data, (byte)offset));
                        }
                        else
                        {
                            yield return state.Apply(new STACK_REL_8_BIT_READ_MODIFY_WRITE(datum.Data, datum.Mask, (byte)offset));
                        }
                    }
                }
            }

            // If the accumulator and stack are both initialized, only propose moves to locations
            // before and after the current 256 byte stack-relative window
            if (state.A.IsScreenOffset && state.S.IsScreenOffset && state.LongA)
            {
                var addr = state.S.Value;
                foreach (var datum in open.Where(x => (x.Offset - addr) > 255 || (x.Offset - addr) < 0))
                {
                    yield return state.Apply(new MOVE_STACK(datum.Offset - state.A.Value));
                }
            }
        }

        private Func<SpriteByte, bool> WithinRangeOf(int addr, int range)
        {
            return x => (x.Offset >= addr) && ((x.Offset - addr) < range);
        }
    }
}
