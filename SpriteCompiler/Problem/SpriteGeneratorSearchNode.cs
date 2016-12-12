namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorSearchNode : HeuristicSearchNode<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost>
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
}
