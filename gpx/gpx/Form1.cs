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

        decimal minLat, maxLat, minLon, maxLon, a, b;

        trackSegment[] track;

        private void Form1_Load(object sender, EventArgs e)
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

                //a = (maxLat - minLat) / 5800; //кол-во градусов в пикселе по горизонтали
                //b = (maxLon - minLon) / 600; //кол-во градусов в пикселе по вертикали

                a = (maxLat - minLat) / (base.Size.Height* 10);
                b = (maxLon - minLon) / base.Size.Width;
                
                //------------------------------------------------------------
                track = new trackSegment[ptList.Count - 1];
                for(int t=0; t<ptList.Count-1;t++)
                {
                    track[t] = new trackSegment();
                    track[t].start = new mapPoint();
                    track[t].end = new mapPoint();

                    //-------------------------------
                    track[t].start.Lat = (Decimal)Degr2Rad((Decimal)77.1539);
                    track[t].start.Lon = (Decimal)Degr2Rad((Decimal)120.398);
                    track[t].end.Lat = (Decimal)Degr2Rad((Decimal)77.1804);
                    track[t].end.Lon = (Decimal)Degr2Rad((Decimal)129.55);
                    //  225883
                    //-------------------------------

                    
                    track[t].start.Lat = ptList.ElementAt(t).lat;
                    track[t].start.Lon = ptList.ElementAt(t).lon;
                    track[t].start.time = ptList.ElementAt(t).time;

                    
                    track[t].end.Lat = ptList.ElementAt(t+1).lat;
                    track[t].end.Lon = ptList.ElementAt(t+1).lon;
                    track[t].end.time = ptList.ElementAt(t+1).time;

                    track[t].distance = CountDistance(track[t]);

                    //tracklenght = tracklenght + track[t].distance;

                    try
                    {
                        track[t].speed = CountSpeed(track[t]);
                    }
                    catch(Exception ex)
                    {
                        track[t].speed = 0;
                    }

                    track[t].start.Y = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t).lat - minLat) / a, 0));
                    track[t].start.X = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t).lon - minLon) / b, 0));
                    track[t].end.Y = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t+1).lat - minLat) / a, 0));
                    track[t].end.X = Decimal.ToInt32(Decimal.Round((ptList.ElementAt(t+1).lon - minLon) / b, 0));
                }
                //------------------------------------------------------------

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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public int CountDistance(trackSegment tracksegment)
        {
            if (tracksegment == null) return 0;

            decimal delta_lon = tracksegment.start.Lon - tracksegment.end.Lon;
            decimal delta_lat = tracksegment.start.Lat - tracksegment.end.Lat;
            decimal dist_in_decimal;
            double cos_lat1, cos_lat2, cos_lat1_lat2, sin_lat1, sin_lat2, cos_delta_lon1_lon2, dist_in_double, cos_lon1,cos_lon2;

            //return  Decimal.ToInt32((Decimal)(6372797.560856 * Math.Sqrt(Decimal.ToDouble(delta_lat * delta_lat) +
            //    Math.Cos(Decimal.ToDouble(tracksegment.start.Lat + tracksegment.end.Lat)) *
            //    Math.Cos(Decimal.ToDouble(tracksegment.start.Lat + tracksegment.end.Lat)) *
            //    Decimal.ToDouble(delta_lon * delta_lon))));
           

            cos_lat1 = Math.Cos(Decimal.ToDouble(tracksegment.start.Lat));
            cos_lat2 = Math.Cos(Decimal.ToDouble(tracksegment.end.Lat));
            cos_lat1_lat2 = Math.Cos(Decimal.ToDouble(tracksegment.start.Lat - tracksegment.end.Lat));
            sin_lat1 = Math.Sin(Decimal.ToDouble(tracksegment.start.Lat));
            sin_lat2 = Math.Sin(Decimal.ToDouble(tracksegment.end.Lat));
            cos_delta_lon1_lon2 = Math.Cos(Decimal.ToDouble(tracksegment.start.Lon - tracksegment.end.Lon));
            cos_lon1 = Math.Cos(Decimal.ToDouble(tracksegment.start.Lon));
            cos_lon2 = Math.Cos(Decimal.ToDouble(tracksegment.end.Lon));

            dist_in_double = 6378137 * Math.Acos(cos_lat1 * cos_lat2 * cos_delta_lon1_lon2 + sin_lat1 * sin_lat2);
            //double tmp = sin_lat1 * sin_lat2 + cos_lon1 * cos_lon2 * cos_delta_lon1_lon2;
            //dist_in_double = 6378137 * Math.Acos(tmp);
            try
            {
                dist_in_decimal = (decimal)dist_in_double;
                return Decimal.ToInt32(Decimal.Round(dist_in_decimal, 0));
            }
            catch (Exception ex)
            {
                return 0;
            }
            #region
            /*
            public double distanceTo(GeoPoint gp1, GeoPoint gp2) {  
        if (gp!=null){  
            int radius = 6371; // Километры 
            double dLat = Math.toRadians(gp2.latitude - gp1.latitude); 
            double dLong = Math.toRadians(gp2.longitude - gp1.longitude); 
            double lat1 = Math.toRadians(gp1.latitude); 
            double lat2 = Math.toRadians(gp2.latitude); 

            double a = Math.sin(dLat / 2) * Math.sin(dLat / 2) + Math.cos(lat1) * Math.cos(lat2) * Math.sin(dLong / 2) * Math.sin(dLong / 2); 
            double c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a)); 
            return c * R; 
             * 
             * --------------------------------------------------------------------------------------------------------
             * 
             R*arccos( sin(д1)*sin(д2) + cos(д1)*cos(д2)*cos(ш2-ш1) )
             
             * 
             * --------------------------------------------------------------------------------------------------------
             * 
             static inline CLLocationDistance GeodesicDistance(CLLocationCoordinate2D a, CLLocationCoordinate2D b) {
static const CLLocationDistance EarthRadiusInMeters = 6372797.560856;
static const double DegreeesToRad = 0.017453292519943295769236907684886;

CLLocationDegrees dtheta = (a.latitude - b.latitude) * DegreeesToRad;
CLLocationDegrees dlambda = (a.longitude - b.longitude) * DegreeesToRad;
CLLocationDegrees mean_t = (a.latitude + b.latitude) * DegreeesToRad / 2.0;
CLLocationDegrees cos_meant = cos(mean_t);

return EarthRadiusInMeters * sqrt(dtheta * dtheta + cos_meant * cos_meant * dlambda * dlambda);
}
             * 
             * 
             * --------------------------------------------------------------------------------------------------------* 
             * 
             * 
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

            #endregion
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
            Form1_Load(this,e);
        }

        private void DrawTrack()
        {            
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
    }


    public class mapPoint
    {
        public decimal Lon;
        public decimal Lat;
        public int X;
        public int Y;
        public DateTime time;
    }

    public class trackSegment
    {
        public  mapPoint start;
        public mapPoint end;
        public int distance;// расстояние в метрах, округленных до целых
        public decimal speed;// скорость на отрезке в м/с округленный до 3 знака после запятой
    }
}
