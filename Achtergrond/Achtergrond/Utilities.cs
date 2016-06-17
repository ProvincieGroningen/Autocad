using System;
using System.Drawing;
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
            if (ex is AchtergrondException)
            {
                Application.ShowAlertDialog(ex.Message);
            }
            else
            {
                Application.ShowAlertDialog(ex.Message + " " + ex.StackTrace);
            }
        }

        public static FileInfo GetFile(string formattedUrl, string fileName, string mimeType)
        {
            if (formattedUrl.StartsWith("file://"))
            {
                var fileInfo = new FileInfo(formattedUrl.Replace("file://", ""));
                if (!fileInfo.Exists)
                {
                    throw new AchtergrondException($"Bestand {formattedUrl} niet gevonden.");
                }
                return fileInfo;
            }

            return DownloadFile(formattedUrl, fileName, mimeType);
        }

        private static FileInfo DownloadFile(string formattedUrl, string fileName, string mimeType)
        {
            var request = WebRequest.CreateHttp(formattedUrl);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    throw new AchtergrondException($"Deze url {formattedUrl} levert niet de verwachtte respons.");
                }
                if (response.ContentType == mimeType)
                {
                    using (var fileStream = File.Create(fileName))
                    {
                        responseStream.CopyTo(fileStream);
                    }
                    return new FileInfo(fileName);
                }
                using (var reader = new StreamReader(responseStream))
                {
                    var data = reader.ReadToEnd();
                    var contentType = response.ContentType;
                    throw new AchtergrondException($"Onverwachte response: {contentType} ({data.Substring(0, 100)})");
                }
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