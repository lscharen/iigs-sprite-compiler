namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorNodeExpander : InformedNodeExpander<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost>
    {
        public override SpriteGeneratorSearchNode CreateNode(SpriteGeneratorSearchNode parent, SpriteGeneratorState state)
        {
            return new SpriteGeneratorSearchNode(parent, state);
        }

        public override SpriteGeneratorSearchNode CreateNode(SpriteGeneratorState state)
        {
            return CreateNode(null, state);
        }
    }
}
