using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.Geometry;

namespace ProvincieGroningen.AutoCad
{
    public static class TileConfigExtensions
    {
        public class TileReference
        {
            public TileConfig TileConfig;
            public int Rij;
            public int Kolom;
            public Point3d TopLeft;
            public Point3d BottomLeft;
        }

        public static IEnumerable<TileReference> GetTilesForRectangle(this TileConfig tileConfig, Point3d[] rectangle)
        {
            var minX = rectangle.Min(r => r.X);
            var maxX = rectangle.Max(r => r.X);
            var minY = rectangle.Min(r => r.Y);
            var maxY = rectangle.Max(r => r.Y);


            var minKolom = (int) Math.Floor((minX - tileConfig.LinksBoven.X)/tileConfig.TegelBreedte) + 1;
            var minRij = (int) Math.Floor((tileConfig.LinksBoven.Y - maxY)/tileConfig.TegelHoogte) + 1;


            var maxKolom = (int) Math.Floor((maxX - tileConfig.LinksBoven.X)/tileConfig.TegelBreedte) + 1;
            var maxRij = (int) Math.Floor((tileConfig.LinksBoven.Y - minY)/tileConfig.TegelHoogte) + 1;

            if (minKolom < 0) minKolom = 0;
            if (minRij < 0) minRij = 0;
            if (maxKolom < 0 || maxRij < 0) yield break;

            for (var kolom = minKolom; kolom <= maxKolom; kolom++)
            {
                for (var rij = minRij; rij <= maxRij; rij++)
                {
                    var x = tileConfig.LinksBoven.X + (kolom - 1)*tileConfig.TegelBreedte;
                    var y = tileConfig.LinksBoven.Y - (rij - 1)*tileConfig.TegelHoogte;
                    yield return new TileReference
                    {
                        TileConfig = tileConfig,
                        Kolom = kolom,
                        Rij = rij,
                        TopLeft = new Point3d(x, y, 0),
                        BottomLeft = new Point3d(x, y - tileConfig.TegelHoogte, 0),
                    };
                }
            }
        }

        public static string FormattedUrl(this TileReference reference)
        {
            return reference.TileConfig.Url
                .Replace("{Rij}", reference.Rij.ToString(CultureInfo.InvariantCulture))
                .Replace("{Kolom}", reference.Kolom.ToString(CultureInfo.InvariantCulture))
                .Replace("{X}", reference.TopLeft.X.ToString(CultureInfo.InvariantCulture))
                .Replace("{Y}", reference.TopLeft.Y.ToString(CultureInfo.InvariantCulture))
                ;
        }
    }
}