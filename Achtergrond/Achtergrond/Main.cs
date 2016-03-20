using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace ProvincieGroningen.AutoCad
{
    public class Main
    {
        [CommandMethod("Achtergrond", CommandFlags.ActionMacro)]
        // ReSharper disable once UnusedMember.Global
        public void AttachRasterImage()
        {
            var epsg = "EPSG:28992";

            if (!AutocadUtils.IsCoordinateSystem(epsg))
            {
                Application.ShowAlertDialog($"Deze tekening heeft niet het juiste Coordinate System ({epsg})");
                return;
            }

            var configs = TilesConfig.Get();
            var command = AutocadUtils.GetCommand("Selecteer de gewenste laag:", configs.Select(c => c.Naam).ToArray(), configs.Select(c => c.Naam).First());
            var config = configs.First(c => c.Naam.StartsWith(command));
            if (config == null)
                return;

            var rectangle = AutocadUtils.GetRectangle();

            if (rectangle?.Length != 2)
                return;

            try
            {
                foreach (var tileFile in ImagesForRectangle(rectangle, config))
                {
                    using (tileFile)
                    {
                        RasterImage.AttachRasterImage(tileFile.BottomLeft, tileFile.File, config.TegelBreedte, config.TegelHoogte);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Utilities.HandleError(ex);
            }
        }

        static IEnumerable<Utilities.TileFile> ImagesForRectangle(Point3d[] rectangle, TileConfig config)
        {
            var dbFileName = Application.DocumentManager.CurrentDocument.Database.Filename;
            var dbPath = new FileInfo(dbFileName).DirectoryName;
            var tilesForRectangle = config.GetTilesForRectangle(rectangle.ToCoordinaat());

            return tilesForRectangle
                .Select(tile => new Utilities.TileFile
                {
                    File = Utilities.GetFile(tile.FormattedUrl(), Path.Combine(dbPath, tile.FileName())),
                    BottomLeft = new Point3d((double) tile.TopLeft.X, (double) tile.BottomRight.Y, 0),
                });
        }
    }
}