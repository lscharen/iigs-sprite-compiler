namespace SpriteCompiler
{
    using Fclp;
    using SpriteCompiler.Problem;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public static class ExtensionMethods
    {
        public static void Dump(this int[,] array)
        {
            var rows = array.GetLength(1);
            var cols = array.GetLength(0);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Console.Write(array[c, r].ToString("X1"));
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
                var palette = new Dictionary<Color, int>();
                int nextIndex = 1;

                // Convert the image / mask to a paletted image
                var bitmap = new Bitmap(filename);
                int[,] data_buffer = new int[bitmap.Width, bitmap.Height];
                int[,] mask_buffer = new int[bitmap.Width, bitmap.Height];

                Console.WriteLine(String.Format("  Image is {0} x {1}", bitmap.Width, bitmap.Height));

                if (maskColor.HasValue)
                {
                    palette[maskColor.Value] = 0;
                }

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

                        data_buffer[w, r] = palette[rgb];

                        if (maskColor.HasValue)
                        {
                            if (rgb.Equals(maskColor.Value))
                            {
                                data_buffer[w, r] = 0x0;
                                mask_buffer[w, r] = 0xF;
                            }
                            else
                            {
                                data_buffer[w, r] = palette[rgb];
                                mask_buffer[w, r] = 0x0;
                            }
                        }
                        else
                        {
                            data_buffer[w, r] = palette[rgb];
                        }
                    }
                }

                data_buffer.Dump();
                Console.WriteLine();
                mask_buffer.Dump();

                // Pair up pixels to build bytes                
                for (int r = 0; r < bitmap.Height; r++)
                {
                    for (int w = 0; w < bitmap.Width; w += 2)
                    {
                        var mask_byte = (byte)((mask_buffer[w, r] << 4) + mask_buffer[w + 1, r]);
                        var data_byte = (byte)((data_buffer[w, r] << 4) + data_buffer[w + 1, r]);
                        var offset = (ushort)(r * 160 + (w / 2));

                        // Skip fully transparent bytes
                        if (mask_byte == 0xFF)
                        {
                            continue;
                        }

                        Console.WriteLine(String.Format("Adding ({0:X2}, {1:X2}, {2})", data_byte, mask_byte, offset));

                        sprite.Add(new SpriteByte(data_byte, mask_byte, offset));
                    }
                }

                initialState = SpriteGeneratorState.Init(sprite);
            }
            else if (data.Count == mask.Count)
            {
                initialState = SpriteGeneratorState.Init(data, mask);
            }
            else
            {
                initialState = SpriteGeneratorState.Init(data);
            }

            IEnumerable<SpriteGeneratorSearchNode> solution = null;
            if (verbose)
            {
                search.InitializeSearch(initialState);
                while (true)
                {
                    var step = search.SearchStep(problem);
                    if (step.IsGoal)
                    {
                        break;
                    }
                }
            }
            else
            {
                solution = search.Search(problem, initialState);
            }

            WriteOutSolution(solution);
        }
    }
}
