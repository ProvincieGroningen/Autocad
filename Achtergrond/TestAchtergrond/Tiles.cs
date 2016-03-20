using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ExpressionToCodeLib;
using ProvincieGroningen.AutoCad;
using Xunit;

namespace TestAchtergrond
{
    public class Tiles
    {
        [Fact]
        public void word_de_juiste_tegel_gevonden()
        {
            var config = new TileConfig
            {
                Naam = nameof(word_de_juiste_tegel_gevonden),
                LinksBoven = new Coordinaat(116000, 580000),
                TegelBreedte = 1000,
                TegelHoogte = 1000,
                Url = "file://test",
            };

            var tiles = config.GetTilesForRectangle(new[] {new Coordinaat(116100, 579500), new Coordinaat(116200, 579600) });

            PAssert.That(() => tiles.Count() == 1);

            var tile = tiles.Single();
            PAssert.That(() => tile.TopLeft.X == 116000);
            PAssert.That(() => tile.TopLeft.Y == 580000);

            PAssert.That(() => tile.TopLeft.X == tile.BottomLeft.X);
            PAssert.That(() => tile.TopLeft.Y == tile.BottomLeft.Y + config.TegelHoogte);

            PAssert.That(() => tile.Rij == 1);
            PAssert.That(() => tile.Kolom == 1);
        }

        [Fact]
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
                PAssert.That(() => tilesConfig.Count==1 );
                PAssert.That(() => tilesConfig.Single().Naam == "Test");
            }
        }

    }
}