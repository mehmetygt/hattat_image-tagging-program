using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lineAnnotationLA
{
    public partial class Form1 : Form
    {
        Graphics g;
        Pen pencil;
        Point coordFirst;
        Point coordCurrent;
        bool mouseDown = false;
        double rateX, rateY;
        int imageSizeX, imageSizeY;
        List<Point[]> points = new List<Point[]>();

        public Form1()
        {
            InitializeComponent();
            this.Text = "::.:: Hattat :::.::";
            this.Location = new Point(0,0);
            pictureBox1.MaximumSize = new Size(
                Convert.ToInt32(Screen.PrimaryScreen.Bounds.Size.Width - 150),
                Convert.ToInt32(Screen.PrimaryScreen.Bounds.Size.Height - 150));
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { 
            // open file
            pictureBox1.Refresh();
            points.Clear();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image files (*.jpg)|*.jpg";
            openFileDialog1.Multiselect = false;
            openFileDialog1.ShowDialog();
            openImage(openFileDialog1.FileName);
            checkedListBox1.Items.Clear();
            checkedListBox1.Items.Add(openFileDialog1.FileName);
            if (File.Exists(openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.Length-4) + ".txt"))
            {
                
                checkedListBox1.SetItemChecked(0,true);
                checkedListBox1.SelectedIndex = 0;
            }
            }catch (ArgumentException ee)
            {
                checkedListBox1.Items.Clear();
                points.Clear();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            coordFirst = new Point(e.X, e.Y);
            mouseDown = true;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                (sender as PictureBox).Refresh();

                for (int i = 0; i < points.Count; i++)
                {
                    drawLine(points[i][0], points[i][1]);
                }
                coordCurrent = new Point(e.X, e.Y);
                drawLine( isGap(coordFirst), isGap(coordCurrent));
            }
        }
        
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            Point[] p = new Point[2];
            p[0] = isGap(coordFirst);
            p[1] = isGap(coordCurrent);

            points.Add(p);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            points.Clear();
            pictureBox1.Refresh();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            saveFile(pictureBox1.ImageLocation.ToString());
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog folderDialog1 = new FolderBrowserDialog();
                folderDialog1.ShowDialog();
                String Folder = folderDialog1.SelectedPath;
                foreach (String f in Directory.GetFiles(Folder))
                {
                    if (f.EndsWith("jpg"))
                    {
                        checkedListBox1.Items.Add(f);
                    }
                }
                
                foreach (String f in Directory.GetFiles(Folder))
                {
                    if (f.EndsWith("txt"))
                    {
                        //label4.Text = f.Substring(0, f.Length - 3);
                        // label1.Text = checkedListBox1.FindString(f.Substring(0, f.Length - 3)).ToString();
                        int index = checkedListBox1.FindString(f.Substring(0, f.Length - 3));
                        if (index > -1)
                        {
                            checkedListBox1.SetItemChecked(index, true);
                        }
                    }
                }
                checkedListBox1.SelectedIndex = 0;
            }
            catch (ArgumentException ee)
            {
                /// not selected folder
                checkedListBox1.Items.Clear();
                points.Clear();
            }
        }

        private void checkedListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            //checkedListBox1.GetItemChecked(checkedListBox1.SelectedIndex);
            pictureBox1.WaitOnLoad = true;
            openImage(checkedListBox1.SelectedItem.ToString());
            points.Clear();

            int index = checkedListBox1.SelectedIndex;
            if (checkedListBox1.GetItemChecked(index))
            {

                string fileName = checkedListBox1.Items[index].ToString();
                getPoints(sender, fileName.Substring(0, fileName.Length - 3) + "txt");

                for (int i = 0; i < points.Count; i++)
                {

                    Point[] p = points[i];
                    drawLine( isGap(p[0]), isGap(p[1]));
                }
            }

        }

        private void getPoints(object sender, String label)
        {
            try
            {
                pictureBox1.Refresh();
                List<Point> p = new List<Point>();
                string[] lines = File.ReadAllLines(label);
                foreach (string l in lines)
                {

                    double rrateX = (double)imageSizeX / 400, rrateY = (double)imageSizeY / 400;
                    string[] xy = l.Split('.');
                    Point[] p1 = new Point[2];
                    p1[0] = isGap(p1[0]);
                    p1[1] = isGap(p1[1]);
                    p1[0].X = Convert.ToInt32((double)(Convert.ToDouble(xy[0]) * rrateX));
                    p1[0].Y = Convert.ToInt32((double)(Convert.ToDouble(xy[1]) * rrateY));
                    p1[1].X = Convert.ToInt32((double)(Convert.ToDouble(xy[2]) * rrateX));
                    p1[1].Y = Convert.ToInt32((double)(Convert.ToDouble(xy[3]) * rrateY));

                    points.Add(p1);
                }
            }
            catch (FileNotFoundException except)
            {
                checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex,false);
            }
        }
        private void drawLine(Point first, Point last)
        {
            pencil = new Pen(System.Drawing.Color.Red, 5);

            g = pictureBox1.CreateGraphics();
            g.DrawLine(pencil, first, last);

            g.Dispose();
        }

        public void openImage(String ImgURL)
        {
            try
            {
                //toolStripStatusLabel1.Text = ImgURL.Substring(ImgURL.LastIndexOf("\\"));
                pictureBox1.ImageLocation = ImgURL;
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                System.Drawing.Image img = System.Drawing.Image.FromFile(ImgURL);
                imageSizeX = img.Size.Width;
                imageSizeY = img.Size.Height;
                rateX = (double)(rateValueX.Value / img.Size.Width);
                rateY = (double)(rateValueY.Value / img.Size.Height);
                toolStripStatusLabel1.Text = pictureBox1.ImageLocation.ToString();
                toolStripStatusLabel3.Text = "Height : " + pictureBox1.Size.Height + " Width : " + pictureBox1.Size.Width;
            }
            catch (ArgumentException except)
            {
                // not selected file
            }
            
        }

        public void saveFile(String file)
        {
            file = file.Substring(0, file.LastIndexOf("."));
            file += ".txt";

            if (points.Count < 1)
            {
                checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex, false);
                if (File.Exists(file))
                { 
                    File.Delete(file);
                }
                return;
            }
            checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex,true);
            
            using (var writer = File.CreateText(file))
            {
                for (int i = 0; i < points.Count(); i += 1)
                {

                    writer.WriteLine(Convert.ToDouble(points[i][0].X * rateX) + "." 
                        + Convert.ToDouble(points[i][0].Y * rateY) + "." 
                        + Convert.ToDouble(points[i][1].X * rateX) + "." 
                        + Convert.ToDouble(points[i][1].Y * rateY));
                }
                writer.Close();
            }
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
            if(e.KeyChar == (char)Keys.Enter)
                saveFile(pictureBox1.ImageLocation.ToString());
            else if (e.KeyChar == (char)Keys.A || e.KeyChar == 'a' || e.KeyChar == (char)Keys.Left)
            {
                saveFile(pictureBox1.ImageLocation.ToString());
                int i = checkedListBox1.SelectedIndex;
                if (i > 0)
                    checkedListBox1.SelectedIndex = i - 1;
            }
            else if (e.KeyChar == (char)Keys.D || e.KeyChar == 'd' || e.KeyChar == (char)Keys.Right)
            {
                saveFile(pictureBox1.ImageLocation.ToString());
                int i = checkedListBox1.SelectedIndex;
                if (i < checkedListBox1.Items.Count)
                    checkedListBox1.SelectedIndex = i + 1;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            saveFile(pictureBox1.ImageLocation.ToString());
            int i = checkedListBox1.SelectedIndex;
            if (i < checkedListBox1.Items.Count)
                checkedListBox1.SelectedIndex = i + 1;
        }

        private void btnPre_Click(object sender, EventArgs e)
        {
            saveFile(pictureBox1.ImageLocation.ToString());
            int i = checkedListBox1.SelectedIndex;
            if (i > 0)
                checkedListBox1.SelectedIndex = i - 1;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            
            if (pictureBox1.Size.Width < screen.Width || pictureBox1.Size.Width < screen.Height)
                Form1.ActiveForm.Size = new Size(Convert.ToInt32(pictureBox1.Size.Width * 1.30), Convert.ToInt32(pictureBox1.Size.Height * 1.30));
            else
                WindowState = FormWindowState.Maximized;
            Form1.ActiveForm.Refresh();
        }
       
        private Point isGap(Point size)
        {
            if (size.X > imageSizeX)
                size.X = imageSizeX;
            if (size.X > imageSizeX)
                size.X = imageSizeX;
            return size;
        }
    }
}