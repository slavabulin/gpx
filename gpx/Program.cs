using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace gpx
{
    class Program
    {
        static void Main(string[] args)
        {
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

                //посчитаем время и перемещение от каждой точки до следующей

                ////Отсортируем по времени
                //Console.WriteLine("Сортировано по времени:");
                //IEnumerable<wptType> list = ptList.OrderBy(pt => pt.time);
                //foreach (wptType pt in list)
                //{
                //    Console.WriteLine("{0}, {1} - {2}", pt.lon, pt.lat, pt.time);
                //}

                //Console.WriteLine("Сортировано по широте:");
                //IEnumerable<wptType> list1 = ptList.OrderBy(pt => pt.lat);
                //foreach (wptType pt in list1)
                //{
                //    Console.WriteLine("{0}, {1} - {2}", pt.lon, pt.lat, pt.time);
                //}
            }
            else
            {
                Console.WriteLine("file doesnt contain needed info");
            }
            Console.ReadLine();
        }
    }
}
