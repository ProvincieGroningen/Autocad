using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

            if (AutocadUtils.IsCoordinateSystem(epsg))
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
            if (rectangle.Length != 2)
                return;

            foreach (var tileFile in ImagesForRectangle(rectangle, config))
            {
                using (tileFile)
                {
                    RasterImage.AttachRasterImage(tileFile.TopLeft, tileFile.File, config.Schaal);
                }
            }
        }

        IEnumerable<TileFile> ImagesForRectangle(Point3d[] rectangle, TileConfig config)
        {
            return config.GetTilesForRectangle(rectangle).Select(tile => new TileFile
            {
                File = Download(tile.FormattedUrl()),
                TopLeft = tile.TopLeft,
            });
        }

        private FileInfo Download(string formattedUrl)
        {
            using (var client = new WebClient())
            {
                var fileName = Path.GetTempFileName();
                client.DownloadFile(formattedUrl, fileName);
                return new FileInfo(fileName);
            }
        }

        class TileFile : IDisposable
        {
            public FileInfo File;
            public Point3d TopLeft;

            public void Dispose()
            {
                File.Delete();
            }
        }
    }
}