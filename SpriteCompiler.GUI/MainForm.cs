using SpriteCompiler.AI;
using SpriteCompiler.AI.Queue;
using SpriteCompiler.Helpers;
using SpriteCompiler.Problem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpriteCompiler.GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void newToolStripMenuItem_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
        }

        private Bitmap source = null;

        private Bitmap Expand(Bitmap src, int width, int height)
        {
            // Create a bitmap exactly the same size as the picture box
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(src, 0, 0, bitmap.Width, bitmap.Height);
            }

            return bitmap;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Load the image from the file
                source = new Bitmap(openFileDialog.FileName);

                pictureBox1.Image = Expand(source, pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                // Load and analyze the image...
            }
        }

        private SpriteGeneratorSearchProblem problem;
        private SpriteGeneratorState initialState = null;
        private ISearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost> search;
        private InspectableTreeSearch strategy;

        private void button1_Click(object sender, EventArgs e)
        {
            var bgcolor = source.GetPixel(0, 0);
            var record = BrutalDeluxeClassifier.Decompose(source, bgcolor);
            
            var classified = new Bitmap(source.Width, source.Height);
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < (source.Width / 2); x++)
                {
                    // A mask value of 255 (0xFF) is a tansparent pair of pixels, use the background color
                    var color = BrutalDeluxeClassifier.ToRGB(record.Classes[x, y]);
                    classified.SetPixel(2 * x, y, color);
                    classified.SetPixel(2 * x + 1, y, color);
                }
            }

            pictureBox3.Image = Expand(classified, pictureBox3.ClientSize.Width, pictureBox3.ClientSize.Height); ;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;


            var rb_record = BrutalDeluxeClassifier.DecomposeIntoRedBlueImageMap(source, bgcolor);

            var rb_classified = new Bitmap(source.Width, source.Height);
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < (source.Width / 2); x++)
                {
                    // A mask value of 255 (0xFF) is a tansparent pair of pixels, use the background color
                    var color = BrutalDeluxeClassifier.ToRGB(rb_record.Classes[x, y]);
                    rb_classified.SetPixel(2 * x, y, color);
                    rb_classified.SetPixel(2 * x + 1, y, color);
                }
            }

            pictureBox2.Image = Expand(rb_classified, pictureBox2.ClientSize.Width, pictureBox2.ClientSize.Height); ;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            var histogram = BrutalDeluxeClassifier.GenerateStatistics(rb_record);
            textBox1.Clear();

            textBox1.AppendText("Most Common Values\n");
            textBox1.AppendText("------------------\n");
            foreach (var stat in histogram.OrderByDescending(x => x.Value).Take(10))
            {
                textBox1.AppendText(String.Format("0x{0:X} : {1}\n", stat.Key, stat.Value));
            }

            // Initialize the search
            var maxCycles = 5 + source.Height * (3 + (source.Width / 4 * 31) - 1 + 41) - 1;

            problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            initialState = SpriteGeneratorState.Init(rb_record.SpriteData);

            var expander = new SpriteGeneratorNodeExpander();
            strategy = new InspectableTreeSearch(expander);

            strategy.Initialize(problem, initialState);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Execute one step of the search
            var node = strategy.SearchStep(problem);
            foreach (var n in strategy.Solution(node))
            {
                textBox1.AppendText(String.Format("{0}:   {1}          ; {2} cycles\n", n.Depth, n.Action, n.PathCost)); 
            }
            textBox1.AppendText(node.State.ToString() + "\n");
        }

        public class InspectableTreeSearch : AbstractSearchStrategy<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost>
        {
            private IQueue<SpriteGeneratorSearchNode> fringe;

            public InspectableTreeSearch(INodeExpander<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost> expander)
                : base(expander)
            {
            }

            public void Initialize(SpriteGeneratorSearchProblem problem, SpriteGeneratorState initialState)
            {
                // Create a new fringe
                // fringe = new Adapters.QueueAdapter<SpriteGeneratorSearchNode, IntegerCost>();
                fringe = new LIFO<SpriteGeneratorSearchNode>();

                // Add the initial state to the fringe
                fringe.Enqueue(Expander.CreateNode(initialState));
            }

            public SpriteGeneratorSearchNode SearchStep(SpriteGeneratorSearchProblem problem)
            {
                // If the fringe is empty, return null to indicate that no solution was found
                if (fringe.Empty)
                {
                    return null;
                }

                // Select the next node
                var node = fringe.Remove();

                // If the node is a solution, then we're done! Otherwise expand the node and add to the queue
                if (!problem.IsGoal(node.State))
                {
                    AddNodes(fringe, node, problem);
                }

                // Return the node that we selected to the caller
                return node;
            }

            /// <summary>
            /// Generic tree search. See page 72 in Russell and Norvig
            /// </summary>
            protected override void AddNodes(IQueue<SpriteGeneratorSearchNode> fringe, SpriteGeneratorSearchNode node, ISearchProblem<CodeSequence, SpriteGeneratorState, IntegerCost> problem)
            {
                fringe.AddRange(Expander.Expand(problem, node));
            }
        }
    }
}
