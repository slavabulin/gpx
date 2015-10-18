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
        }

        static int[] ar_lat;
        static int[] ar_lon;
        decimal minLat, maxLat, minLon, maxLon;

        private void Form1_Load(object sender, EventArgs e)
        {
            //----------------------------------------------------------------------
            gpxType gpxtype = new gpxType();
            

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
                List<wptType> ptList = new List<wptType>();
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

                //minLat = ptList.ElementAt(0).lat; //minY
                //maxLat = ptList.ElementAt(0).lat; //maxY
                //minLon = ptList.ElementAt(0).lon; // minX
                //maxLon = ptList.ElementAt(0).lon; // maxX


                //преобразуем градусы в радианы
                minLat = (Decimal)Degr2Rad(180);
                minLon = (Decimal)Degr2Rad(180);
                maxLat = (Decimal)Degr2Rad(0);
                maxLon = (Decimal)Degr2Rad(0);

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

                decimal a =(maxLat - minLat) / 5800; //кол-во градусов в пикселе
                decimal b =(maxLon - minLon) / 600;

                ar_lat = new int[ptList.Count];
                ar_lon = new int[ptList.Count];

                for (int t = 0; t < ptList.Count; t++)
                {
                    ar_lat[t] = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t).lat - minLat) / a, 0));
                    ar_lon[t] = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t).lon - minLon) / b, 0));
                }

                trkSeg[] track = new trkSeg[ptList.Count - 1];
                double dt;
                for (int t = 0; t < ptList.Count - 1; t++)
                {
                    track[t] = new trkSeg();
                    track[t].start = new Point(ar_lon[t], 600 - ar_lat[t]);
                    track[t].end = new Point(ar_lon[t + 1], 600 - ar_lat[t + 1]);
                    track[t].dt = (ptList.ElementAt(t + 1).time - ptList.ElementAt(t).time).TotalSeconds;
                    //track[t].velocity = 
                }
            }
            else
            {
                Console.WriteLine("file doesnt contain needed info");
            }
            Console.ReadLine();
            //----------------------------------------------------------------------

           
        }

        public double Degr2Rad(decimal degr)
        {
            return Math.PI * Decimal.ToDouble(degr) / 180;
        }

        public decimal dist(Point start, Point end)
        {
            if (start == null || end == null) return Decimal.Zero;
           
                /*
                    function distance($lat1,$lng1,$lat2,$lng2) 
     { 
         // Convert degrees to radians. 
        $lat1=deg2rad($lat1); 
        $lng1=deg2rad($lng1); 
        $lat2=deg2rad($lat2); 
        $lng2=deg2rad($lng2); 
     
        // Calculate delta longitude and latitude. 
        $delta_lat=($lat2 - $lat1); 
        $delta_lng=($lng2 - $lng1); 
     
        return round( 6378137 * acos( cos( $lat1 ) * cos( $lat2 ) * cos( $lng1 - $lng2 ) + sin( $lat1 ) * sin( $lat2 ) ) ); 
     }
                 */
            return 0;
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


        private void DrawTrack()
        {
            for (int t = 0; t < ar_lat.Length - 1; t++)
            {
                using (System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Red))
                {
                    using (System.Drawing.Graphics formGraphics = this.CreateGraphics())
                    {
                        formGraphics.DrawLine(myPen, new Point(ar_lon[t], 600 - ar_lat[t]), new Point(ar_lon[t + 1], 600 - ar_lat[t + 1]));
                    }
                }
            }
        }
    }

    internal class trkSeg
    {
        internal Point start;
        internal Point end;
        internal Color color;
        internal double dt;
        internal decimal velocity;
    }
}
