using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.Helpers
{
    public static class BrutalDeluxeClassifier
    {
        public static bool IsYellow(this ByteColor color) { return ByteColor.YELLOW.Equals(color); }
        public static bool IsOrange(this ByteColor color) { return ByteColor.ORANGE.Equals(color); }
        public static bool IsRed(this ByteColor color) { return ByteColor.RED.Equals(color); }
        public static bool IsBlue(this ByteColor color) { return ByteColor.BLUE.Equals(color); }
        public static bool IsGreen(this ByteColor color) { return ByteColor.GREEN.Equals(color); }

        public sealed class Palette 
        {
            private IDictionary<Color, int> palette = new Dictionary<Color, int>();
            public Color? MaskColor { get; set; }

            public int this[Color key]
            {
                get
                {
                    return palette[key];
                }
                set
                {
                    palette[key] = value;
                }
            }

            public bool Contains(Color color)
            {
                return palette.ContainsKey(color);
            }

            public int Count { get { return palette.Count; } }

            public int RGBToData(Color rgb)
            {
                if (MaskColor.HasValue && rgb.Equals(MaskColor.Value))
                {
                    // Always set masked data values to zero by convention
                    return 0x0;
                }
                else
                {
                    return palette[rgb];
                }
            }

            public int RGBToMask(Color rgb)
            {
                if (MaskColor.HasValue && rgb.Equals(MaskColor.Value))
                {
                    return 0xF;
                }
                else
                {
                    return 0x0;
                }
            }
        }

        public enum ByteColor
        {
            GREEN,
            YELLOW,
            BLUE,
            ORANGE,
            PURPLE,
            RED
        }

        public static Color ToRGB(ByteColor color)
        {
            switch (color)
            {
                case ByteColor.GREEN: return Color.PaleGreen;
                case ByteColor.YELLOW: return Color.Yellow;
                case ByteColor.BLUE: return Color.Blue;
                case ByteColor.ORANGE: return Color.Orange;
                case ByteColor.PURPLE: return Color.Purple;
                case ByteColor.RED: return Color.Red;
            }

            return Color.Transparent;
        }

        public struct ColoredSpriteData
        {
            public int Offset;
            public ByteColor Color;
            public int Data;
            public int Mask;          
        }

        public static ByteColor RedBlueClassifier(int mask)
        {
            if (mask == 0x00) return ByteColor.RED;
            if (mask == 0xFF) return ByteColor.GREEN;
            return ByteColor.BLUE;
        }

        public static ByteColor YellowBlueClassifier(int mask)
        {
            if (mask == 0x00) return ByteColor.YELLOW;
            if (mask == 0xFF) return ByteColor.GREEN;
            return ByteColor.BLUE;
        }

        public static Tuple<List<ByteColor>, List<ByteColor>>[] ReductionRules = new []
        {
            // B-R-R-B -> P-P-P-P
            Tuple.Create(new List<ByteColor> { ByteColor.BLUE, ByteColor.RED, ByteColor.RED, ByteColor.GREEN }, new List<ByteColor> { ByteColor.PURPLE, ByteColor.PURPLE, ByteColor.PURPLE, ByteColor.PURPLE }),
            Tuple.Create(new List<ByteColor> { ByteColor.GREEN, ByteColor.RED, ByteColor.GREEN }, new List<ByteColor> { ByteColor.GREEN, ByteColor.YELLOW, ByteColor.YELLOW })
        };

        public static List<ByteColor> Extend(List<ByteColor> neighbors, ByteColor current)
        {
            // Base case: Just append to an empty list
            int n = neighbors.Count;
            if (n == 0)
            {
                neighbors.Add(current);
                return neighbors;
            }

            // Interesting cases
            //
            // ORANGE + ORANGE + YELLOW + YELLOW = RED + RED + RED + RED
            // RED + YELLOW + YELLOW = RED + RED + RED
            // YELLOW + YELLOW = ORANGE + ORANGE
            // YELLOW + BLUE   = PURPLE + PURPLE
            // BLUE + YELLOW   = PURPLE + PURPLE
            // BLUE + BLUE     = PURPLE + PURPLE
            // 
            // Everything else just appends the current classification
            if (current.IsYellow())
            {
                // Check to see if we have a new red span
                if (n >= 3 && neighbors[n-1].IsYellow() && neighbors[n-2].IsOrange() && neighbors[n-3].IsOrange())
                {
                    neighbors[n - 3] = neighbors[n - 2] = neighbors[n - 1] = ByteColor.RED;
                    neighbors.Add(ByteColor.RED);
                    return neighbors;
                }

                if (n >= 2 && neighbors[n-1].IsYellow() && neighbors[n-2].IsRed())
                {
                    neighbors[n - 1] = ByteColor.RED;
                    neighbors.Add(ByteColor.RED);
                    return neighbors;
                }

                if (neighbors[n-1].IsYellow())
                {
                    neighbors[n - 1] = ByteColor.ORANGE;
                    neighbors.Add(ByteColor.ORANGE);
                    return neighbors;
                }

                if (neighbors[n - 1].IsBlue())
                {
                    neighbors[n - 1] = ByteColor.PURPLE;
                    neighbors.Add(ByteColor.PURPLE);
                    return neighbors;
                }
            }
            else if (current.IsBlue())
            {
                if (neighbors[n - 1].IsYellow())
                {
                    neighbors[n - 1] = ByteColor.ORANGE;
                    neighbors.Add(ByteColor.ORANGE);
                    return neighbors;
                }

                if (neighbors[n - 1].IsBlue())
                {
                    neighbors[n - 1] = ByteColor.PURPLE;
                    neighbors.Add(ByteColor.PURPLE);
                    return neighbors;
                }
            }

            neighbors.Add(current);
            return neighbors;
        }

        public static Palette ExtractColorPalette(Bitmap bitmap, Color? maskColor = null)
        {
            var palette = new Palette();
            int nextIndex = 1;

            for (int r = 0; r < bitmap.Height; r++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    var rgb = bitmap.GetPixel(w, r);

                    if (!palette.Contains(rgb))
                    {
                        if (palette.Count >= 16)
                        {
                            throw new Exception("Image cannot have more than 15 unique colors");
                        }

                        palette[rgb] = nextIndex++;
                    }
                }
            }

            palette.MaskColor = maskColor;
            return palette;
        }

        public static int[,] Extract4BitPixelData(Bitmap bitmap, Palette palette)
        {
            var data_buffer = new int[bitmap.Width, bitmap.Height];

            for (int r = 0; r < bitmap.Height; r++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    data_buffer[w, r] = palette.RGBToData(bitmap.GetPixel(w, r));
                }
            }

            return data_buffer;
        }

        public static int[,] Extract4BitPixelMask(Bitmap bitmap, Palette palette)
        {
            var mask_buffer = new int[bitmap.Width, bitmap.Height];

            for (int r = 0; r < bitmap.Height; r++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    mask_buffer[w, r] = palette.RGBToMask(bitmap.GetPixel(w, r));
                }
            }

            return mask_buffer;
        }

        public static int[,] Decimate(int[,] fourBitData)
        {
            // Conver the 4-bit data into bytes
            int width  = fourBitData.GetLength(0);
            int height = fourBitData.GetLength(1);
            var byte_buffer = new int[width / 2, height];

            for (int r = 0; r <height; r++)
            {
                for (int w = 0; w < width; w += 2)
                {
                    byte_buffer[w / 2, r] = ((fourBitData[w, r] << 4) + fourBitData[w + 1, r]);
                }
            }

            return byte_buffer;
        }

        public class SpriteBitmapRecord
        {
            public int[,] Data;
            public int[,] Mask;
            public ByteColor[,] Classes;
            public List<ColoredSpriteData> SpriteData;
        }

        public static IDictionary<int, int> GenerateStatistics(SpriteBitmapRecord record)
        {
            // Look at the data and find top 16-bit patterns in the image (scan the data/mask arrays and create a list of all 16-bit values)
            // NOTE: These are overlapping values, a pattern of 0xAA 0x55 0xAA will give values of 0x55AA and 0xAA55
            var data = record.Data;
            var mask = record.Mask;

            var width = data.GetLength(0);
            var height = data.GetLength(1);

            var histogram = new Dictionary<int, int>();
            for (int row = 0; row < height; row++)
            {
                for (int col = 1; col < width; col++)
                {
                    if (mask[col, row] == 0x00 && mask[col - 1, row] == 0x00)
                    {
                        var value = data[col, row] * 256 + data[col - 1, row];
                        if (!histogram.ContainsKey(value))
                        {
                            histogram[value] = 0;
                        }

                        histogram[value] += 1;
                    }
                }
            }

            return histogram;  
        }

        public static SpriteBitmapRecord DecomposeIntoRedBlueImageMap(Bitmap bitmap, Color? maskColor = null)
        {
            var sprite = new List<ColoredSpriteData>();

            // Build the palette of this sprite image
            var palette = ExtractColorPalette(bitmap, maskColor);

            // Get the 4-bit data and mask arrays
            var data = Decimate(Extract4BitPixelData(bitmap, palette));
            var mask = Decimate(Extract4BitPixelMask(bitmap, palette));

            // Start scanning the data, row by row.  Each line is so far away from the
            // previous line that we always reset
            var width = data.GetLength(0);
            var height = data.GetLength(1);
            var classes = new ByteColor[width, height];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    classes[col, row] = RedBlueClassifier(mask[col, row]);
                }

                // Once a full row is classified, create the sprite data
                for (int col = 0; col < width; col++)
                {
                    if (mask[col, row] == 0xFF)
                    {
                        continue;
                    }

                    sprite.Add(new ColoredSpriteData
                    {
                        Color = classes[col, row],
                        Offset = row * 160 + col,
                        Mask = mask[col, row],
                        Data = data[col, row]
                    });
                }
            }

            return new SpriteBitmapRecord
            {
                Data = data,
                Mask = mask,
                Classes = classes,
                SpriteData = sprite
            };
        }

        public static SpriteBitmapRecord Decompose(Bitmap bitmap, Color? maskColor = null)
        {
            // Classified the data according to the method described at http://www.brutaldeluxe.fr/products/crossdevtools/mrspritetech/index.html
            //
            // GREEN  : is skipped (this is a sparse structure)
            // YELLOW : Solid, isolated byte (-- -- XX -- --)
            // BLUE   : Mixed, isolated byte (-- -- -X -- --)
            // ORANGE : Solid, isolated word (-- -- XX XX -- --)
            // PURPLE : Mixed, isolated word (-- -- -X XX -- --) at least 1 pixel out of 4.
            // RED:   : At least 4 solid pixels (-- -- XX XX XX XX ...)
            //
            // A yellow can turn into a purple or red
            // A blue can be turned into a purple
            var sprite = new List<ColoredSpriteData>();

            // Build the palette of this sprite image
            var palette = ExtractColorPalette(bitmap, maskColor);

            // Get the 4-bit data and mask arrays
            var data = Decimate(Extract4BitPixelData(bitmap, palette));
            var mask = Decimate(Extract4BitPixelMask(bitmap, palette));

            // Start scanning the data, row by row.  Each line is so far away from the
            // previous line that we always reset
            var width = data.GetLength(0);
            var height = data.GetLength(1);
            var classes = new ByteColor[width, height];

            for (int row = 0; row < height; row++)
            {
                var classification = new List<ByteColor>();

                for (int col = 0; col < width; col++)
                {
                    classes[col, row] = YellowBlueClassifier(mask[col, row]);
                    classification = Extend(classification, classes[col, row]);
                }

                // Once a full row is classified, create the sprite data
                for (int col = 0; col < width; col++)
                {
                    classes[col, row] = classification[col];

                    if (classification[col].IsGreen())
                    {
                        continue;
                    }

                    sprite.Add(new ColoredSpriteData
                    {
                        Color = classification[col],
                        Offset = row * 160 + col,
                        Mask = mask[col, row],
                        Data = data[col, row]
                    });
                }
            }

            return new SpriteBitmapRecord
            {
                Data = data,
                Mask = mask,
                Classes = classes,
                SpriteData = sprite
            };
        }
    }
}
