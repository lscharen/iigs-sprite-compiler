using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.Problem
{
    /// <summary>
    /// A set of code sequences that can be used to generate sprites
    /// </summary>
    public abstract class CodeSequence
    {
        protected CodeSequence(int cycles)
        {
            CycleCount = cycles;
        }

        // Number of cycles that this code snippets takes to execute
        public int CycleCount { get; private set; }

        // Function to generate a new state based on the code's operation
        public abstract SpriteGeneratorState Apply(SpriteGeneratorState state);

        // Helper function for ToString implementations
        protected string FormatLine(string label, string opcode, string operand, string comment)
        {
            return String.Format("{0}\t{1}\t{2}\t; {3}", label, opcode, operand, comment);
        }
    }

    public sealed class MOVE_STACK : CodeSequence
    {
        private readonly int offset;

        public MOVE_STACK(int offset)
            : base(offset == 0 ? 2 : 5)
        {
            this.offset = offset;
        }

        public override SpriteGeneratorState Apply(SpriteGeneratorState state)
        {
            return state.Clone(_ =>
            {
                _.A = _.A.Add(offset);
                _.S = _.A;
            });
        }

        public override string ToString()
        {
            if (offset == 0)
            {
                return FormatLine("", "TCS", "", "2 cycles");
            }
            else
            {
                return String.Join("\n",
                    FormatLine("", "ADC", "#" + offset.ToString(), "3 cycles"),
                    FormatLine("", "TCS", "", "2 cycles")
                );
            }
        }
    }

    public sealed class SHORT_M : CodeSequence
    {
        public SHORT_M() : base(3) { }

        public override SpriteGeneratorState Apply(SpriteGeneratorState state)
        {
            return state.Clone(_ => _.P &= 0xEF);
        }

        public override string ToString()
        {
            return FormatLine("", "SEP", "#$10", "3 cycles");
        }
    }

    public sealed class LONG_M : CodeSequence
    {
        public LONG_M() : base(3) { }

        public override SpriteGeneratorState Apply(SpriteGeneratorState state)
        {
            return state.Clone(_ => _.P |= 0x10);
        }

        public override string ToString()
        {
            return FormatLine("", "REP", "#$10", "3 cycles");
        }
    }

    public sealed class STACK_REL_8_BIT_IMMEDIATE_STORE : CodeSequence
    {
        private readonly byte value;
        private readonly byte offset;

        public STACK_REL_8_BIT_IMMEDIATE_STORE(byte value, byte offset) : base(6) { this.value = value; this.offset = offset; }

        public override SpriteGeneratorState Apply(SpriteGeneratorState state)
        {
            return state.Clone(_ =>
            {
                _.A = _.A.LoadConstant((_.A.Value & 0xFF00) | value);
                _.RemoveByte((ushort)(offset + _.S.Value));
            });
        }

        public override string ToString()
        {
            return String.Join("\n",
                FormatLine("", "LDA", "#$" + value.ToString("X2"), "2 cycles"),
                FormatLine("", "STA", offset.ToString("X2") + ",s", "4 cycles")
            );
        }
    }

    public sealed class STACK_REL_16_BIT_IMMEDIATE_STORE : CodeSequence
    {
        private readonly ushort value;
        private readonly byte offset;

        public STACK_REL_16_BIT_IMMEDIATE_STORE(ushort value, byte offset) : base(8) { this.value = value; this.offset = offset; }

        public override SpriteGeneratorState Apply(SpriteGeneratorState state)
        {
            return state.Clone(_ =>
            {
                _.A = _.A.LoadConstant(value);
                _.RemoveWord((ushort)(offset + _.S.Value));
            });
        }

        public override string ToString()
        {
            return String.Join("\n",
                FormatLine("", "LDA", "#$" + value.ToString("X4"), "3 cycles"),
                FormatLine("", "STA", offset.ToString("X2") + ",s", "5 cycles")
            );
        }
    }

    public sealed class PEA : CodeSequence
    {
        private readonly ushort value;

        public PEA(ushort value) : base(5) { this.value = value; }
    
        public override SpriteGeneratorState Apply(SpriteGeneratorState state)
        {
            return state.Clone(_ =>
            {
                _.S.Add(-2);
                _.RemoveWord((ushort)(_.S.Value - 1));
            });
        }

        public override string ToString()
        {
            return FormatLine("", "PEA", "$" + value.ToString("X4"), "5 cycles");
        }
    }


}
