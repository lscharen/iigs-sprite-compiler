namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorNodeExpander : InformedNodeExpander<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>
    {
        public override SpriteGeneratorSearchNode CreateNode(SpriteGeneratorSearchNode parent, SpriteGeneratorState state)
        {
            return new SpriteGeneratorSearchNode(parent, state);
        }
    }
}
