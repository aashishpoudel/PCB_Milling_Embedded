using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PCBLayoutGenerator
{   
    public struct join
    {
            // = new int[10];
            public Point[] mydblpts1;
            public Point[] mydblpts2;
            public Point[] mypts;
            public Point[] mycmppts1;
            public Point[] mycmppts2;  
            public Point[] allpts;    
            public Point startcircle;
            public Point endcircle;       
            public int ptCount;
            public int allcount;  
    }
    public partial class fclsPCBGenerator : Form
    {
        #region variables       
        
        private Graphics myGraphics;
        private Bitmap myBitmap, myTmpBitmap;
        private Pen myPen, myPen2;
        private Brush myBrush;

        int No_of_Blocks, allcount;
        int ResolutionFactor;
        string XStartString, YStartString;
        string XCenterString, YCenterString;
        int XCenter, YCenter, radius, X, Y, ptcount, circ_count;
        string XEndString, YEndString;
        int Xlength, Ylength;
        float XBegin, YBegin, XEnd, YEnd;
        String[] LinesRead = new String[200];
        Point[] circle_center = new Point[15];//for 15 components
        bool PCBDrawn = false;
        bool flagAnup;
        int join_count;
        int XDiff, YDiff, delX, delY;
        double pi = 3.1416;
        double angle;
        int XDisp, YDisp;
        public bool RTSState = false;
        public bool CTSState = false;


        private double ResX;
        private double ResY;
        private int DelayX;
        private int DelayY;
        private int XD, YD; 
        private double PrimDelay;
        private float Freq;
        private int[] InitPt = new int[2];
        private int[] FinalPt = new int[2];
        private int XStep;
        private int YStep;
        private bool Xck;
        private bool Yck;
        private int XDelay;
        private int YDelay;
        private byte[] DataByte = new byte[9];
        //private RS232 MySp = new RS232();

        
        #endregion
                
        public join[] join_trace = new join[10];

        public fclsPCBGenerator()
        {
            InitializeComponent();
            ResX = (double)0.21; //* 39.37/10 ;//5 is corrrection factor for running //dont debugged actual error
            ResY = (double)0.16; //* 39.37/10 ;
           // ResX = 0.21F;
           // ResY = 0.16F; 
            //ResX = 8.2677F;
            //ResY = 6.2992F;

            PrimDelay = 25;
            Freq = 11.0592F;
            serialPort1.Open();
            try
            {
                myBitmap = new Bitmap(595, 595);
                myTmpBitmap = new Bitmap(595, 595);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            myPen = new Pen(Color.Blue);

            ResolutionFactor = 96;// (int)(1024 / 10.5);
            //radius = (int)(10 * ResolutionFactor / 1000000);
        }

        private void MyDrawing()
        {
            XStartString = "0";
            YStartString = "0";
            myTmpBitmap = new Bitmap(myBitmap);
            
            #region Drawing
            join_count = -1; ptcount = 0; circ_count = 0;
            for (int i = 0; i < No_of_Blocks; i++)
            {
                if (LinesRead[i].Contains("G01"))
                {
                    //MessageBox.Show(LinesRead[i]);
                    if (LinesRead[i].Contains("D01"))
                    {
                        Xlength = LinesRead[i].IndexOf('Y') - LinesRead[i].IndexOf('X');
                        Ylength = LinesRead[i].IndexOf('D') - LinesRead[i].IndexOf('Y');
                        XEndString = LinesRead[i].Substring(LinesRead[i].IndexOf('X') + 1, Xlength - 1);
                        YEndString = LinesRead[i].Substring(LinesRead[i].IndexOf('Y') + 1, Ylength - 1);
                        XEnd = Int32.Parse(XEndString) * ResolutionFactor / 1000000;
                        YEnd = Int32.Parse(YEndString) * ResolutionFactor / 1000000;

                        //Plotting Function
                        myGraphics = Graphics.FromImage(myBitmap);
                        //myGraphics.DrawLine(myPen, Int32.Parse(XStartString)/1000000, Int32.Parse(YStartString)/1000000, Int32.Parse(XStartString)/1000000, Int32.Parse(YEndString)/1000000);
                        myGraphics.DrawLine(myPen, XBegin, YBegin, XEnd, YEnd);
                        picGerberPCB.Image = myBitmap;

                        X = (int)(XEnd + 0.5);
                        Y = (int)(YEnd + 0.5);
                        join_trace[join_count].mypts[ptcount++] = new Point(X, Y);
                        //MessageBox.Show(join_trace[join_count].mypts[ptcount].ToString());

                        XBegin = XEnd;
                        YBegin = YEnd;
                        join_trace[join_count].ptCount = ptcount;
                    }
                    else if (LinesRead[i].EndsWith("D02") && (LinesRead[i].Substring(0,LinesRead[i].IndexOf('D'))!=LinesRead[i-1].Substring(0,LinesRead[i-1].IndexOf('D'))))
                    {
                        ptcount = 0; join_count++;
                        join_trace[join_count].mypts = new Point[10];
                        join_trace[join_count].mydblpts1 = new Point[10];
                        join_trace[join_count].mydblpts2 = new Point[10];
                        join_trace[join_count].mycmppts1 = new Point[8];
                        join_trace[join_count].mycmppts2 = new Point[8];
                        join_trace[join_count].allpts = new Point[36];

                        Xlength = LinesRead[i].IndexOf('Y') - LinesRead[i].IndexOf('X');
                        Ylength = LinesRead[i].IndexOf('D') - LinesRead[i].IndexOf('Y');
                        XStartString = LinesRead[i].Substring(LinesRead[i].IndexOf('X') + 1, Xlength - 1);
                        YStartString = LinesRead[i].Substring(LinesRead[i].IndexOf('Y') + 1, Ylength - 1);
                        XBegin = Int32.Parse(XStartString) * ResolutionFactor / 1000000;
                        YBegin = Int32.Parse(YStartString) * ResolutionFactor / 1000000;
                        //MessageBox.Show("xStartvalue=" + XStartString);
                        //MessageBox.Show("yStartvalue=" + YStartString);
                        X = (int)(XBegin+0.5);
                        Y = (int)(YBegin+0.5);

                        join_trace[join_count].mypts[ptcount++] = new Point(X, Y);

                    }
                    else if(LinesRead[i].EndsWith("D03"))
                    {
                        
                        Xlength = LinesRead[i].IndexOf('Y') - LinesRead[i].IndexOf('X');
                        Ylength = LinesRead[i].IndexOf('D') - LinesRead[i].IndexOf('Y');
                        XCenterString = LinesRead[i].Substring(LinesRead[i].IndexOf('X') + 1, Xlength - 1);
                        YCenterString = LinesRead[i].Substring(LinesRead[i].IndexOf('Y') + 1, Ylength - 1);
                        XCenter = Int32.Parse(XCenterString) * ResolutionFactor / 1000000;
                        YCenter = Int32.Parse(YCenterString) * ResolutionFactor / 1000000;
                        radius = 12;
                        myGraphics = Graphics.FromImage(myBitmap);
                        myPen.Brush = Brushes.Blue;
                        myGraphics.FillEllipse(myPen.Brush, XCenter - radius, YCenter - radius, 2 * radius, 2 * radius);

                        X = (int)(XCenter+0.5);
                        Y = (int)(YCenter + 0.5);

                        circle_center[circ_count] = new Point(X, Y);
                        circ_count++;
                    }
                }

            }
            
            join_count++;
            #endregion

        }

        private void MyOutlineDrawing()
        {
            XStartString = "0";
            YStartString = "0";
            myTmpBitmap = new Bitmap(myBitmap);

            #region Drawing
            for (int i = 0; i < No_of_Blocks; i++)
            {
                if (LinesRead[i].Contains("G01"))
                {
                    //MessageBox.Show(LinesRead[i]);
                    if (LinesRead[i].Contains("D01"))
                    {
                        Xlength = LinesRead[i].IndexOf('Y') - LinesRead[i].IndexOf('X');
                        Ylength = LinesRead[i].IndexOf('D') - LinesRead[i].IndexOf('Y');
                        XEndString = LinesRead[i].Substring(LinesRead[i].IndexOf('X') + 1, Xlength - 1);
                        YEndString = LinesRead[i].Substring(LinesRead[i].IndexOf('Y') + 1, Ylength - 1);
                        //MessageBox.Show("xStartvalue=" + XStartString);
                        //MessageBox.Show("yStartvalue=" + YStartString);
                        //MessageBox.Show("xEndvalue=" + XStartString);
                        //MessageBox.Show("yEndvalue=" + YEndString);
                        XEnd = Int32.Parse(XEndString) * ResolutionFactor / 1000000;
                        YEnd = Int32.Parse(YEndString) * ResolutionFactor / 1000000;

                        //Plotting Function
                        myGraphics = Graphics.FromImage(myBitmap);
                        //myGraphics.DrawLine(myPen, Int32.Parse(XStartString)/1000000, Int32.Parse(YStartString)/1000000, Int32.Parse(XStartString)/1000000, Int32.Parse(YEndString)/1000000);
                        myGraphics.DrawLine(myPen, XBegin, YBegin, XEnd, YEnd);
                        picGerberPCB.Image = myBitmap;

                        XBegin = XEnd;
                        YBegin = YEnd;
                    }
                    else if (LinesRead[i].EndsWith("D02"))
                    {
                        Xlength = LinesRead[i].IndexOf('Y') - LinesRead[i].IndexOf('X');
                        Ylength = LinesRead[i].IndexOf('D') - LinesRead[i].IndexOf('Y');
                        XStartString = LinesRead[i].Substring(LinesRead[i].IndexOf('X') + 1, Xlength - 1);
                        YStartString = LinesRead[i].Substring(LinesRead[i].IndexOf('Y') + 1, Ylength - 1);
                        XBegin = Int32.Parse(XStartString) * ResolutionFactor / 1000000;
                        YBegin = Int32.Parse(YStartString) * ResolutionFactor / 1000000;
                        //MessageBox.Show("xStartvalue=" + XStartString);
                        //MessageBox.Show("yStartvalue=" + YStartString);

                    }
                }

            }
            #endregion
        }

        private void btnSelectGerberFile_Click(object sender, EventArgs e)
        {
            if (ofdGerberFile.ShowDialog() == DialogResult.OK)
            {
                myBitmap = new Bitmap(myTmpBitmap);
                picGerberPCB.Image = myBitmap;

                String AllContents = File.ReadAllText(ofdGerberFile.FileName);
                LinesRead = AllContents.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                No_of_Blocks = LinesRead.Length;

                MyDrawing();
                lblFilePath.Text = "File: " + Path.GetFullPath(ofdGerberFile.FileName);
                PCBDrawn = true;
            }
        }
              
        private void rdobtn800_CheckedChanged(object sender, EventArgs e)
        {
            if (rdobtn800.Checked)
            {
                ResolutionFactor = (int)(800 / 10.5);
                //MessageBox.Show(ResolutionFactor.ToString());
                myBitmap = new Bitmap(myTmpBitmap);
                picGerberPCB.Image = myBitmap;
                MyDrawing();
            }
        }

        private void rdobtn1152_CheckedChanged(object sender, EventArgs e)
        {
            if (rdobtn1152.Checked)
            {
                ResolutionFactor = (int)(1152 / 10.5);
                myBitmap = new Bitmap(myTmpBitmap);
                picGerberPCB.Image = myBitmap;
                MyDrawing();
            }
        }

        private void rdobtn1024_CheckedChanged(object sender, EventArgs e)
        {
            if (rdobtn1024.Checked)
            {
                ResolutionFactor = (int)(1024 / 10.5);
                myBitmap = new Bitmap(myTmpBitmap);
                picGerberPCB.Image = myBitmap;
                MyDrawing();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            myBitmap = new Bitmap(myTmpBitmap);
            picGerberPCB.Image = myBitmap;
            lblFilePath.Text = "";
            picDoubleLine.Image = myBitmap;
            PCBDrawn = false;
        }        

        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void fclsPCBGenerator_Load(object sender, EventArgs e)
        {

        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
       
        }

        Point bisector_intersection(Point pt1, Point pt2, Point pt3, Point pt4)
        {
            double m1, m2, m3, f1, f2, mb1, mb2;
            double tangent;
            Point myPoint = new Point();

            if (pt1.Y == pt2.Y && pt3.Y == pt2.Y)
            {
                myPoint.X = pt2.X;
                myPoint.Y = pt4.Y;
            }
            else if (pt1.X == pt2.X && pt1.X == pt3.X)
            {
                myPoint.X = pt4.X;
                myPoint.Y = pt2.Y;
            }

            else if (pt1.X == pt2.X)   //m1 infinite
            {
                //MessageBox.Show("x1=x2");
                m2 = (pt3.Y - pt2.Y) / (double)(pt3.X - pt2.X);
                f2 = 1 / Math.Sqrt(1 + m2 * m2);
                mb1 = -(1 - f2 * m2) / f2;
                mb2 = (1 + f2 * m2) / f2;

                tangent = Math.Abs((m2 - mb1) / (double)(1 + m2 * mb1));
                                
                if (tangent > 1)  m3 = mb1;//Take B1
                else              m3 = mb2;
                
                myPoint.X = (int)(pt4.X + 0.5);
                myPoint.Y = (int)(pt2.Y + m3 * (pt4.X - pt2.X) + 0.5);
                //MessageBox.Show("m2=" + m2.ToString() + " f2=" + f2.ToString() + " mb1=" + mb1.ToString() + " mb2=" + mb2.ToString() + " tangent=" + tangent.ToString()+ " final pt="+myPoint.ToString());
                
            }
                

            else if (pt3.X == pt2.X)//m2 infinite
            {
                //MessageBox.Show("x1=x2");
                m1 = (pt1.Y - pt2.Y) / (double)(pt1.X - pt2.X);
                f1 = 1 / Math.Sqrt(1 + m1 * m1);
                mb1 = (f1 * m1 - 1) / f1;
                mb2 = (f1 * m1 + 1) / f1;
                tangent = Math.Abs((m1 - mb1) / (1 + m1 * mb1));

                if (tangent > 1) m3 = mb1;
                else             m3 = mb2;

                myPoint.X = (int)((m1 * pt4.X - pt4.Y - m3 * pt2.X + pt2.Y) / (m1 - m3) + 0.5);
                myPoint.Y = (int)((m3 * (m1 * pt4.X - pt4.Y) - m1 * (m3 * pt2.X - pt2.Y)) / (m1 - m3) + 0.5);

                //MessageBox.Show("m1=" + m1.ToString() + " f1=" + f1.ToString() + " mb1=" + mb1.ToString() + " mb2=" + mb2.ToString() + " tangent=" + tangent.ToString() + " final pt=" + myPoint.ToString());
            }
            else
            {
                //MessageBox.Show("last general cond");
                m1 = (pt1.Y - pt2.Y) / (double)(pt1.X - pt2.X);
                m2 = (pt3.Y - pt2.Y) / (double)(pt3.X - pt2.X);

                f1 = 1 / Math.Sqrt(1 + m1 * m1);
                f2 = 1 / Math.Sqrt(1 + m2 * m2);

                mb1 = (f1 * m1 - f2 * m2) / (f1 - f2);
                mb2 = (f1 * m1 + f2 * m2) / (f1 + f2);
                tangent = Math.Abs((m1 - mb1) / (1 + m1 * mb1));

                if (tangent > 1) m3 = mb1;
                else             m3 = mb2;

                myPoint.X = (int)((m1 * pt4.X - pt4.Y - m3 * pt2.X + pt2.Y) / (m1 - m3) + 0.5);
                myPoint.Y = (int)((m3 * (m1 * pt4.X - pt4.Y) - m1 * (m3 * pt2.X - pt2.Y)) / (m1 - m3) + 0.5);

                //MessageBox.Show("m1=" + m1.ToString() + "m2=" + m2.ToString() + " f1=" + f1.ToString() + " f2=" + f2.ToString() + " mb1=" + mb1.ToString() + " mb2=" + mb2.ToString() + " tangent=" + tangent.ToString() + " final pt=" + myPoint.ToString());
            }
            
            return myPoint;

        }
        Point find90intersection(Point pt1, Point pt2, Point pt3)
        {
            Point myPoint = new Point();
            double m;
            
            if (pt1.X == pt2.X)
            {
                myPoint.X = pt1.X;
                myPoint.Y = pt3.Y;
            }
            else if (pt1.Y == pt2.Y)
            {
                myPoint.X = pt3.X;
                myPoint.Y = pt1.Y;
            }
            else 
            {
                m = (pt2.Y - pt1.Y) / (double)(pt2.X - pt1.X);
                myPoint.X = (int)((pt3.Y + pt3.X / m + m * pt1.X - pt1.Y) / (m + 1 / m) + 0.5);
                myPoint.Y = (int)((pt1.Y / m - pt1.X + pt3.Y * m + pt3.X) / (m + 1 / m) + 0.5);
            }
            return myPoint;

        }
        Point findintersection(Point pt1, Point pt2, Point pt3, Point pt4)
        {
            Point myPoint = new Point();
            double m1, m2;
            

            if (pt2.X == pt1.X)
            {
                m2 = (pt4.Y - pt3.Y) / (double)(pt4.X - pt3.X);
                myPoint.X = (int)(pt1.X + 0.5);
                myPoint.Y = (int)(pt3.Y + m2 * (pt1.X - pt3.X) + 0.5);
                
            }
            else if (pt4.X == pt3.X)
            {
                m1 = (pt2.Y - pt1.Y) / (double)(pt2.X - pt1.X);
                myPoint.X = pt3.X;
                myPoint.Y = (int)(pt1.Y + m1 * (pt3.X - pt1.X) + 0.5);

            }
            else
            {
                m1 = (pt2.Y - pt1.Y) / (double)(pt2.X - pt1.X);
                m2 = (pt4.Y - pt3.Y) / (double)(pt4.X - pt3.X);
                myPoint.X = (int)((pt3.Y - pt1.Y - m2 * pt3.X + m1 * pt1.X) / (m1 - m2) + 0.5);
                myPoint.Y = (int)(((pt1.X - pt3.X) * m1 * m2 + pt3.Y * m1 - pt1.Y * m2) / (m1 - m2) + 0.5);
            }
            
            return myPoint;
                
        }

        private void picDoubleLine_MouseEnter(object sender, EventArgs e)
        {
       
        }

        private void picDoubleLine_MouseHover(object sender, EventArgs e)
        {
                        
        }

        private void btnDoubleLine_Click(object sender, EventArgs e)
        {

            double delta;
            int Sign_of_Y, Sign_of_X;
            myPen = new Pen(Color.Blue);
            myBitmap = new Bitmap(myTmpBitmap);
            myGraphics = Graphics.FromImage(myBitmap);

            //Important for generating Single trace
            //for (int join_no = 0; join_no < join_count; join_no++)
            //{
            //    MessageBox.Show("Start join_no = " + join_no.ToString());
            //    for (int i=0; i<join_trace[join_no].ptCount; i++)
            //    {
            //        MessageBox.Show(join_trace[join_no].mypts[i].ToString());
            //    }
            //}

            for (int join_no = 0; join_no < join_count; join_no++) //for small traces elimination
            {
                
                if (Distance(join_trace[join_no].mypts[join_trace[join_no].ptCount - 1], join_trace[join_no].mypts[join_trace[join_no].ptCount - 2]) < (radius + 2))
                {
                    //MessageBox.Show("last pts diff < 7");
                    //MessageBox.Show("dist=" + Distance(join_trace[join_no].mypts[join_trace[join_no].ptCount - 1], join_trace[join_no].mypts[join_trace[join_no].ptCount - 2]));
                    join_trace[join_no].ptCount -= 1;
                }

                if (Distance(join_trace[join_no].mypts[0], join_trace[join_no].mypts[1]) < (radius + 2))
                {
                    //MessageBox.Show("0 1 diff < 7");
                    //MessageBox.Show("pt0=" + join_trace[join_no].mypts[0].ToString() + " pt1=" + join_trace[join_no].mypts[1].ToString());
                    //MessageBox.Show("dist=" + Distance(join_trace[join_no].mypts[0], join_trace[join_no].mypts[1]));
                    for (int i = 0; i <= join_trace[join_no].ptCount - 2; i++)
                    {
                        join_trace[join_no].mypts[i] = join_trace[join_no].mypts[i + 1];
                    }
                    join_trace[join_no].ptCount -= 1;
                }

            }

            for (int join_no = 0; join_no < join_count; join_no++)
            {
                XDiff = join_trace[join_no].mypts[1].X - join_trace[join_no].mypts[0].X;
                YDiff = join_trace[join_no].mypts[1].Y - join_trace[join_no].mypts[0].Y;
                Sign_of_Y = Math.Sign(YDiff); Sign_of_X = Math.Sign(XDiff);
                //double ang_deg;
                delta = (radius) * 0.27 / (Math.Sqrt(Sign_of_X * Sign_of_X + Sign_of_Y * Sign_of_Y));//width 
                //radius = (int)(delta / 1.4);

                for (int i = 0; i < 8; i++)//for first component
                {
                    if (XDiff == 0) angle = (Math.Sign(YDiff)) * pi / 2 - 0.3926 - i * pi / 4;
                    else if (XDiff < 0) angle = -pi + Math.Atan((YDiff / (double)XDiff)) - 0.3926 - i * pi / 4;
                    else angle = Math.Atan(YDiff / (double)XDiff) - 0.3926 - i * pi / 4;
                    //ang_deg = angle * 180 / pi;
                    //MessageBox.Show("Hi="+Math.Atan((19/(double)20)).ToString());
                    //MessageBox.Show("1st Cmp XDiff, YDiff = ("+XDiff.ToString()+", "+YDiff.ToString()+") Angle = " + ang_deg.ToString());

                    join_trace[join_no].mycmppts1[i] = new Point((int)(join_trace[join_no].mypts[0].X + radius * Math.Cos(angle) + 0.5), (int)(join_trace[join_no].mypts[0].Y + radius * Math.Sin(angle) + 0.5));
                }

                join_trace[join_no].mydblpts1[0] = new Point((int)(join_trace[join_no].mypts[0].X + Sign_of_Y * delta + 0.5), (int)(join_trace[join_no].mypts[0].Y - Sign_of_X * delta + 0.5));  //offset setting
                join_trace[join_no].mydblpts2[0] = new Point((int)(join_trace[join_no].mypts[0].X - Sign_of_Y * delta + 0.5), (int)(join_trace[join_no].mypts[0].Y + Sign_of_X * delta + 0.5));

                //if (join_no == 2)
                //{
                //    MessageBox.Show(" dbl1[0]=" + join_trace[join_no].mydblpts1[0].ToString() + " dbl2[0]=" + join_trace[join_no].mydblpts2[0].ToString());
                //}
                //Managing for second component

                XDiff = join_trace[join_no].mypts[join_trace[join_no].ptCount - 2].X - join_trace[join_no].mypts[join_trace[join_no].ptCount - 1].X;
                YDiff = join_trace[join_no].mypts[join_trace[join_no].ptCount - 2].Y - join_trace[join_no].mypts[join_trace[join_no].ptCount - 1].Y;
                //if (join_no == 2)
                //{
                //    MessageBox.Show("XDiff =" + XDiff.ToString() + " YDiff =" + YDiff.ToString());
                //}

                for (int i = 0; i < 8; i++)//for second component
                {
                    if (XDiff == 0) angle = (Math.Sign(YDiff)) * pi / 2 - 0.3926 - i * pi / 4;
                    else if (XDiff < 0) angle = -pi + Math.Atan((YDiff / (double)XDiff)) - 0.3926 - i * pi / 4;
                    else angle = Math.Atan(YDiff / (double)XDiff) - 0.3926 - i * pi / 4;
                    join_trace[join_no].mycmppts2[i] = new Point((int)(join_trace[join_no].mypts[join_trace[join_no].ptCount - 1].X + radius * Math.Cos(angle) + 0.5), (int)(join_trace[join_no].mypts[join_trace[join_no].ptCount - 1].Y + radius * Math.Sin(angle) + 0.5));
                }

                if (join_trace[join_no].ptCount > 2)
                {
                    //if (join_no == 2)
                    //{
                    //    MessageBox.Show("ptcount=" + join_trace[join_no].ptCount.ToString());
                    //}
                    for (int point_no = 1; point_no <= join_trace[join_no].ptCount - 2; point_no++)//excluding end points
                    {
                        //if (join_no == 2)
                        //{
                        //    MessageBox.Show("Note: pt no = " + point_no.ToString() + join_trace[join_no].mypts[point_no - 1].ToString() + " " + join_trace[join_no].mypts[point_no].ToString() + " " + join_trace[join_no].mypts[point_no + 1].ToString() + " " + join_trace[join_no].mydblpts1[point_no - 1].ToString());
                        //}
                        join_trace[join_no].mydblpts1[point_no] = bisector_intersection(join_trace[join_no].mypts[point_no - 1], join_trace[join_no].mypts[point_no], join_trace[join_no].mypts[point_no + 1], join_trace[join_no].mydblpts1[point_no - 1]);
                        join_trace[join_no].mydblpts2[point_no] = bisector_intersection(join_trace[join_no].mypts[point_no - 1], join_trace[join_no].mypts[point_no], join_trace[join_no].mypts[point_no + 1], join_trace[join_no].mydblpts2[point_no - 1]);
                        //if (join_no == 2)
                        //{
                        //    MessageBox.Show("wer dbl1[" + point_no.ToString() + "]=" + join_trace[join_no].mydblpts1[point_no].ToString() + " dbl2[" + point_no.ToString() + "]=" + join_trace[join_no].mydblpts2[point_no].ToString());
                        //}
                    }
                }
                join_trace[join_no].mydblpts1[join_trace[join_no].ptCount - 1] = find90intersection(join_trace[join_no].mycmppts2[0], join_trace[join_no].mycmppts2[7], join_trace[join_no].mydblpts1[join_trace[join_no].ptCount - 2]);
                join_trace[join_no].mydblpts1[0] = find90intersection(join_trace[join_no].mycmppts1[0], join_trace[join_no].mycmppts1[7], join_trace[join_no].mydblpts1[1]);

                join_trace[join_no].mydblpts2[join_trace[join_no].ptCount - 1] = find90intersection(join_trace[join_no].mycmppts2[0], join_trace[join_no].mycmppts2[7], join_trace[join_no].mydblpts2[join_trace[join_no].ptCount - 2]);
                join_trace[join_no].mydblpts2[0] = find90intersection(join_trace[join_no].mycmppts1[0], join_trace[join_no].mycmppts1[7], join_trace[join_no].mydblpts2[1]);

            }

            //for (int join_no = 0; join_no < join_count; join_no++)
            //{
            //    MessageBox.Show("join_no = " + join_no.ToString());
            //    for (int i = 0; i < join_trace[join_no].ptCount; i++)
            //    {
            //        MessageBox.Show(join_trace[join_no].mypts[i].ToString());
            //    }
            //    MessageBox.Show("Starting first comp");
            //    for (int i = 0; i < 8; i++)//for first component
            //    {
            //        MessageBox.Show(join_trace[join_no].mycmppts1[i].ToString());
            //    }
            //    MessageBox.Show("Starting Second comp");
            //    for (int i = 0; i < 8; i++)//for second component
            //    {
            //        MessageBox.Show(join_trace[join_no].mycmppts2[i].ToString());
            //    }
            //    MessageBox.Show("Starting first double line ");
            //    for (int point_no = 0; point_no < join_trace[join_no].ptCount; point_no++)
            //    {
            //        MessageBox.Show(join_trace[join_no].mydblpts1[point_no].ToString());
            //    }
            //    MessageBox.Show("Starting 2nd double line ");
            //    for (int point_no = 0; point_no < join_trace[join_no].ptCount; point_no++)
            //    {
            //        MessageBox.Show(join_trace[join_no].mydblpts2[point_no].ToString());
            //    }

            //}
            for (int join_no = 0; join_no < join_count; join_no++)
            {
                allcount = 0;
                for (int i = 0; i < 8; i++)
                {
                    join_trace[join_no].allpts[allcount++] = join_trace[join_no].mycmppts1[i];
                }

                for (int point_no = 0; point_no < join_trace[join_no].ptCount; point_no++)
                {
                    join_trace[join_no].allpts[allcount++] = join_trace[join_no].mydblpts2[point_no];
                }

                for (int i = 0; i < 8; i++)
                {
                    join_trace[join_no].allpts[allcount++] = join_trace[join_no].mycmppts2[i];
                }
                for (int point_no = 0; point_no < join_trace[join_no].ptCount; point_no++)
                {
                    join_trace[join_no].allpts[allcount++] = join_trace[join_no].mydblpts1[join_trace[join_no].ptCount - 1 - point_no];
                }
                //Final Double Drawing

                for (int i = 0; i < allcount - 1; i++)
                {
                    myGraphics.DrawLine(myPen, join_trace[join_no].allpts[i], join_trace[join_no].allpts[i + 1]);
                    picDoubleLine.Image = myBitmap;
                }
                myPen2 = new Pen(Color.Red);
                join_trace[join_no].allcount = allcount - 1;
                //MessageBox.Show("join_no=" + join_no.ToString());
                
            }

        }

        private void make_Packet()
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            
            for (int join_no = 0; join_no < join_count; join_no++)
            {
               
                #region drill off
                DelayX = 0; DelayY = 0;

                if (join_no == 0)
                {
                    XD = join_trace[join_no].allpts[0].X;
                    YD = join_trace[join_no].allpts[0].Y;
                }
                else
                {
                    XD = join_trace[join_no].allpts[0].X - join_trace[join_no - 1].allpts[0].X;
                    YD = join_trace[join_no].allpts[0].Y - join_trace[join_no - 1].allpts[0].Y;
                }
                
                //MessageBox.Show("For Drill OFF: XD=" + XD.ToString() + " YD=" + YD.ToString());
                XStep = (int)(Math.Ceiling(Math.Abs(XD) *25.4/ (ResX*ResolutionFactor))); 
                YStep = 0;
                MessageBox.Show("XStep=" + XStep.ToString() + " YStep=" + YStep.ToString());

                #region part 1  
                //X Step Greater
                DelayX = (int)PrimDelay;
                DelayY = 0;
                               
                //MessageBox.Show("DelayX=" + DelayX.ToString() + " DelayY=" + DelayY.ToString());
                XDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayX * Math.Pow(10, -3)) / 12);
                YDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayY * Math.Pow(10, -3)) / 12);
               MessageBox.Show("XDelay=" + XDelay.ToString() + " YDelay=" + YDelay.ToString());

                DataByte[0] = Convert.ToByte((int)(0)); // DataByte[0].0 bit is 0 to indicate milling OFF

                if (XD >= 0) DataByte[0] += Convert.ToByte((int)(2)); // taking clk (1) wise direction positive
                else if (XD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative 
                
                DataByte[0] += Convert.ToByte((int)(8)); // this indicates the undergoing of a process not manual control
                DataByte[1] = Convert.ToByte((int)(XStep / 256));
                DataByte[2] = Convert.ToByte(decimal.Remainder(XStep, 256));
                DataByte[3] = Convert.ToByte((int)(YStep / 256));
                DataByte[4] = Convert.ToByte(decimal.Remainder(YStep, 256));
                DataByte[5] = Convert.ToByte((int)(XDelay / 256));
                DataByte[6] = Convert.ToByte(decimal.Remainder(XDelay, 256));
                DataByte[7] = Convert.ToByte((int)(YDelay / 256));
                DataByte[8] = Convert.ToByte(decimal.Remainder(YDelay, 256));
                Sendserial();

                #endregion

                #region part 2
                //Ystep greater
                XStep = 0;
                YStep = (int)(Math.Ceiling(Math.Abs(YD) * 25.4 / (ResY * ResolutionFactor))); 
                DelayY = (int)PrimDelay;
                DelayX = 0;
                
                //MessageBox.Show("DelayX=" + DelayX.ToString() + " DelayY=" + DelayY.ToString());
                XDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayX * Math.Pow(10, -3)) / 12);
                YDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayY * Math.Pow(10, -3)) / 12);
              MessageBox.Show("XDelay=" + XDelay.ToString() + " YDelay=" + YDelay.ToString());

                DataByte[0] = Convert.ToByte((int)(0)); // DataByte[0].0 bit is 0 to indicate milling OFF

                if (YD >= 0) DataByte[0] += Convert.ToByte((int)(4)); // taking clk (1) wise direction positive
                else if (YD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative

                DataByte[0] +=Convert.ToByte((int)(8)); // this indicates the undergoing of a process not manual control
                DataByte[1] = Convert.ToByte((int)(XStep / 256));
                DataByte[2] = Convert.ToByte(decimal.Remainder(XStep, 256));
                DataByte[3] = Convert.ToByte((int)(YStep / 256));
                DataByte[4] = Convert.ToByte(decimal.Remainder(YStep, 256));
                DataByte[5] = Convert.ToByte((int)(XDelay / 256));
                DataByte[6] = Convert.ToByte(decimal.Remainder(XDelay, 256));
                DataByte[7] = Convert.ToByte((int)(YDelay / 256));
                DataByte[8] = Convert.ToByte(decimal.Remainder(YDelay, 256));
                Sendserial();
                
                #endregion
                
                #endregion

                MessageBox.Show("Starting join_no " + join_no.ToString());
                for (int point_no = 0; point_no < join_trace[join_no].allcount ; point_no++)
                {
                    myPen.Color = Color.Red;

                    myGraphics.DrawLine(myPen, join_trace[join_no].allpts[point_no], join_trace[join_no].allpts[point_no + 1]);
                    picDoubleLine.Image = myBitmap;
                                                                             
                    DelayX = 0; DelayY = 0;
                    if(point_no == join_trace[join_no].allcount-1)
                    {
                        XD = join_trace[join_no].allpts[0].X - join_trace[join_no].allpts[point_no].X;// total displacement in X
                        YD = join_trace[join_no].allpts[0].Y - join_trace[join_no].allpts[point_no].Y;// total displacement in Y
                    }
                    else //for completion of the loop
                    {
                        XD = join_trace[join_no].allpts[point_no + 1].X - join_trace[join_no].allpts[point_no].X;// total displacement in X
                        YD = join_trace[join_no].allpts[point_no + 1].Y - join_trace[join_no].allpts[point_no].Y;// total displacement in Y
                    }

                    XStep = (int)(Math.Ceiling(Math.Abs(XD) * 25.4 / (ResX * ResolutionFactor)));
                    YStep = (int)(Math.Ceiling(Math.Abs(YD) * 25.4 / (ResY * ResolutionFactor)));
                    
                    // cannot draw line less than 20 degree and greater than 70 degree
                    if (XStep >= 3 * YStep)
                        YStep = 0;
                    if (YStep >= 3 * XStep)
                        XStep = 0;
             
                    if (XStep >= YStep)
                    {
                        DelayX = (int)PrimDelay;
                        if (YStep != 0) DelayY = (int)(PrimDelay * XStep / YStep);
                        else DelayY = 0;
                    }
                    else if (XStep < YStep)
                    {
                        DelayY = (int)PrimDelay;
                        if (XStep != 0) DelayX = (int)(PrimDelay * YStep / XStep);
                        else DelayX = 0;
                    }
                    //MessageBox.Show("DelayX=" + DelayX.ToString() + " DelayY=" + DelayY.ToString());
                    XDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayX * Math.Pow(10, -3)) / 12);
                    YDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayY * Math.Pow(10, -3)) / 12);
                    //MessageBox.Show("XDelay=" + XDelay.ToString() + " YDelay=" + YDelay.ToString());
                    //Serially Transmitting Code with Drill Bit 'ON' 
                    
                    DataByte[0] = Convert.ToByte((int)(1)); // DataByte[0].0 bit is 1 to indicate milling ON

                    if (XD >= 0) DataByte[0] += Convert.ToByte((int)(2)); // taking clk (1) wise direction positive
                    else if (XD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative 
                    if (YD >= 0) DataByte[0] += Convert.ToByte((int)(4)); // taking clk (1) wise direction positive
                    else if (YD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative

                    DataByte[0] += Convert.ToByte((int)(8)); // this indicates the undergoing of a process not manual control
                    DataByte[1] = Convert.ToByte((int)(XStep / 256));
                    DataByte[2] = Convert.ToByte(decimal.Remainder(XStep, 256));
                    DataByte[3] = Convert.ToByte((int)(YStep / 256));
                    DataByte[4] = Convert.ToByte(decimal.Remainder(YStep, 256));
                    DataByte[5] = Convert.ToByte((int)(XDelay / 256));
                    DataByte[6] = Convert.ToByte(decimal.Remainder(XDelay, 256));
                    DataByte[7] = Convert.ToByte((int)(YDelay / 256));
                    DataByte[8] = Convert.ToByte(decimal.Remainder(YDelay, 256));

                    Sendserial();
                    //if(point_no!=join_trace[join_no].allcount-1)
                    //    myGraphics.DrawLine(myPen2, join_trace[join_no].allpts[point_no], join_trace[join_no].allpts[point_no + 1]);
                    //else
                    //    myGraphics.DrawLine(myPen2, join_trace[join_no].allpts[point_no], join_trace[join_no].allpts[0]);
                    //picDoubleLine.Image = myBitmap;

                }
                MessageBox.Show("Ending join no " + join_no.ToString());

                //Serially Transmitting Code with Drill Bit 'OFF'
            }
        }

        public void Sendserial()
        {
            //serialPort1.Open();
            while (CTSState == true)
            {
                // counter
                // check if RTS false, CTS false then only send the data
                // unless both false, undergo looping
            }

            serialPort1.RtsEnable = true; // assert RTS low to indicate Ready to Send
            RTSState = true;
            while (CTSState == false)
            {
                // undergo loop until CTS is asserted low by the controller
            }
            if (CTSState == true)
            {
                //serialPort1.DataBits = 7;    // control word 7 bits  + 1 parity
                //serialPort1.Parity = System.IO.Ports.Parity.Odd; // using odd parity
                //serialPort1.RtsEnable = false;   // RTS high to indicate the start of transmission
                //RTSState = false;
                //serialPort1.Write(DataByte, 0, 1);// transmit the first databyte, the control word with parity check
                //serialPort1.DataBits = 8;    // data 8 bit
                //serialPort1.Parity = System.IO.Ports.Parity.None; // no parity 
                //serialPort1.Write(DataByte, 1, 8);// next send all the data to the microcontroller
                serialPort1.Write(DataByte, 0, 9);// next send all the data to the microcontroller
                serialPort1.RtsEnable = false;
                RTSState = false;

            }
            //MessageBox.Show("Sending Done"); 
            //serialPort1.Close();   

        }

        private void serialPort1_PinChanged(object sender, System.IO.Ports.SerialPinChangedEventArgs e)
        {
            CTSState = serialPort1.CtsHolding;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            byte[] bytes = {0x00, 0x12, 0x34, 0x56, 0xAA, 0x55, 0xFF};
            char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7','8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                ///MessageBox.Show(b.ToString());
                chars[i * 2] = hexDigits[b >> 4];
                //MessageBox.Show("chars[i*2]=" + chars[i * 2].ToString());
                chars[i * 2 + 1] = hexDigits[b & 0xF];
                //MessageBox.Show("chars[i * 2 + 1]=" + chars[i * 2 + 1].ToString());
            }
            //Convert.ToByte(
            
        }
        double Distance(Point pt1, Point pt2)
        {
            double distance;
            distance=Math.Sqrt((pt1.X-pt2.X)*(pt1.X-pt2.X)+(pt1.Y-pt2.Y)*(pt1.Y-pt2.Y));
            return distance;
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            XD = 0;
            YD = 10;
            XStep = 0;
            YStep = (int)(Math.Ceiling(Math.Abs(YD) / ResY));
            DelayY = (int)PrimDelay;
            DelayX = 0;

            //MessageBox.Show("DelayX=" + DelayX.ToString() + " DelayY=" + DelayY.ToString());
            XDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayX * Math.Pow(10, -3)) / 12);
            YDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayY * Math.Pow(10, -3)) / 12);
            //MessageBox.Show("XDelay=" + XDelay.ToString() + " YDelay=" + YDelay.ToString());

            DataByte[0] = Convert.ToByte((int)(0)); // DataByte[0].0 bit is 0 to indicate milling OFF

            if (YD >= 0) DataByte[0] += Convert.ToByte((int)(4)); // taking clk (1) wise direction positive
            else if (YD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative

            DataByte[0] += Convert.ToByte((int)(0)); // this indicates the undergoing process is manual control
            DataByte[1] = Convert.ToByte((int)(XStep / 256));
            DataByte[2] = Convert.ToByte(decimal.Remainder(XStep, 256));
            DataByte[3] = Convert.ToByte((int)(YStep / 256));
            DataByte[4] = Convert.ToByte(decimal.Remainder(YStep, 256));
            DataByte[5] = Convert.ToByte((int)(XDelay / 256));
            DataByte[6] = Convert.ToByte(decimal.Remainder(XDelay, 256));
            DataByte[7] = Convert.ToByte((int)(YDelay / 256));
            DataByte[8] = Convert.ToByte(decimal.Remainder(YDelay, 256));
            Sendserial(); 

        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            DelayX = 0; DelayY = 0;
            XD = 10;
            YD = 0;

            XStep = (int)(Math.Ceiling(Math.Abs(XD) / ResX));
            YStep = 0;
           
            //X Step Greater
            DelayX = (int)PrimDelay;
            DelayY = 0;

            //MessageBox.Show("DelayX=" + DelayX.ToString() + " DelayY=" + DelayY.ToString());
            XDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayX * Math.Pow(10, -3)) / 12);
            YDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayY * Math.Pow(10, -3)) / 12);
            //MessageBox.Show("XDelay=" + XDelay.ToString() + " YDelay=" + YDelay.ToString());

            DataByte[0] = Convert.ToByte((int)(0)); // DataByte[0].0 bit is 0 to indicate milling OFF

            if (XD >= 0) DataByte[0] += Convert.ToByte((int)(2)); // taking clk (1) wise direction positive
            else if (XD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative 

            DataByte[0] += Convert.ToByte((int)(8)); // this indicates the undergoing of a process not manual control
            DataByte[1] = Convert.ToByte((int)(XStep / 256));
            DataByte[2] = Convert.ToByte(decimal.Remainder(XStep, 256));
            DataByte[3] = Convert.ToByte((int)(YStep / 256));
            DataByte[4] = Convert.ToByte(decimal.Remainder(YStep, 256));
            DataByte[5] = Convert.ToByte((int)(XDelay / 256));
            DataByte[6] = Convert.ToByte(decimal.Remainder(XDelay, 256));
            DataByte[7] = Convert.ToByte((int)(YDelay / 256));
            DataByte[8] = Convert.ToByte(decimal.Remainder(YDelay, 256));
            Sendserial();

        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            DelayX = 0; DelayY = 0;
            XD = -10;
            YD = 0;

            XStep = (int)(Math.Ceiling(Math.Abs(XD) / ResX));
            YStep = 0;

            //X Step Greater
            DelayX = (int)PrimDelay;
            DelayY = 0;

            //MessageBox.Show("DelayX=" + DelayX.ToString() + " DelayY=" + DelayY.ToString());
            XDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayX * Math.Pow(10, -3)) / 12);
            YDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayY * Math.Pow(10, -3)) / 12);
            //MessageBox.Show("XDelay=" + XDelay.ToString() + " YDelay=" + YDelay.ToString());

            DataByte[0] = Convert.ToByte((int)(0)); // DataByte[0].0 bit is 0 to indicate milling OFF

            if (XD >= 0) DataByte[0] += Convert.ToByte((int)(2)); // taking clk (1) wise direction positive
            else if (XD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative 

            DataByte[0] += Convert.ToByte((int)(8)); // this indicates the undergoing of a process not manual control
            DataByte[1] = Convert.ToByte((int)(XStep / 256));
            DataByte[2] = Convert.ToByte(decimal.Remainder(XStep, 256));
            DataByte[3] = Convert.ToByte((int)(YStep / 256));
            DataByte[4] = Convert.ToByte(decimal.Remainder(YStep, 256));
            DataByte[5] = Convert.ToByte((int)(XDelay / 256));
            DataByte[6] = Convert.ToByte(decimal.Remainder(XDelay, 256));
            DataByte[7] = Convert.ToByte((int)(YDelay / 256));
            DataByte[8] = Convert.ToByte(decimal.Remainder(YDelay, 256));
            Sendserial();
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            XD = 0;
            YD = -10;
            XStep = 0;
            YStep = (int)(Math.Ceiling(Math.Abs(YD) / ResY));
            DelayY = (int)PrimDelay;
            DelayX = 0;

            //MessageBox.Show("DelayX=" + DelayX.ToString() + " DelayY=" + DelayY.ToString());
            XDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayX * Math.Pow(10, -3)) / 12);
            YDelay = 65535 - (int)((Freq * Math.Pow(10, 6)) * (DelayY * Math.Pow(10, -3)) / 12);
            //MessageBox.Show("XDelay=" + XDelay.ToString() + " YDelay=" + YDelay.ToString());

            DataByte[0] = Convert.ToByte((int)(0)); // DataByte[0].0 bit is 0 to indicate milling OFF

            if (YD >= 0) DataByte[0] += Convert.ToByte((int)(4)); // taking clk (1) wise direction positive
            else if (YD < 0) DataByte[0] += Convert.ToByte((int)(0)); // taking Anticlk (0) wise direction negative

            DataByte[0] += Convert.ToByte((int)(8)); // this indicates the undergoing of a process not manual control
            DataByte[1] = Convert.ToByte((int)(XStep / 256));
            DataByte[2] = Convert.ToByte(decimal.Remainder(XStep, 256));
            DataByte[3] = Convert.ToByte((int)(YStep / 256));
            DataByte[4] = Convert.ToByte(decimal.Remainder(YStep, 256));
            DataByte[5] = Convert.ToByte((int)(XDelay / 256));
            DataByte[6] = Convert.ToByte(decimal.Remainder(XDelay, 256));
            DataByte[7] = Convert.ToByte((int)(YDelay / 256));
            DataByte[8] = Convert.ToByte(decimal.Remainder(YDelay, 256));
            Sendserial(); 

        }

        private void btnMill_Click(object sender, EventArgs e)
        {
            if (btnMill.Text == "Mill Up") { btnMill.Text = "Mill Dn"; flagAnup = true; }
            else { btnMill.Text = "Mill Up"; flagAnup = false; }
        }

        private void btnRight_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void fclsPCBGenerator_KeyUp(object sender, KeyEventArgs e)
        {
      
        }

        private void fclsPCBGenerator_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void picDoubleLine_Click(object sender, EventArgs e)
        {

        }


        }               
    
    }
