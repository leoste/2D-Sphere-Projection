using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sphere_projection
{
    partial class Projections
    {        
        public Bitmap ProjectImageOnSphere(Bitmap bitmap)
        {
            //Before looking at the actual code, a short explanation of what we will have to do
            //to project a rectangular image onto a seemingly 3D sphere.
            //We have to keep several things in mind when drawing it.

            //First, since we're drawing on a 2D canvas, we are basically drawing a circle with stuff inside of it.
            //This means we need to warp each row of the image length-wise so that it would fit the width of the circle at that y value.

            //Second, because we're drawing on a 2D canvas, we need to additionally warp the image so that the final image looks like
            //a real sphere - at the edges of your vision of any sphere, the stuff on the surface of the sphere is sideways, so you don't
            //see those at all, while at the centre of your vision of that sphere, you can see things almost undistorted, and the areas
            //inbetween of that are kinda inbetween. Without keeping this in mind the image will not look like a sphere at all.
            
            //The method by which we project the source onto the canvas is by iterating through points on the canvas and calculating the
            //according points on the source where to take the pixels from.

            //We create the bitmap that the sphere will be drawn on.
            //Since a sphere is as wide as it is high, we need to make the resulting bitmap be as wide as it is high aswell.
            Bitmap canvas = new Bitmap(bitmap.Width, bitmap.Width);

            //We define and pre-calculate some properties of the sphere that we will be drawing -
            //the diameter as d, the radius as r, and the square of the radius as rSQR.
            double d = bitmap.Width;
            double r = d / 2;
            double rSQR = r * r;

            //We define some variables that we will be later using to map the pixels from the source onto the canvas.
            int dm1 = (int)d - 1;
            int wm1 = bitmap.Width - 1;
            int hm1 = bitmap.Height - 1;
            
            //We calculate half the perimeter of a circle with the radius of the sphere, which is a key part to
            //mapping points from the source onto the meridians and parallels of the sphere as seen on the canvas.
            double halfperimeter = GreatCircleDistance(-r, r, r);

            //We pre-calculate parts of the equasions that are used to map points later, as neither of these variables
            //change through-out the algorithm.
            double multipW = bitmap.Width / halfperimeter;
            double multipH = bitmap.Height / halfperimeter;

            //Some explanations before further discussing the algorithm (xi, yi) is a point on the canvas, where the
            //color is drawn onto, and (xa, ya) is a point on the source image, where that color for those xi and yi
            //values is taken from.            

            //As a sphere's four sides algorithms are mirrorable, we only need to calculate the coordinate sets for
            //a quarter of the canvas, and then we can mirror the points to map the pixels for the rest of the sphere.
            //That's why yi goes from 0 to radius and xi goes from middle to x2 at that y value.
            //This is explained in more detail when we actually get and draw the pixels.
            for (int yi = 0; yi < r; yi++)
            {
                //As I previously said, the overall shape drawn is a circle. This means we need a way of accurately
                //knowing the circle's width at each y value. For this we calculate it's x for the current y.
                //There are two x values for each y value since it's a square equation, but since we're only
                //calculating a quarter of the circle, we only need the second x.
                double x2 = Math.Sqrt(rSQR - Math.Pow(yi - r, 2)) + r;

                //Mapping the image onto the canvas as if it was projected onto a sphere has two parts: getting the
                //projection along x-axis correct and getting the projection along the y-axis correct. Here we do
                //the latter. To do this, we first calculate the great circle distance of the current y from the
                //start of our imaginary 180 degree arc, after which we multiply the result with our multiplier to
                //get the y-coordinate of the point on the source.
                double distcircumH = GreatCircleDistance(-r, yi - r, r);
                int ya = (int)(distcircumH * multipH);
                
                //We calculate the square of the y value of a point that the meridians calculated below will
                //pass through.
                //This is necessary for mapping the projection correctly along the x-axis, which I will explain
                //in more detail below.
                double meriYSQR = Math.Pow(r - yi, 2);
                
                for (int xi = (int)r; xi < (int)x2; xi++)
                {
                    //Projecting the x-axis properly is a bit more complicated than it was with the y-axis. This is
                    //because there are two sorts of warping being done and to understand them, we have to look at
                    //both a sphere and a map of the sphere (think of earth and a map).

                    //First, on a map, parallels are all equal length. This is of course not true on a sphere, the
                    //parallels closer to the poles are shorter and shorter, and on the poles their lengths are 0
                    //(that's why maps are more distorted closer to the poles).

                    //The second warp involves the meridians - On a map, all meridians are parallel, but on a sphere,
                    //they all connect the two poles. This means that although the meridian you are directly looking
                    //at still appears straight, the meridians to the right or left appear curved, and the meridians
                    //at 180 and -180 degrees follow the outlines of the 2D circle shape that the sphere looks like.

                    //The x value of a point that passes through the current meridian.
                    double meriX = Math.Abs(r - xi);

                    //We know a coordinate (meriX, meriY) that is a point on the current meridian. We also know this
                    //meridian goes through both of the poles, i.e coordinates (r, 0) and (r, d). Using this knowledge
                    //We can calculate b of the meridian (half-width) - keep in mind the meridian when viewed in 2D is
                    //an ellipse.
                    //meriB is essentially the x coordinate of the point on the source image where to take the pixel from,
                    //we now only need to address the same problem that we had with calculating y coordinate, making
                    //the image look like a sphere.
                    double meriB = (r * meriX) / Math.Sqrt(rSQR - meriYSQR);
                    
                    //And that is done here - we find the great circle distance from the start of the 180 degree arc to
                    //a point with the x that is meriB, and we have our x coordinate for picking a pixel from the source!
                    double distcircumW = GreatCircleDistance(-r, meriB, r);
                    int xa = (int)(distcircumW * multipW);

                    //Finally we just have to use the calculated points to draw the sphere. As discussed before, the
                    //four quarters have the same mathematics behind it, so we now use the data we calculated for one
                    //quarter and do simple mathematics to draw the sphere. This is how the coordinates are transformed
                    //for each quarter:
                    //Quarter 1. x <= x. y <= y. Quarter 2. x <= x. y <= height - y.
                    //Quarter 3. x <= width - x. y <= height - y. Quarter 4. x <= width - x. y <= y.

                    Color c;
                    int tem1 = wm1 - xa;
                    int tem2 = dm1 - xi;
                    int tem3 = hm1 - ya;
                    int tem4 = dm1 - yi;

                    c = bitmap.GetPixel(xa, ya);
                    canvas.SetPixel(xi, yi, c);
                    c = bitmap.GetPixel(tem1, ya);
                    canvas.SetPixel(tem2, yi, c);
                    c = bitmap.GetPixel(tem1, tem3);
                    canvas.SetPixel(tem2, tem4, c);
                    c = bitmap.GetPixel(xa, tem3);
                    canvas.SetPixel(xi, tem4, c);

                }
            }
            return canvas;
        }

        //This function is used to calculate the great circle distance between two given points.
        //That's the distance between two points along the circumference of the circle.
        //x1 and x2 are Cartesian coordinates.
        private double GreatCircleDistance(double x1, double x2, double r)
        {
            //First, we get the y coordinates for both x coordinates to get two points.
            double y1 = CircleY(x1, r);
            double y2 = CircleY(x2, r);

            //The center of the circle O, point 1 A and point 2 B form a triangle. If we calculate all side lengths,
            //we can calculate the angle AOB and use it to get the arc length.
            double a = Math.Sqrt(x2 * x2 + y2 * y2);
            double b = Math.Sqrt(x1 * x1 + y1 * y1);
            double c = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

            //Calculating the angle here,
            double angle = Math.Acos((a * a + b * b - c * c) / (2 * a * b));

            //and getting the arc length!
            double distance = r * angle;

            return distance;
        }

        //This function is used to calculate the Y point of a circle at the given X.
        private double CircleY(double x, double r)
        {
            //Very easy maths, but a function is just more convenient to use than copypasting this line.
            return Math.Sqrt(r * r - x * x);
        }
    }
}