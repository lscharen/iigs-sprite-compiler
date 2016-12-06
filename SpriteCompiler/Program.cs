namespace SpriteCompiler
{
    using Fclp;
    using SpriteCompiler.Problem;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
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
            IEnumerable<SpriteByte> data = null;

            try
            {
                data = args.Select((s, i) => new SpriteByte(Convert.ToByte(s, 16), (ushort)i));
            }
            catch (FormatException e)
            {
                // If there is only one or two arguments, them marybe the user passed in a file
                if (args.Length <= 2)
                {
                    var palette = new Dictionary<Color, int>();
                    int nextIndex = 1;

                    // Convert the image / mask to a paletted image
                    var bitmap = new Bitmap(args[0]);
                    int[,] buffer = new int[bitmap.Width, bitmap.Height];

                    for (int r = 0; r < bitmap.Height; r++)
                    {
                        for (int w = 0; w < bitmap.Width; w++)
                        {
                            var rgb = bitmap.GetPixel(w, r);

                            if (!palette.ContainsKey(rgb))
                            {
                                if (palette.Count >= 15)
                                {
                                    throw new Exception("Image cannot have more than 15 unique colors");
                                }
                                palette[rgb] = nextIndex++;
                            }

                            buffer[w, r] = palette[rgb];
                        }
                    }

                    // Pair up pixles to build bytes
                    var sprite = new List<SpriteByte>();

                    for (int r = 0; r < bitmap.Height; r++)
                    {
                        for (int w = 0; w < bitmap.Width; w += 2)
                        {
                            sprite.Add(new SpriteByte((byte)((buffer[w, r] << 4) + buffer[w + 1, r]), (ushort)(r * 160 + (w / 2))));
                        }
                    }

                    data = sprite;
                }
            }
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
