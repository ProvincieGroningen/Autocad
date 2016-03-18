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
            public double X;
            public double Y;
        }

        public static IEnumerable<TileReference> GetTilesForRectangle(this TileConfig tileConfig, Point3d[] rectangle)
        {
            var minX = rectangle.Min(r => r.X);
            var maxX = rectangle.Max(r => r.X);
            var minY = rectangle.Min(r => r.Y);
            var maxY = rectangle.Max(r => r.Y);
            var minKolom = (int) Math.Floor((minX - tileConfig.LinksBoven.X)/tileConfig.TegelBreedte);
            var minRij = (int) Math.Floor((minY - tileConfig.LinksBoven.Y)/tileConfig.TegelHoogte);

            var maxKolom = (int) Math.Floor((maxX - tileConfig.LinksBoven.X)%tileConfig.TegelBreedte + 1);
            var maxRij = (int) Math.Floor((maxY - tileConfig.LinksBoven.Y)%tileConfig.TegelHoogte + 1);
            for (var kolom = minKolom; kolom <= maxKolom; kolom++)
            {
                for (var rij = minRij; rij <= maxRij; rij++)
                {
                    var x = tileConfig.LinksBoven.X - kolom*tileConfig.TegelBreedte;
                    var y = tileConfig.LinksBoven.Y - tileConfig.TegelHoogte*rij;
                    yield return new TileReference
                    {
                        TileConfig = tileConfig,
                        Kolom = kolom,
                        Rij = rij,
                        TopLeft = new Point3d(x, y, 0),
                        X = x,
                        Y = y,
                    };
                }
            }
        }

        public static string FormattedUrl(this TileReference reference)
        {
            return reference.TileConfig.Url
                .Replace("{Rij}", reference.Rij.ToString(CultureInfo.InvariantCulture))
                .Replace("{Kolom}", reference.Kolom.ToString(CultureInfo.InvariantCulture))
                .Replace("{X}", reference.X.ToString(CultureInfo.InvariantCulture))
                .Replace("{Y}", reference.Y.ToString(CultureInfo.InvariantCulture))
                ;
        }
    }
}