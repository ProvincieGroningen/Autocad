using System.Linq;
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
    }
}