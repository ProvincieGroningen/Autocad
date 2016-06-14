using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;

namespace ProvincieGroningen.AutoCad
{
    public class Main
    {
        [CommandMethod("Achtergrond", CommandFlags.ActionMacro)]
        // ReSharper disable once UnusedMember.Global
        public void AttachRasterImage()
        {
            var epsg = "Netherlands-RDNew";

            if (!AutocadUtils.IsCoordinateSystem(epsg))
            {
                Application.ShowAlertDialog($"Deze tekening heeft niet het juiste Coordinate System ({epsg})");
                return;
            }

            var configs = TilesConfig.Get();
            var command = AutocadUtils.GetCommand("Selecteer de gewenste laag:", configs.Select(c => c.Naam).ToArray(), configs.Select(c => c.Naam).First());
            var config = configs.First(c => c.Naam.StartsWith(command));
            if (config == null)
            {
                Application.ShowAlertDialog($"De configuratie voor {command} is niet gevonden.");
                return;
            }

            var rectangle = AutocadUtils.GetRectangle("Rechthoek");

            if (rectangle?.Length != 2)
            {
                Application.ShowAlertDialog($"Kan geen rechtoek bepalen (aantal hoekpunten: {rectangle?.Length.ToString() ?? "0"}).");
                return;
            }

            try
            {
                var imagesForRectangle = ImagesForRectangle(rectangle, config).ToArray();
                using (var autocadProgress = new ProgressMeter())
                {
                    autocadProgress.SetLimit(imagesForRectangle.Length);
                    autocadProgress.Start("Tegels worden in de tekening geplaatst");
                    foreach (var tileFile in imagesForRectangle)
                    {
                        using (tileFile)
                        {
                            RasterImage.AttachRasterImage(tileFile.BottomLeft, tileFile.File, config.TegelBreedte, config.TegelHoogte);
                        }
                        autocadProgress.MeterProgress();
                    }
                    autocadProgress.Stop();
                }
            }
            catch (System.Exception ex)
            {
                Utilities.HandleError(ex);
            }
        }

        static Utilities.TileFile[] ImagesForRectangle(Point3d[] rectangle, TileConfig config)
        {
            var dbFileName = Application.DocumentManager.CurrentDocument.Database.Filename;
            var dbPath = new FileInfo(dbFileName).DirectoryName;
            var tilesForRectangle = config.GetTilesForRectangle(rectangle.ToCoordinaat()).ToArray();
            if (tilesForRectangle.Length == 0)
                return new Utilities.TileFile[0];

            var autocadProgress = new ProgressMeter();
            autocadProgress.SetLimit(tilesForRectangle.Length);
            autocadProgress.Start("Tiles worden verzameld");
            var firstTile = GetTileFile(tilesForRectangle.First(), dbPath, autocadProgress);
            var tileFiles = tilesForRectangle
                .Skip(1)
                .AsParallel()
                .Select(tile => GetTileFile(tile, dbPath, autocadProgress))
                .ToArray();
            autocadProgress.Stop();
            autocadProgress.Dispose();
            return tileFiles.Union(new[] {firstTile}).ToArray();
        }

        static readonly object _lock = new object();

        private static Utilities.TileFile GetTileFile(TileConfigExtensions.TileReference tile, string dbPath, ProgressMeter progress)
        {
            lock (_lock)
            {
                progress.MeterProgress();
            }
            return new Utilities.TileFile
            {
                File = Utilities.GetFile(tile.FormattedUrl(), Path.Combine(dbPath, tile.FileName())),
                BottomLeft = new Point3d((double) tile.TopLeft.X, (double) tile.BottomRight.Y, 0),
            };
        }
    }
}