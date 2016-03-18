using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace ProvincieGroningen.AutoCad
{
    public class TileConfig
    {
        [XmlElement]
        public string Naam { get; set; }

        [XmlElement]
        public TegelCoordinaat LinksBoven { get; set; }

        [XmlElement]
        public double TegelBreedte { get; set; }


        [XmlElement]
        public double TegelHoogte { get; set; }

        [XmlElement]
        public int AantalRijen { get; set; }

        [XmlElement]
        public int AantalKolommen { get; set; }

        [XmlElement]
        public string Url { get; set; }
    }

    public class TegelCoordinaat
    {
        [XmlElement]
        public double X { get; set; }

        [XmlElement]
        public double Y { get; set; }
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