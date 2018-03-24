using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sphere_projection
{
    public partial class Form1 : Form
    {
        //The code in this file is not the main focus of the project and exists only as an interface
        //for the user to test this feature. The interesting stuff is in ProjectImageOnSphere.cs.

        public Form1()
        {
            InitializeComponent();
        }

        //A simple function to open an image. It returns true if the user selected an image to open.
        private bool OpenImage(out Image image)
        {
            image = null;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                image = Image.FromFile(openFileDialog1.FileName);
                return true;
            }
            else return false;
        }
        
        // A simple function to save an image. It returns true if the user decided to save the image.
        private bool SaveImage(Image image)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageFormat imageformat;

                string filename = saveFileDialog1.FileName;
                switch (filename.Substring(filename.Length - 3))
                {
                    default: imageformat = ImageFormat.Jpeg; break;
                    case "png": imageformat = ImageFormat.Png; break;
                    case "bmp": imageformat = ImageFormat.Bmp; break;
                }

                image.Save(saveFileDialog1.FileName, imageformat);
                return true;
            }
            else return false;
        }
                
        private void button1_Click(object sender, EventArgs e)
        {
            Projections ep = new Projections();
            pictureBox2.Image = ep.ProjectImageOnSphere((Bitmap)pictureBox1.Image);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveImage(pictureBox2.Image);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (OpenImage(out Image image)) pictureBox1.Image = image;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.decorative_stained_glass;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.smallimg;
        }
    }
}
