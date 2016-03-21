using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProvincieGroningen.AutoCad;

namespace TestAchtergrond
{
    [TestClass]
    public class TilesTest
    {
        [TestMethod]
        public void word_de_juiste_tegel_gevonden()
        {
            var config = new TileConfig
            {
                Naam = nameof(word_de_juiste_tegel_gevonden),
                LinksBoven = new Coordinaat(116000, 580000),
                TegelBreedte = 1000,
                TegelHoogte = 1000,
                Url = "{X_Links},{Y_Boven},{X_Rechts},{Y_Onder},{Rij},{Kolom}",
            };

            var tiles = config.GetTilesForRectangle(new[] { new Coordinaat(config.LinksBoven.X + 1, config.LinksBoven.Y - 1), new Coordinaat(config.LinksBoven.X + 2, config.LinksBoven.Y - 2) });

            PAssert.That(() => tiles.Count() == 1);

            var tile = tiles.Single();
            PAssert.That(() => tile.TopLeft.X == config.LinksBoven.X);
            PAssert.That(() => tile.TopLeft.Y == config.LinksBoven.Y);

            PAssert.That(() => tile.BottomRight.X == config.LinksBoven.X + config.TegelBreedte);
            PAssert.That(() => tile.BottomRight.Y == config.LinksBoven.Y - config.TegelHoogte);

            PAssert.That(() => tile.Rij == 1);
            PAssert.That(() => tile.Kolom == 1);

            PAssert.That(() => tile.FormattedUrl() == "116000,580000,117000,579000,1,1");
        }

        [TestMethod]
        public void wordt_de_configuratie_goed_ingelezen()
        {
            var config = @"<?xml version=""1.0"" encoding=""utf-8""?>

<TilesConfig>
  <TileConfig>
    <Naam>Test</Naam>
    <LinksBoven>
      <X>116000</X>
      <Y>580000</Y>
    </LinksBoven>
    <TegelBreedte>1000</TegelBreedte>
    <TegelHoogte>1000</TegelHoogte>
    <Url>file://E:/Ortho/Beelden_RGB_ecw_tegels/2015_{X}_{Y}_RGB_hrl.ecw</Url>
  </TileConfig>
</TilesConfig>
";

            var s = new XmlSerializer(typeof(TilesConfig));
            using (var sr = new StringReader(config))
            {
                var tilesConfig = (TilesConfig)s.Deserialize(sr);
                PAssert.That(() => tilesConfig.Count == 1);
                PAssert.That(() => tilesConfig.Single().Naam == "Test");
            }
        }

    }
}