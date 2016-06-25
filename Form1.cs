using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Gr2_2
{
    public partial class Form1 : Form
    {

        private Image image,image1;
        private Point start, finish;
        private Rectangle rectangle;
        private bool f = false;


        public Form1()
        {
            InitializeComponent();
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = openFileDialog1.FileName;
                try
                {
                    Image im = new Bitmap(s);
                    if (pictureBox1.Image != null)
                        pictureBox1.Image.Dispose();
                    pictureBox1.Image = im;
                    image = im;
                    image1 = im;
                }
                catch
                {
                    MessageBox.Show("File " + s + "has a wrong format.", "Error");
                    return;
                }
                openFileDialog1.FileName = "";
            }
            f = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string s0 = saveFileDialog1.FileName;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = saveFileDialog1.FileName;
                if (s.ToUpper() == s0.ToUpper())
                {
                    s0 = Path.GetDirectoryName(s0) + "\\($$##$$).png";
                    pictureBox1.Image.Save(s0);
                    pictureBox1.Image.Dispose();
                    File.Delete(s);
                    File.Move(s0, s);
                    pictureBox1.Image = new Bitmap(s);
                }
                else
                    pictureBox1.Image.Save(s);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            rectangle = new Rectangle(0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height);
            Random rnd = new Random();
            Point[] points = new Point[Math.Max(rectangle.Width,rectangle.Height)];
            for (int i = 0; i < points.GetLength(0); ++i)
                points[i] = new Point(rnd.Next(rectangle.Width), rnd.Next(rectangle.Height));
            image = Voronoi(new Bitmap(pictureBox1.Image), rectangle, points);
            pictureBox1.Image = image;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = image1;
            image = image1;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (f)
                f = false;
            else
                start = e.Location;
        }

        private class PointComparer : IComparer<Point>
        {
            public int Compare(Point p1, Point p2)
            {
                if (p1.Y < p2.Y)
                    return -1;
                else
                    if (p1.Y > p2.Y)
                        return 1;
                    else
                        if (p1.X < p2.X)
                            return -1;
                        else
                            if (p1.X > p2.Y)
                                return 1;
                            else
                                return 0;
            }
        }

        private double distance(int x1, int y1, int x2, int y2)
        {
            return (Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)));
        }

        private Bitmap Voronoi(Bitmap bm, Rectangle rect, Point[] points)
        {
            int[,] area_property = new int[points.GetLength(0), 4];
            Color[] area_colors = new Color[points.GetLength(0)];
            int[,] im_points = new int[rect.Width, rect.Height];

            for (int i = 0; i < rect.Width; ++i)
                for (int j = 0; j < rect.Height; ++j)
                {
                    double min_d = distance(i, j, points[0].X, points[0].Y);
                    im_points[i, j] = 0;
                    for (int k = 1; k < points.GetLength(0); ++k)
                    {
                        double d = distance(i, j, points[k].X, points[k].Y);
                        if (min_d > d)
                        {
                            min_d = d;
                            im_points[i, j] = k;      // принадлежит области точки k
                        }
                    }
                    area_property[im_points[i, j], 0] += bm.GetPixel(i + rect.Left, j + rect.Top).R;  //сумма всех красных компонент точек области
                    area_property[im_points[i, j], 1] += bm.GetPixel(i + rect.Left, j + rect.Top).G;   //сумма всех зеленных компонент точек области
                    area_property[im_points[i, j], 2] += bm.GetPixel(i + rect.Left, j + rect.Top).B;  //сумма всех синих компонент точек области
                    ++area_property[im_points[i, j], 3];  //количество точек в области точки
                }
            
            for (int k = 0; k < points.GetLength(0); ++k)
            {
                if ((double)area_property[k, 3] == 0)
                    continue;
                int r = (int)Math.Round((double)area_property[k, 0] / (double)area_property[k, 3]);  //среднее всех красных компонент точек области
                int g = (int)Math.Round((double)area_property[k, 1] / (double)area_property[k, 3]);  //среднее всех зеленных компонент точек области
                int b = (int)Math.Round((double)area_property[k, 2] / (double)area_property[k, 3]);  //среднее всех синих компонент точек области
                area_colors[k] = Color.FromArgb(r, g, b); //цвет области
            
            }

            //красим области
            for (int i = 0; i < rect.Width; ++i)
                for (int j = 0; j < rect.Height; ++j)
                    bm.SetPixel(rect.Left + i, rect.Top + j, area_colors[im_points[i, j]]);

            //разделяем области
            for (int i = rect.Left; i < rect.Right-1; ++i)
                for (int j = rect.Top; j < rect.Bottom - 1; ++j)
                    if (bm.GetPixel(i, j) != bm.GetPixel(i, j + 1) || bm.GetPixel(i, j) != bm.GetPixel(i + 1, j))
                        bm.SetPixel(i, j, Color.White);

            return bm;
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (f)
                f = false;
            else
                if (e.Button == MouseButtons.Left)
                {
                    finish = e.Location;
                    Bitmap bm = new Bitmap(image);
                    Graphics g = Graphics.FromImage(bm);
                    rectangle = new Rectangle(Math.Min(start.X, finish.X), Math.Min(start.Y, finish.Y), Math.Abs(start.X - finish.X), Math.Abs(start.Y - finish.Y));
                    g.DrawRectangle(Pens.Black, rectangle);
                    pictureBox1.Image = bm;
                }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

            pictureBox1.Image = image;
            Random rnd = new Random();
            Point[] points = new Point[Math.Max(rectangle.Width,rectangle.Height)];
            for (int i = 0; i < points.GetLength(0); ++i)
                points[i] = new Point(rnd.Next(rectangle.Width), rnd.Next(rectangle.Height));
            image = Voronoi(new Bitmap(pictureBox1.Image), rectangle, points);
            pictureBox1.Image = image;          
        }

    }
}
