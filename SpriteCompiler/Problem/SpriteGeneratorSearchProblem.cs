using SpriteCompiler.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.Problem
{
    public sealed class IntegerPathCost : IPathCost<IntegerPathCost>
    {
        private readonly int value;

        public IntegerPathCost()
            : this(0)
        {
        }

        private IntegerPathCost(int value)
        {
            this.value = value;
        }

        public static implicit operator int(IntegerPathCost obj)
        {
            return obj.value;
        }

        public static implicit operator IntegerPathCost(int value)
        {
            return new IntegerPathCost(value);
        }

        public IntegerPathCost Add(IntegerPathCost other)
        {
            return value + other.value;
        }

        public int CompareTo(IntegerPathCost other)
        {
            return value.CompareTo(other.value);
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class SpriteGeneratorState
    {
        public SpriteGeneratorState()
            : this(new SpriteByte[0])
        {
        }

        public SpriteGeneratorState(byte[] data)
            : this(data.Select((x, i) => new SpriteByte(x, (ushort)i)))
        {
        }

        public SpriteGeneratorState(IEnumerable<SpriteByte> bytes)
        {
            Bytes = bytes.ToList();

            // Initialize the CPU state
            A = Register.INITIAL_OFFSET; // the address to draw the sprite is passed in, this is a run-time value

            X = Register.UNINITIALIZED; // the other registered are also undefined
            Y = Register.UNINITIALIZED;
            D = Register.UNINITIALIZED;
            S = Register.UNINITIALIZED;

            P = 0x30; // Start in native mode (16 bit A/X/Y) with the carry clear
        }

        private SpriteGeneratorState(SpriteGeneratorState other)
        {
            Bytes = new List<SpriteByte>(other.Bytes);
            A = other.A;
            X = other.X;
            Y = other.Y;
            D = other.D;
            S = other.S;
            P = other.P;
        }

        public void RemoveByte(ushort offset)
        {
            var total = Bytes.RemoveAll(x => x.Offset == offset);
            if (total != 1)
            {
                throw new ArgumentException(string.Format("Cannot remove: {0}", total));
            }
        }

        public SpriteGeneratorState Clone(Action<SpriteGeneratorState> f = null)
        {
            var other = new SpriteGeneratorState(this);

            f?.Invoke(other);

            return other;
        }

        public List<SpriteByte> Bytes { get; private set; }
        public bool IsEmpty { get { return Bytes.Count == 0; } }

        public bool LongA { get { return (P & 0x10) == 0x10; } }
        public bool LongI { get { return (P & 0x20) == 0x20; } }

        // Maintain the state of the execution
        public Register A { get; set; }  // Nullable because unknown values can be passed through the accumulator
        public Register X { get; set; }
        public Register Y { get; set; }
        public Register D { get; set; }
        public Register S { get; set; }  // S is always an offset, not a literal number

        public byte P { get; set; }
    }

    public class Register
    {
        public static readonly Register UNINITIALIZED = new Register(0, DataType.UNINITIALIZED);
        public static readonly Register INITIAL_OFFSET = new Register(0, DataType.SCREEN_OFFSET);

        public enum DataType
        {
            UNINITIALIZED,
            SCREEN_OFFSET,
            LITERAL
        }

        private Register(int value, DataType tag)
        {
            Value = value;
            Tag = tag;
        }

        public Register Clone()
        {
            return new Register(Value, Tag);
        }

        public Register Add(int offset)
        {
            if (IsUninitialized)
            {
                throw new ArgumentException("Cannot add value to uninitialized registers");
            }

            // Adding a value does not change the tag
            return new Register(Value + offset, Tag);
        }

        public Register LoadConstant(int value)
        {
            return new Register(value, DataType.LITERAL);
        }

        public bool IsUninitialized { get { return DataType.UNINITIALIZED.Equals(Tag); } }
        public bool IsScreenOffset { get { return DataType.SCREEN_OFFSET.Equals(Tag); } }
        public bool IsLiteral { get { return DataType.LITERAL.Equals(Tag); } }

        public DataType Tag { get; private set; }
        public int Value { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Tag, Value.ToString("X4"));
        }
    }

    public class SpriteGeneratorStepCost : IStepCostFunction<CodeSequence, SpriteGeneratorState, IntegerPathCost>
    {
        public IntegerPathCost StepCost(SpriteGeneratorState fromState, CodeSequence action, SpriteGeneratorState toState)
        {
            return action.CycleCount;
        }
    }

    public class SpriteGeneratorGoalTest : IGoalTest<SpriteGeneratorState>
    {
        public bool IsGoal(SpriteGeneratorState state)
        {
            // We have reached our goal when there is no data left to display and we are back in 
            // 16-bit mode
            return state.IsEmpty && state.LongA && state.LongI;
        }
    }

    public class SpriteGeneratorSuccessorFunction : ISuccessorFunction<CodeSequence, SpriteGeneratorState>
    {
        public IDictionary<CodeSequence, SpriteGeneratorState> Successors(SpriteGeneratorState state)
        {
            var actions = new List<CodeSequence>();

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

            // It is always permissible to move to/from 16 bit mode
            if (state.LongA)
            {
                actions.Add(new SHORT_M());

                // Add any possible 16-bit data manipulations
            }
            else
            {
                actions.Add(new LONG_M());

                // Add any possible 8-bit manipulations
                if (state.S.IsScreenOffset)
                {
                    var addr = state.S.Value;

                    // We can LDA #$XX / STA X,s for any values within 256 bytes of the curren address
                    foreach (var datum in state.Bytes.Where(x => (x.Offset - addr) < 255))
                    {
                        var offset = datum.Offset - addr;
                        actions.Add(new STACK_REL_8_BIT_IMMEDIATE_STORE(datum.Data, (byte)offset));
                    }
                }
            }

            // Run through the actions to create a dictionary
            return actions.ToDictionary(x => x, x => x.Apply(state));
        }
    }

    public class SpriteGeneratorHeuristicFunction : IHeuristicFunction<SpriteGeneratorState, IntegerPathCost>
    {
        public IntegerPathCost Eval(SpriteGeneratorState state)
        {
            return 0;
        }
    }

    public class SpriteGeneratorSearchNode : HeuristicSearchNode<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>
    {
        public SpriteGeneratorSearchNode(SpriteGeneratorSearchNode node, SpriteGeneratorState state)
            : base(node, state)
        {
        }

        public override string ToString()
        {
            return (action == null) ? "NO ACTION" : action.ToString();
        }
    }

    public class SpriteGeneratorNodeExpander : InformedNodeExpander<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>
    {
        public override SpriteGeneratorSearchNode CreateNode(SpriteGeneratorSearchNode parent, SpriteGeneratorState state)
        {
            return new SpriteGeneratorSearchNode(parent, state);
        }
    }

    public class SpriteGeneratorSearchProblem
    {
        public static ISearchProblem<CodeSequence, SpriteGeneratorState, IntegerPathCost> CreateSearchProblem()
        {
            var goalTest = new SpriteGeneratorGoalTest();
            var stepCost = new SpriteGeneratorStepCost();
            var successors = new SpriteGeneratorSuccessorFunction();
            var heuristic = new SpriteGeneratorHeuristicFunction();

            return new SearchProblem<CodeSequence, SpriteGeneratorState, IntegerPathCost>(goalTest, stepCost, successors, heuristic);
        }

        public static ISearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost> Create()
        {
            //var problem = CreateSearchProblem();
            var strategy = new TreeSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(new SpriteGeneratorNodeExpander());

            return new AStarSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(strategy);
        }
    }
}
