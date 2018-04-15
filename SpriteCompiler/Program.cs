namespace SpriteCompiler
{
    using Fclp;
    using SpriteCompiler.Helpers;
    using SpriteCompiler.Problem;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public static class ExtensionMethods
    {
        public static void Dump(this SpriteCompiler.Helpers.BrutalDeluxeClassifier.ByteColor[,] array)
        {
            var rows = array.GetLength(1);
            var cols = array.GetLength(0);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    char chr = ' ';
                    switch (array[c, r])
                    {
                        case BrutalDeluxeClassifier.ByteColor.BLUE: chr = 'B'; break;
                        case BrutalDeluxeClassifier.ByteColor.GREEN: chr = 'G'; break;
                        case BrutalDeluxeClassifier.ByteColor.ORANGE: chr = 'O'; break;
                        case BrutalDeluxeClassifier.ByteColor.PURPLE: chr = 'P'; break;
                        case BrutalDeluxeClassifier.ByteColor.RED: chr = 'R'; break;
                        case BrutalDeluxeClassifier.ByteColor.YELLOW: chr = 'Y'; break;
                    }
                    Console.Write(chr);
                }
                Console.Write(Environment.NewLine);
            }
        }

        public static void Dump(this int[,] array)
        {
            var rows = array.GetLength(1);
            var cols = array.GetLength(0);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Console.Write(array[c, r].ToString("X2"));
                }
                Console.Write(Environment.NewLine);
            }
        }
    }

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
            var data = new List<byte>();
            var mask = new List<byte>();
            var filename = (string)null;
            Color? maskColor = null;
            int? maxCycles = null;
            var sprite = new List<SpriteByte>();
            bool verbose = false;

            var p = new FluentCommandLineParser();

            p.Setup<List<string>>('d', "data")
                .Callback(_ => data = _.Select(s => Convert.ToByte(s, 16)).ToList());

            p.Setup<List<string>>('m', "mask")
                .Callback(_ => mask = _.Select(s => Convert.ToByte(s, 16)).ToList());

            p.Setup<string>('i', "image")
                .Callback(_ => filename = _);

            p.Setup<string>('l', "limit")
                .Callback(_ => maxCycles = int.Parse(_));

            p.Setup<string>('v', "verbose")
                .Callback(_ => verbose = true);

            p.Setup<string>("bg-color")
                .Callback(_ => maskColor = Color.FromArgb(0xFF, Color.FromArgb(Convert.ToInt32(_, 16))));
            
            p.Parse(args);

            Console.WriteLine("Manual data has " + data.Count + " bytes");
            Console.WriteLine("Manual mask has " + mask.Count + " bytes");
            Console.WriteLine("Input filename is " + filename);
            Console.WriteLine("Image mask color is " + (maskColor.HasValue ? maskColor.ToString() : "(none)"));

            // Set the global state
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = maxCycles.HasValue ?
                SpriteGeneratorSearchProblem.Create(maxCycles.Value) :                
                SpriteGeneratorSearchProblem.Create();

            SpriteGeneratorState initialState = null;

            // Handle the difference command line cases
            if (!String.IsNullOrEmpty(filename))
            {
                var bitmap = new Bitmap(filename);
                var record = BrutalDeluxeClassifier.Decompose(bitmap, maskColor);

                record.Data.Dump();
                Console.WriteLine();
                record.Mask.Dump();
                Console.WriteLine();
                record.Classes.Dump();

                //initialState = SpriteGeneratorState.Init(sprite);
            }
            else if (data.Count == mask.Count)
            {
                initialState = SpriteGeneratorState.Init(data, mask);
            }
            else
            {
                initialState = SpriteGeneratorState.Init(data);
            }

            //var solution = search.Search(problem, initialState);
            //WriteOutSolution(solution);
        }
    }
}
