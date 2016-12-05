namespace SpriteCompiler
{
    using Fclp;
    using SpriteCompiler.Problem;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ApplicationArguments
    {
        public List<string> Data { get; set; }
        public List<string> Mask { get; set; }
    }

    public class Program
    {
        public static void WriteOutSolution(IEnumerable<SpriteGeneratorSearchNode> solution)
        {
            foreach (var step in solution.Skip(1))
            {
                Console.WriteLine(step.Action.Emit());
            }

            Console.WriteLine(string.Format("; Total Cost = {0} cycles", (int)solution.Last().PathCost));
        }

        static void Main(string[] args)
        {
            byte[] data = null;

            Console.WriteLine(string.Join(", ", args.Select(s => "'" + s + "'")));
            data = args.Select(s => Convert.ToByte(s, 16)).ToArray();

            /*
            return;

            var p = new FluentCommandLineParser<ApplicationArguments>();

            // specify which property the value will be assigned too.
            p.Setup<List<string>>(arg => arg.Data)
             .As('d', "data") // define the short and long option name
             .Required()      // using the standard fluent Api to declare this Option as required.
             .Callback(d => d.Select(s => Convert.ToByte(s, 16)).ToArray());

            p.Setup<List<string>>(arg => arg.Mask)
             .As('m', "mask");
            
            var result = p.Parse(args);

            if (!result.HasErrors)
            {
             */

                var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
                var search = SpriteGeneratorSearchProblem.Create();

                var solution = search.Search(problem, new SpriteGeneratorState(data));

                WriteOutSolution(solution);
            //}           
        }
    }
}
