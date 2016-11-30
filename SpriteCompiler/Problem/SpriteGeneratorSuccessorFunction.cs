namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class SpriteGeneratorSuccessorFunction : ISuccessorFunction<CodeSequence, SpriteGeneratorState>
    {
        public IDictionary<CodeSequence, SpriteGeneratorState> Successors(SpriteGeneratorState state)
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

            var actions = new List<CodeSequence>();
            var bytes = state.Bytes.ToDictionary(x => x.Offset, x => x);

            // If the accumulator holds an offset then we could move to any byte position.
            if (state.A.IsScreenOffset && !state.S.IsScreenOffset)
            {
                foreach (var datum in state.Bytes)
                {
                    actions.Add(new MOVE_STACK(datum.Offset - state.A.Value));
                }
            }

            // If the accumulator and stack are both initialized, only propose moves to locations
            // before and after the current 256 byte stack-relative window
            if (state.A.IsScreenOffset && state.S.IsScreenOffset)
            {
                var addr = state.S.Value;
                foreach (var datum in state.Bytes.Where(x => (x.Offset - addr) > 255 || (x.Offset - addr) < 0))
                {
                    actions.Add(new MOVE_STACK(datum.Offset - state.A.Value));
                }
            }

            // If the stack is valid on a word (consecutive bytes), when we can alway do a PEA
            if (state.S.IsScreenOffset && state.S.Value > 0)
            {
                var addr = state.S.Value;
                if (bytes.ContainsKey((ushort)addr) && bytes.ContainsKey((ushort)(addr - 1)))
                {
                    var high = bytes[(ushort)addr].Data;
                    var low = bytes[(ushort)(addr - 1)].Data;

                    var word = (ushort)(low + (high << 8));
                    actions.Add(new PEA(word));
                }
            }

            // It is always permissible to move to/from 16 bit mode
            if (state.LongA)
            {
                actions.Add(new SHORT_M());

                // Add any possible 16-bit data manipulations
                if (state.S.IsScreenOffset)
                {
                    var addr = state.S.Value;

                    // Look for consecutive bytes
                    var local = state.Bytes.Where(WithinRangeOf(addr, 257)).ToList(); // 16-bit value can extend to the 256th byte
                    var words = local
                        .Skip(1)
                        .Select((x, i) => new { High = x, Low = local[i] })
                        .Where(p => p.Low.Offset == (p.High.Offset - 1))
                        .ToList();

                    foreach (var word in words)
                    {
                        var offset = (byte)(word.Low.Offset - addr);
                        var data = (ushort)(word.Low.Data + (word.High.Data << 8));
                        actions.Add(new STACK_REL_16_BIT_IMMEDIATE_STORE(data, offset));

                    }

                    // We can LDA #$XXXX / STA X,s for any values within 256 bytes of the current address
                    foreach (var datum in state.Bytes.Where(WithinRangeOf(addr, 256)))
                    {
                        var offset = (byte)(datum.Offset - addr);
                        actions.Add(new STACK_REL_8_BIT_IMMEDIATE_STORE(datum.Data, offset));
                    }
                }
            }
            else
            {
                actions.Add(new LONG_M());

                // Add any possible 8-bit manipulations
                if (state.S.IsScreenOffset)
                {
                    var addr = state.S.Value;

                    // We can LDA #$XX / STA X,s for any values within 256 bytes of the current address
                    foreach (var datum in state.Bytes.Where(WithinRangeOf(addr, 256)))
                    {
                        var offset = datum.Offset - addr;
                        actions.Add(new STACK_REL_8_BIT_IMMEDIATE_STORE(datum.Data, (byte)offset));
                    }
                }
            }

            // Run through the actions to create a dictionary
            return actions.ToDictionary(x => x, x => x.Apply(state));
        }

        private Func<SpriteByte, bool> WithinRangeOf(int addr, int range)
        {
            return x => (x.Offset >= addr) && ((x.Offset - addr) < range);
        }
    }
}
