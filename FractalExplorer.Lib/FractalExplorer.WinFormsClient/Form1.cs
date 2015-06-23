using FractalExplorer.Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace FractalExplorer.WinFormsClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private float Aspect = 1F; //Height/Width aspect ratio of screen that should be maintained
        private int MaxIterations = 2;
        private double Bailout = 2.5; //Should be between 2 and 3 depending on completeness of set desired
        private Rectangle ScreenRect;
        private RectangleF ComplexRect;
        private float Magnification = 1;
        private int ResolutionEnhancementFactor = 25; //Range this from 1 up to 100 to determine how quickly screen redraws gets to high res

        //At 1, for instance, the image appears to grow like a crystal as more res is added
        //At 100, for instance, it might just draw once at the highest res
        private int MaxResolutionFactor = 250; //Range this from 100 up to 1000 to determine highest res.  The lower this is, the faster things will

        //render, and the higher then slower.
        private ComplexFunctions.ComplexFn SelectedComplexFn; //= (c) => Complex.Pow(c, new Complex(2, 0));

        private bool EnsuringProperAspect = false;
        private List<Color> Colors = new List<Color>();

        private void Form1_Load(object sender, EventArgs e)
        {
            loadMenus();
            ScreenRect = new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height); //Init internal representation of screen area
            ComplexRect = new RectangleF(-2.5F, -2.5F, 5F, 5F); //Init internal representation of complex area
            EnsureProperAspect();
            InitColors(true); //Init a bunch of colors, random or not
            //InitComplexFunction(true); //Init the function you want to use, random or not
            MaxIterations = 1; //Reset resolution
            changeSelectedFunc((x) => Complex.Pow(x, 2), "Pow(2)");
        }

        private void loadMenus()
        {
            ToolStripMenuItem mnuPow = new ToolStripMenuItem() { Text = "Pow" };
            mnuPow.DropDownOpening += (x, y) => timer1.Enabled = false;
            for (int pow = 2; pow <= 10; pow++)
            {
                string ps = pow.ToString().Clone().ToString();
                int pc = pow;
                ToolStripMenuItem m = new ToolStripMenuItem(pow.ToString(), null, (s, e) => changeSelectedFunc((x) => Complex.Pow(x, pc), "Pow(" + ps + ")"));
                mnuPow.DropDownItems.Add(m);
            }
            mnuMain.Items.Add(mnuPow);

            ToolStripMenuItem mnuBasicFuncs = new ToolStripMenuItem() { Text = "Basic Functions" };
            mnuBasicFuncs.DropDownOpening += (x, y) => timer1.Enabled = false;
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("ASin", null, (x, y) => changeSelectedFunc((z) => Complex.Asin(z), "ASin")));
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("ATan", null, (x, y) => changeSelectedFunc((z) => Complex.Atan(z), "ATan")));
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("Exp", null, (s, e) => changeSelectedFunc((x) => Complex.Exp(x), "Exp")));
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("Sin", null, (s, e) => changeSelectedFunc((x) => Complex.Sin(x), "Sin")));
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("Sinh", null, (s, e) => changeSelectedFunc((x) => Complex.Sinh(x), "Sinh")));
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("Sqrt", null, (s, e) => changeSelectedFunc((x) => Complex.Sqrt(x), "Sqrt")));
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("Tan", null, (s, e) => changeSelectedFunc((x) => Complex.Tan(x), "Tan")));
            mnuBasicFuncs.DropDownItems.Add(new ToolStripMenuItem("Tanh", null, (s, e) => changeSelectedFunc((x) => Complex.Tanh(x), "Tanh")));
            mnuMain.Items.Add(mnuBasicFuncs);

            ToolStripMenuItem mnuMoreComplexFuncs = new ToolStripMenuItem() { Text = "More Complex Functions" };
            
            mnuMoreComplexFuncs.DropDownOpening += (x, y) => timer1.Enabled = false;
            mnuMoreComplexFuncs.DropDownItems.Add(new ToolStripMenuItem("Exp(Pow(2))", null, (s, e) => changeSelectedFunc((x) => Complex.Exp(Complex.Pow(x, 2)), "Exp(Pow(2))"))); 
            mnuMoreComplexFuncs.DropDownItems.Add(new ToolStripMenuItem("Exp(Pow(3))", null, (s, e) => changeSelectedFunc((x) => Complex.Exp(Complex.Pow(x, 3)), "Exp(Pow(3))")));
            mnuMoreComplexFuncs.DropDownItems.Add(new ToolStripMenuItem("Sqrt(Sin(Pow(2)))", null, (s, e) => changeSelectedFunc((x) => Complex.Sqrt(Complex.Sin(Complex.Pow(x, 2))), "Sqrt(Sin(Pow(2)))")));
            mnuMoreComplexFuncs.DropDownItems.Add(new ToolStripMenuItem("Sqrt(Sin(Pow(3)))", null, (s, e) => changeSelectedFunc((x) => Complex.Sqrt(Complex.Sin(Complex.Pow(x, 3))), "Sqrt(Sin(Pow(3)))")));
            mnuMain.Items.Add(mnuMoreComplexFuncs);

            ToolStripMenuItem mnuCustomFunc = new ToolStripMenuItem("Custom Function", null, mnuCustomFunction_Click);
            mnuMain.Items.Add(mnuCustomFunc);
        }

        protected void mnuCustomFunction_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi == null)
                return;

            timer1.Enabled = false;
            MessageBox.Show("Not yet implemented", "Custom Function");
            //Do something like this to support custom fns: http://www.blackbeltcoder.com/Articles/algorithms/a-c-expression-evaluator
            timer1.Enabled = true;
        }

        protected void changeSelectedFunc(ComplexFunctions.ComplexFn func, string functionName = "")
        {
            timer1.Enabled = false;
            Text = "Fractal Explorer " + functionName;
            SelectedComplexFn = func;
            MaxIterations = 1; //Reset resolution
            EnsureProperAspect();
            this.Invalidate();
            timer1.Enabled = true;
        }

        private void InitColors(bool random)
        {
            if (random)
            {
                Random r = new Random();
                for (int i = 0; i < MaxResolutionFactor; i++)
                    Colors.Add(Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
            }
            else
            {
                for (int i = 0; i < MaxResolutionFactor; i++)
                    Colors.Add(Color.FromArgb((int)(255.0 * i / MaxResolutionFactor), 0, 0)); //This needs work, random is cooler anyway
            }
        }

        private Color GetColor(int iterations, int maxIterations)
        {
            Color ret = Colors[iterations - 1];
            return ret;
        }

        private void EnsureProperAspect()
        {
            EnsuringProperAspect = true;
            //Rescale height of window to maintain aspect given width
            ScreenRect.Width = this.ClientSize.Width; //Sync ScreenRect.Width with actual screen width
            int toolbarHeight = this.Height - this.ClientSize.Height;
            this.Height = Convert.ToInt32(ScreenRect.Width * Aspect) + toolbarHeight; //Reset screen height to maintain height/width aspect
            ScreenRect.Height = this.ClientSize.Height; //Sync ScreenRect.Height with actual screen height
            ComplexRect.Height = (ComplexRect.Width * Aspect); //Rescale complex rectangle to match
            //dbug.WriteLine("Aspect {0}, ScreenRect {1}, ComplexRect {2}", Aspect, ScreenRect, ComplexRect);
            EnsuringProperAspect = false;
        }

        private float CalculateEpsilon(RectangleF complexRect, Rectangle screenRect)
        {
            //Epsilon is ratio of complex coordinate rect to screen coordinate rect.  We are rendering an imaginary
            //set of numbers to the screen by mapping the real part to X and the imaginary part to Y
            return complexRect.Width / Convert.ToSingle(screenRect.Width);
        }

        private Point ConvertComplexPointToScreenPoint(PointF complexPoint, RectangleF complexRect, Rectangle screenRect)
        {
            //Convert a complex point within a complex rect area to a point on the screen
            float scale = Convert.ToSingle(screenRect.Width) / complexRect.Width;
            Point ret = new Point(Convert.ToInt32((complexPoint.X - complexRect.X) * scale),
                Convert.ToInt32((complexPoint.Y - complexRect.Y) * scale));
            return ret;
        }

        private PointF ConvertScreenPointToComplexPoint(Point screenPoint, RectangleF complexRect, Rectangle screenRect)
        {
            //Convert a point on the screen to a complex point within a complex rect area
            float scale = Convert.ToSingle(screenRect.Width) / complexRect.Width;
            PointF ret = new PointF(Convert.ToSingle(screenPoint.X) / scale + complexRect.X,
                Convert.ToSingle(screenPoint.Y) / scale + complexRect.Y);
            return ret;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            timer1.Enabled = false; //Stop screen updates
            redrawFractal(e.Graphics);
            timer1.Enabled = true; //Restart screen updates
        }

        private void redrawFractal(Graphics g)
        {
            if (SelectedComplexFn != null)
            {
                FractalIterator iter = FractalIterator.GetSingletonInstance();
                float epsilon = CalculateEpsilon(ComplexRect, ScreenRect);
                for (float x = ComplexRect.X; x <= ComplexRect.X + ComplexRect.Width; x += epsilon)
                {
                    for (float y = ComplexRect.Y; y <= ComplexRect.Y + ComplexRect.Height; y += epsilon)
                    {
                        PointF complexPoint = new PointF(x, y);
                        int iterations = iter.IterateMandelbrotPoint(complexPoint, MaxIterations, Bailout, SelectedComplexFn);
                        if (iterations < MaxIterations)
                            drawPoint(g, ConvertComplexPointToScreenPoint(complexPoint, ComplexRect, ScreenRect), GetColor(iterations, MaxIterations));
                    }
                }
            }
        }

        private void drawPoint(Graphics g, Point screenPoint, Color color)
        {
            g.FillRectangle(new SolidBrush(color), screenPoint.X, screenPoint.Y, 1, 1);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!EnsuringProperAspect)
            {
                EnsureProperAspect();
                this.Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            //Left clicking zooms in and right-clicking zooms out
            timer1.Enabled = false;
            Point screenPoint = e.Location;
            bool zoomIn = (e.Button == System.Windows.Forms.MouseButtons.Left); //Left button click zooms in, otherwise zoom out
            PointF complexPoint = ConvertScreenPointToComplexPoint(screenPoint, ComplexRect, ScreenRect);
            if (zoomIn)
            {
                //Zoom in with center at click point
                float factor = 2.0F;
                ComplexRect.Width = ComplexRect.Width / factor; //Cut complex range by factor
                ComplexRect.Height = ComplexRect.Height / factor;
                ComplexRect.X = complexPoint.X - ComplexRect.Width / 2; //Recenter on point
                ComplexRect.Y = complexPoint.Y - ComplexRect.Height / 2;
                Magnification = Magnification * factor;
            }
            else
            {
                //Zoom out with center at click point
                float factor = 2.0F;
                ComplexRect.Width = ComplexRect.Width * factor; //Increase complex range by factor
                ComplexRect.Height = ComplexRect.Height * factor;
                ComplexRect.X = complexPoint.X - ComplexRect.Width / 2; //Recenter on point
                ComplexRect.Y = complexPoint.Y - ComplexRect.Height / 2;
                Magnification = Magnification / factor;
            }
            MaxIterations = 1; //Reset resolution
            //this.Text = string.Format("{0} at {1}X magnification {2}", complexPoint, Magnification, ComplexRect);
            EnsureProperAspect();

            this.Invalidate();
            timer1.Enabled = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //Move up, down, left, right using arrow keys to explore set
            timer1.Enabled = false;
            if (e.KeyCode == Keys.Up)
            {
                ComplexRect.Y = ComplexRect.Y - ComplexRect.Height;
            }
            else if (e.KeyCode == Keys.Down)
            {
                ComplexRect.Y = ComplexRect.Y + ComplexRect.Height;
            }
            else if (e.KeyCode == Keys.Left)
            {
                ComplexRect.X = ComplexRect.X - ComplexRect.Width;
            }
            else if (e.KeyCode == Keys.Right)
            {
                ComplexRect.X = ComplexRect.X + ComplexRect.Width;
            }
            MaxIterations = 1; //Reset resolution
            this.Text = string.Format("{0}X magnification {1}", Magnification, ComplexRect);
            EnsureProperAspect();
            this.Invalidate();
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Reset max iterations and redraw screen as needed to jack up resolution to a certain point
            //So the first time a new image is drawn it might be medium res and then a few seconds later it
            //gets redrawn with higher res.  This was to improve performance while just jumping around and
            //exploring not needing a super high res image every time the coordinates changed.
            MaxIterations += ResolutionEnhancementFactor * Convert.ToInt32(Math.Log(1 + Convert.ToDouble(MaxIterations)));
            if (MaxIterations < MaxResolutionFactor)
            {
                //dbug.WriteLine("MaxIterations = {0}", MaxIterations);
                this.Invalidate();
            }
            else
                timer1.Enabled = false;
        }

        private void mnuBuiltInFunctions_MouseEnter(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void mnuBuiltInFunctions_MouseLeave(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void mnuZoomWayOut_Click(object sender, EventArgs e)
        {
            //Reset to initial view
            timer1.Enabled = false;
            MaxIterations = 1; //Reset resolution
            ComplexRect = new RectangleF(-2.5F, -2.5F, 5F, 5F);
            EnsureProperAspect();
            this.Invalidate();
            timer1.Enabled = true;
        }
    }
}