using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.Geometry;

// ReSharper disable ClassNeverInstantiated.Global

namespace ProvincieGroningen.AutoCad
{
    public class TileConfig
    {
        [XmlElement]
        public string Naam { get; set; }

        [XmlElement]
        public Coordinaat LinksBoven { get; set; }

        [XmlElement]
        public decimal TegelBreedte { get; set; }


        [XmlElement]
        public decimal TegelHoogte { get; set; }

        [XmlElement]
        public int AantalRijen { get; set; }

        [XmlElement]
        public int AantalKolommen { get; set; }

        [XmlElement]
        public string Url { get; set; }

        [XmlElement]
        public string Extensie { get; set; }

        [XmlElement]
        public string Mimetype { get; set; }
    }

    public class Coordinaat
    {
        [XmlElement]
        public decimal X { get; set; }

        [XmlElement]
        public decimal Y { get; set; }

        public Coordinaat()
        {
        }

        public Coordinaat(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
    }

    public static class CoordinaatExtensies
    {
        public static Coordinaat[] ToCoordinaat(this Point3d[] rectangle)
        {
            return rectangle.Select(p => new Coordinaat((decimal) p.X, (decimal) p.Y)).ToArray();
        }
    }

    [XmlRoot("TilesConfig")]
    public class TilesConfig : List<TileConfig>
    {
        public static TilesConfig Get()
        {
            var s = new XmlSerializer(typeof (TilesConfig));
            var assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                using (var fs = new FileInfo(assemblyPath + "\\TilesConfig.xml").OpenRead())
                {
                    return (TilesConfig) s.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                Utilities.HandleError(ex);
                return Empty();
            }
        }

        private static TilesConfig Empty()
        {
            return new TilesConfig();
        }
    }
}