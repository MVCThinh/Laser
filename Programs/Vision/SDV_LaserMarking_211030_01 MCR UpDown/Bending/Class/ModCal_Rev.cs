using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using rs2DAlign;


namespace Bending
{
    public class Point2d
    {
        public double X;
        public double Y;

        public Point2d()
        { }

        public Point2d(double dX, double dY)
        {
            X = dX;
            Y = dY;
        }
    }

    public class XXYY
    {
        //20170503 : SetValue 추가
        public double X1;
        public double X2;
        public double Y1;
        public double Y2;

        public void SetValue(double y1, double y2)
        {
            Y1 = y1;
            Y2 = y2;
        }
    }

    public class XYT
    {
        //20170413 : SetValue 추가
        public double X;
        public double Y;
        public double T;

        public void SetValue(double x, double y, double t)
        {
            X = x;
            Y = y;
            T = t;
        }
    }
}
