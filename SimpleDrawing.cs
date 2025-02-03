using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleDrawing
{
    public enum DrawingTool
    {
        Pen,
        Rectangle,
        Ellipse,
        Line
    }

    public class DrawingForm : Form
    {
        private Point lastPoint = Point.Empty;
        private bool isMouseDown = false;
        private Pen currentPen;
        private Color currentColor = Color.Black;
        private float currentWidth = 2f;
        private DrawingTool currentTool = DrawingTool.Pen;
        private Point startPoint;
        private Bitmap drawingBitmap;

        public DrawingForm()
        {
            this.Text = "Простое приложение для рисования";
            this.Size = new Size(800, 600);
            this.BackColor = Color.White;

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true);

            drawingBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            using (Graphics g = Graphics.FromImage(drawingBitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }

            currentPen = new Pen(currentColor, currentWidth);
            currentPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            currentPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            InitializeControls();

            this.MouseDown += new MouseEventHandler(DrawingForm_MouseDown);
            this.MouseMove += new MouseEventHandler(DrawingForm_MouseMove);
            this.MouseUp += new MouseEventHandler(DrawingForm_MouseUp);
            this.Paint += new PaintEventHandler(DrawingForm_Paint);
            this.Resize += new EventHandler(DrawingForm_Resize);
        }

        private void InitializeControls()
        {
            ToolStrip toolStrip = new ToolStrip();
            
            var penButton = new ToolStripButton("Карандаш");
            penButton.Click += (s, e) => currentTool = DrawingTool.Pen;
            
            var rectangleButton = new ToolStripButton("Прямоугольник");
            rectangleButton.Click += (s, e) => currentTool = DrawingTool.Rectangle;
            
            var ellipseButton = new ToolStripButton("Круг");
            ellipseButton.Click += (s, e) => currentTool = DrawingTool.Ellipse;
            
            var lineButton = new ToolStripButton("Линия");
            lineButton.Click += (s, e) => currentTool = DrawingTool.Line;

            var colorButton = new ToolStripButton("Цвет");
            colorButton.Click += (s, e) =>
            {
                ColorDialog colorDialog = new ColorDialog();
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    currentColor = colorDialog.Color;
                    currentPen.Color = currentColor;
                }
            };

            var widthCombo = new ToolStripComboBox();
            widthCombo.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "8", "10", "12", "15" });
            widthCombo.SelectedIndex = 1;
            widthCombo.SelectedIndexChanged += (s, e) =>
            {
                if (float.TryParse(widthCombo.SelectedItem.ToString(), out float width))
                {
                    currentWidth = width;
                    currentPen.Width = currentWidth;
                }
            };

            var clearButton = new ToolStripButton("Очистить");
            clearButton.Click += (s, e) =>
            {
                using (Graphics g = Graphics.FromImage(drawingBitmap))
                {
                    g.Clear(Color.White);
                }
                this.Invalidate();
            };

            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                penButton,
                rectangleButton,
                ellipseButton,
                lineButton,
                new ToolStripSeparator(),
                colorButton,
                widthCombo,
                new ToolStripSeparator(),
                clearButton
            });

            this.Controls.Add(toolStrip);
        }

        private void DrawingForm_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = e.Location;
            startPoint = e.Location;
            isMouseDown = true;
        }

        private void DrawingForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                if (currentTool == DrawingTool.Pen)
                {
                    using (Graphics g = Graphics.FromImage(drawingBitmap))
                    {
                        g.DrawLine(currentPen, lastPoint, e.Location);
                    }
                    lastPoint = e.Location;
                    this.Invalidate();
                }
                else
                {
                    lastPoint = e.Location;
                    this.Invalidate();
                }
            }
        }

        private void DrawingForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                using (Graphics g = Graphics.FromImage(drawingBitmap))
                {
                    switch (currentTool)
                    {
                        case DrawingTool.Rectangle:
                            DrawRectangle(g, startPoint, e.Location);
                            break;
                        case DrawingTool.Ellipse:
                            DrawEllipse(g, startPoint, e.Location);
                            break;
                        case DrawingTool.Line:
                            g.DrawLine(currentPen, startPoint, e.Location);
                            break;
                    }
                }
                this.Invalidate();
            }
            isMouseDown = false;
        }

        private void DrawRectangle(Graphics g, Point start, Point end)
        {
            Rectangle rect = GetRectangle(start, end);
            g.DrawRectangle(currentPen, rect);
        }

        private void DrawEllipse(Graphics g, Point start, Point end)
        {
            Rectangle rect = GetRectangle(start, end);
            g.DrawEllipse(currentPen, rect);
        }

        private Rectangle GetRectangle(Point start, Point end)
        {
            return new Rectangle(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(end.X - start.X),
                Math.Abs(end.Y - start.Y));
        }

        private void DrawingForm_Paint(object sender, PaintEventArgs e)
        {
           
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(drawingBitmap, 0, 0);

            if (isMouseDown && currentTool != DrawingTool.Pen)
            {
                switch (currentTool)
                {
                    case DrawingTool.Rectangle:
                        DrawRectangle(e.Graphics, startPoint, lastPoint);
                        break;
                    case DrawingTool.Ellipse:
                        DrawEllipse(e.Graphics, startPoint, lastPoint);
                        break;
                    case DrawingTool.Line:
                        e.Graphics.DrawLine(currentPen, startPoint, lastPoint);
                        break;
                }
            }
        }

        private void DrawingForm_Resize(object sender, EventArgs e)
        {
            var newBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(drawingBitmap, 0, 0);
            }
            
            var oldBitmap = drawingBitmap;
            drawingBitmap = newBitmap;
            oldBitmap.Dispose();
            
            this.Invalidate();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DrawingForm());
        }
    }
}