using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;



using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace gpx
{
    public partial class Form1 : Form
    {
        public Form1()
        {            
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(this.onMouseWheel);
            this.MouseClick += new MouseEventHandler(this.onMouseClick);
        }

        decimal minLat, maxLat, minLon, maxLon, a, b, wheel_mult=1;



        trackSegment[] track;
        List<wptType> ptList;

        void onMouseClick(object sender, System.Windows.Forms.MouseEventArgs e) 
        {
            this.Focus();
        }

        void onMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //if(e.Delta!=null)
            //{
            //    wheel_mult += e.Delta;
            //}
            //DrawTrack();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ReadData();
            FormTrack();
            CutOffPoints();
        }

        public double Degr2Rad(decimal degr)
        {
            return Math.PI * Decimal.ToDouble(degr) / 180;
        }

        public decimal CountSpeed(trackSegment tracksegment)
        {
            TimeSpan timespan;
            decimal timespan_dec, speed;
            timespan = tracksegment.start.time - tracksegment.end.time;

            try
            {
                timespan_dec = (Decimal)timespan.TotalSeconds;
                timespan_dec = Decimal.Round(timespan_dec, 0);
                speed = tracksegment.distance / timespan_dec;
                if (speed <= 0)
                {
                    return speed * (-1);
                }
                else
                    return speed;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int CountDistance(trackSegment tracksegment)
        {
            if (tracksegment == null) return 0;

            decimal dist_in_decimal;
            double cos_lat1, cos_lat2, dist_in_double, sin_delta_lat_half, sin_delta_lon_half;

            cos_lat1 = Math.Cos(Decimal.ToDouble(tracksegment.start.Lat));
            cos_lat2 = Math.Cos(Decimal.ToDouble(tracksegment.end.Lat));

            sin_delta_lat_half = Math.Sin(Decimal.ToDouble((tracksegment.end.Lat - tracksegment.start.Lat) / 2));
            sin_delta_lon_half = Math.Sin(Decimal.ToDouble((tracksegment.end.Lon - tracksegment.start.Lon) / 2));

            dist_in_double = 6372795 * 2 * Math.Asin(Math.Sqrt(sin_delta_lat_half * sin_delta_lat_half +
                cos_lat1 * cos_lat2 * sin_delta_lon_half * sin_delta_lon_half));

            try
            {
                dist_in_decimal = (decimal)dist_in_double;
                return Decimal.ToInt32(Decimal.Round(dist_in_decimal, 0));
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // If there is an image and it has a location, 
            // paint it when the Form is repainted.
            base.OnPaint(e);
            Graphics g = e.Graphics;
            // Insert code to paint the form here.
            DrawTrack();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            DrawTrack();
        }

        private void DrawTrack()
        {
            //------------------------------------------
            minLat = (Decimal)Degr2Rad(180);
            minLon = (Decimal)Degr2Rad(180);
            maxLat = (Decimal)Degr2Rad(0);
            maxLon = (Decimal)Degr2Rad(0);

            for (int t = 0; t < track.Length; t++)
            {
                if (minLat > track.ElementAt(t).start.Lat)
                {
                    minLat = track.ElementAt(t).start.Lat;
                }
                if (maxLat < track.ElementAt(t).start.Lat)
                {
                    maxLat = track.ElementAt(t).start.Lat;
                }
                if (minLat > track.ElementAt(t).end.Lat)
                {
                    minLat = track.ElementAt(t).end.Lat;
                }
                if (maxLat < track.ElementAt(t).end.Lat)
                {
                    maxLat = track.ElementAt(t).end.Lat;
                }

                if (minLon > track.ElementAt(t).start.Lon)
                {
                    minLon = track.ElementAt(t).start.Lon;
                }
                if (maxLon < track.ElementAt(t).start.Lon)
                {
                    maxLon = track.ElementAt(t).start.Lon;
                }
                if (minLon > track.ElementAt(t).end.Lon)
                {
                    minLon = track.ElementAt(t).end.Lon;
                }
                if (maxLon < track.ElementAt(t).end.Lon)
                {
                    maxLon = track.ElementAt(t).end.Lon;
                }
            }
            a = (maxLat - minLat) / (base.Size.Height*wheel_mult);
            b = (maxLon - minLon) / (base.Size.Width*wheel_mult);

            for (int t = 0; t < track.Length; t++)
            {
                track[t].start.Y = Decimal.ToInt32(Decimal.Round((track[t].start.Lat - minLat) / a, 0));
                track[t].start.X = Decimal.ToInt32(Decimal.Round((track[t].start.Lon - minLon) / b, 0));
                track[t].end.Y = Decimal.ToInt32(Decimal.Round((track[t].end.Lat - minLat) / a, 0));
                track[t].end.X = Decimal.ToInt32(Decimal.Round((track[t].end.Lon - minLon) / b, 0));
            }
            //------------------------------------------

            System.Drawing.Color color = System.Drawing.Color.Red;

            using (System.Drawing.Pen myPen = new System.Drawing.Pen(color))
            {
                using (System.Drawing.Graphics formGraphics = this.CreateGraphics())
                {
                    formGraphics.Clear(Color.WhiteSmoke);

                    for (int t = 0; t < track.Length - 1; t++)
                    {
                        if (track[t].speed > (Decimal)0.138) //больше 500 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78CC0000);
                        }
                        if (track[t].speed > (Decimal)0.222) //больше 800 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78FF0000);
                        }
                        if (track[t].speed > (Decimal)0.277) //больше 1000 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78FF3300);
                        }
                        if (track[t].speed > (Decimal)0.333) //больше 1200 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78FF9933);
                        }
                        if (track[t].speed > (Decimal)0.416) //больше 1500 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78FFCC00);
                        }
                        if (track[t].speed > (Decimal)0.472) //больше 1700 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78FFFF00);
                        }
                        if (track[t].speed > (Decimal)0.555) //больше 2000 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78CCFF33);
                        }
                        if (track[t].speed > (Decimal)0.694) //больше 2500 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x7899FF33);
                        }
                        if (track[t].speed > (Decimal)0.833) //больше 3000 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78009900);
                        }
                        if (track[t].speed > (Decimal)0.972) //больше 3500 м/ч
                        {
                            myPen.Color = System.Drawing.Color.FromArgb(0x78009933);
                        }
                        if (track[t].speed > (Decimal)2.0)
                        {
                            myPen.Color = System.Drawing.Color.Black;
                        }

                        formGraphics.DrawLine(myPen, new Point(track[t].start.X, base.Size.Height - track[t].start.Y),
                            new Point(track[t].end.X, base.Size.Height - track[t].end.Y));
                    }
                }
            }
        }

        void ReadData()
        {
            //----------------------------------------------------------------------
            gpxType gpxtype = new gpxType();
            int tracklenght = 0;

            using (FileStream fs = new FileStream("1.gpx", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(gpxType));

                try
                {
                    gpxtype = (gpxType)xmlSerializer.Deserialize(fs);
                }
                catch (Exception ex)
                { }
                finally
                { }
            }

            if (gpxtype != null &&
              gpxtype.trk != null &&
              gpxtype.trk[0].trkseg != null &&
              gpxtype.trk[0].trkseg[0].trkpt != null &&
              gpxtype.trk[0].trkseg[0].trkpt[0].time != null)
            {
                //если я хочу отобразить сегмент трека, мне нужен набор точек одного сегмента
                //склеим сегменты в один лист
                ptList = new List<wptType>();
                for (int u = 0; u < gpxtype.trk.Length; u++)
                {
                    for (int t = 0; t < gpxtype.trk[u].trkseg.Length; t++)
                    {
                        for (int y = 0; y < gpxtype.trk[u].trkseg[t].trkpt.Length; y++)
                        {
                            ptList.Add(gpxtype.trk[u].trkseg[t].trkpt[y]);
                        }
                    }
                }
            }
        }

        void FormTrack()
        {
            if (ptList == null) return;

            //minLat = ptList.ElementAt(0).lat; //minY
            //maxLat = ptList.ElementAt(0).lat; //maxY
            //minLon = ptList.ElementAt(0).lon; // minX
            //maxLon = ptList.ElementAt(0).lon; // maxX
            if((minLat==0)&&(minLon==0)&&(maxLat==0)&&(maxLon==0))
            {
                //преобразуем градусы в радианы
                minLat = (Decimal)Degr2Rad(180);
                minLon = (Decimal)Degr2Rad(180);
                maxLat = (Decimal)Degr2Rad(0);
                maxLon = (Decimal)Degr2Rad(0);


                //вычисляем крайние точки трека
                for (int y = 0; y < ptList.Count; y++)
                {
                    ptList.ElementAt(y).lat = (Decimal)Degr2Rad(ptList.ElementAt(y).lat);
                    ptList.ElementAt(y).lon = (Decimal)Degr2Rad(ptList.ElementAt(y).lon);

                    if (minLat >= ptList.ElementAt(y).lat)
                    {
                        minLat = ptList.ElementAt(y).lat;
                    }
                    if (maxLat <= ptList.ElementAt(y).lat)
                    {
                        maxLat = ptList.ElementAt(y).lat;
                    }
                    if (minLon >= ptList.ElementAt(y).lon)
                    {
                        minLon = ptList.ElementAt(y).lon;
                    }
                    if (maxLon <= ptList.ElementAt(y).lon)
                    {
                        maxLon = ptList.ElementAt(y).lon;
                    }
                }
            }
            
            //a = (maxLat - minLat) / (base.Size.Height * 10);
            a = (maxLat - minLat) / (base.Size.Height);
            b = (maxLon - minLon) / base.Size.Width;

            //------------------------------------------------------------
            track = new trackSegment[ptList.Count - 1];
            for (int t = 0; t < ptList.Count - 1; t++)
            {
                track[t] = new trackSegment();
                track[t].start = new mapPoint();
                track[t].end = new mapPoint();

                track[t].start.Lat = ptList.ElementAt(t).lat;
                track[t].start.Lon = ptList.ElementAt(t).lon;
                track[t].start.time = ptList.ElementAt(t).time;

                track[t].end.Lat = ptList.ElementAt(t + 1).lat;
                track[t].end.Lon = ptList.ElementAt(t + 1).lon;
                track[t].end.time = ptList.ElementAt(t + 1).time;

                ////--------test data--------------
                //track[t].start.Lat = (Decimal)Degr2Rad((Decimal)77.1539);
                //track[t].start.Lon = (Decimal)Degr2Rad((Decimal)(-120.398));
                //track[t].end.Lat = (Decimal)Degr2Rad((Decimal)77.1804);
                //track[t].end.Lon = (Decimal)Degr2Rad((Decimal)129.55);
                ////  2332669
                ////-------------------------------
                track[t].distance = CountDistance(track[t]);

                //tracklenght = tracklenght + track[t].distance;

                try
                {
                    track[t].speed = CountSpeed(track[t]);
                }
                catch (Exception ex)
                {
                    track[t].speed = 0;
                }

                track[t].start.Y = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t).lat - minLat) / a, 0));
                track[t].start.X = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t).lon - minLon) / b, 0));
                track[t].end.Y = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t + 1).lat - minLat) / a, 0));
                track[t].end.X = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t + 1).lon - minLon) / b, 0));
            }
        }

        void CutOffPoints()
        {
            if (track == null) return;

            List<trackSegment> segmentlist = new List<trackSegment>();
            for (int t = 0; t < track.Length; t++)
            {
                if (track[t].speed < (Decimal)(2.5))
                {
                    segmentlist.Add(track[t]);
                }
            }
            track = segmentlist.ToArray();
        }

        public class mapPoint
        {
            public decimal Lon;// в радианах
            public decimal Lat;// в радианах
            public int X;// координаты точки на форме
            public int Y;// координаты точки на форме
            public DateTime time;// время фиксации точки
        }

        public class trackSegment
        {
            public mapPoint start;// начальная точка отрезка 
            public mapPoint end;// конечная точка отрезка
            public int distance;// расстояние в метрах, округленных до целых
            public decimal speed;// скорость на отрезке в м/с 
        }
    }
}
