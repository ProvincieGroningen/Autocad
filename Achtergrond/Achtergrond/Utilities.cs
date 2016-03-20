using System;
using System.IO;
using System.Net;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Geometry;

namespace ProvincieGroningen.AutoCad
{
    public static class Utilities
    {
        public static void HandleError(Exception ex)
        {
            Application.ShowAlertDialog(ex.Message + " " + ex.StackTrace);
        }

        public static FileInfo GetFile(string formattedUrl, string fileName)
        {
            if (formattedUrl.StartsWith("file://"))
                return new FileInfo(formattedUrl.Replace("file://", ""));

            using (var client = new WebClient())
            {
                client.DownloadFile(formattedUrl, fileName);
                return new FileInfo(fileName);
            }
        }

        public class TileFile : IDisposable
        {
            public FileInfo File;
            public Point3d BottomLeft;

            public void Dispose()
            {
            }
        }
    }
}