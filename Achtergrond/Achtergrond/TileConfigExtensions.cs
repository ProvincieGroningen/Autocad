using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProvincieGroningen.AutoCad
{
    public static class TileConfigExtensions
    {
        public class TileReference
        {
            public TileConfig TileConfig;
            public int Rij;
            public int Kolom;
            public Coordinaat TopLeft;
            public Coordinaat BottomRight;
        }

        public static IEnumerable<TileReference> GetTilesForRectangle(this TileConfig tileConfig, Coordinaat[] rectangle)
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
                        TopLeft = new Coordinaat(x, y),
                        BottomRight = new Coordinaat(x + tileConfig.TegelBreedte, y - tileConfig.TegelHoogte),
                    };
                }
            }
        }

        public static string FormattedUrl(this TileReference reference)
        {
            return reference.TileConfig.Url
                .Replace("{Rij}", reference.Rij.ToString(CultureInfo.InvariantCulture))
                .Replace("{Kolom}", reference.Kolom.ToString(CultureInfo.InvariantCulture))
                .Replace("{X_Links}", reference.TopLeft.X.ToString(CultureInfo.InvariantCulture))
                .Replace("{Y_Boven}", reference.TopLeft.Y.ToString(CultureInfo.InvariantCulture))
                .Replace("{X_Rechts}", reference.BottomRight.X.ToString(CultureInfo.InvariantCulture))
                .Replace("{Y_Onder}", reference.BottomRight.Y.ToString(CultureInfo.InvariantCulture))
                ;
        }

        static Regex ValidateFileNameRegEx = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");

        public static string FileName(this TileReference reference)
        {
            var fileName = $@"{reference.TileConfig.Naam}_{reference.Rij}_{reference.Kolom}";
            return ValidateFileNameRegEx.Replace(fileName, "_") + reference.TileConfig.Extensie;
        }
    }
}