using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace gpx
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
         
        }
    }
}
/*
           gpxType gpxtype = new gpxType();

            using (FileStream fs = new FileStream("1.gpx", FileMode.Open, FileAccess.Read, FileShare.Read))
            { 
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(gpxType));

                try
                {
                   gpxtype = (gpxType)xmlSerializer.Deserialize(fs);
                }
                catch(Exception ex)
                {}
                finally
                {}
            }

            //check if folowing nodes exist
            // <gpx>
            //-<trk> - трек (м.б. несколько реков одном файле)
            //--<name> - имя для каждого из треков
            //--<trkseg> - каждый трек может состоять из нескольких сегментов 
            //            (в случае отсутсвия приема сигнала со спутника или выключения навигатора)
            //---<trkpt (lat, lon)> - каждый сегмент трека состоит из набора точек с широтой и долготой
            //----<ele>  - высота (над уровнем моря) для каждой из точек
            //----<time> - время, когда точка была записана

            if(gpxtype!=null&&
                gpxtype.trk!=null&&
                gpxtype.trk[0].trkseg!=null&&
                gpxtype.trk[0].trkseg[0].trkpt!=null&&
                gpxtype.trk[0].trkseg[0].trkpt[0].time!=null)
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

                Dictionary<DateTime, wptType> ptdictionary = ptList.ToDictionary<wptType, DateTime>(pt => pt.time);
                decimal minLat, maxLat, minLon, maxLon;

                //Отсортируем по времени
                //Console.WriteLine("Сортировано по времени:");
                //IEnumerable<wptType> list = ptList.OrderBy(pt => pt.time);
                
                //foreach (wptType pt in list)
                //{
                //    Console.WriteLine("{0}, {1} - {2}", pt.lon, pt.lat, pt.time);
                //}
                //Console.WriteLine(list.ElementAt(0).time.ToString());

                //Console.WriteLine("Сортировано по широте:");
                IEnumerable<wptType> list1 = ptList.OrderBy(pt => pt.lat);
                foreach (wptType pt in list1)
                {
                    Console.WriteLine("{0}, {1} - {2}", pt.lon, pt.lat, pt.time);
                }

                minLat = list1.ElementAt(0).lat;
                maxLat = list1.ElementAt(list1.Count()).lat;
                //Console.WriteLine(list1.ElementAt(0).time.ToString());
                //Console.WriteLine("Сортировано по долготе:");
                IEnumerable<wptType> list2 = ptList.OrderBy(pt => pt.lon);
                foreach (wptType pt in list1)
                {
                    Console.WriteLine("{0}, {1} - {2}", pt.lon, pt.lat, pt.time);
                }
                minLon = list2.ElementAt(0).lon;
                maxLon = list2.ElementAt(list2.Count()).lon;
                //Console.WriteLine(list2.ElementAt(0).time.ToString());
                Console.WriteLine("Минимальная широта - {0}/r Минимальная"
                + "долгота - {1}/r Максимальная широта - {2}/r Максимальная долгота - {3}", minLat, minLon, maxLat, maxLon);
            }
            else
            {
                Console.WriteLine("file doesnt contain needed info");
            }
            Console.ReadLine();

 */