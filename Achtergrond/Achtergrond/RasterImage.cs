using System;
using System.Diagnostics;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ProvincieGroningen.AutoCad
{
    public class RasterImage
    {
        public static void AttachRasterImage(Point3d insertionPoint, FileInfo imageFile, double unitsPerPixel)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var imageId = AddOrGetImage(acCurDb, acTrans, imageFile);

                var blockTable = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                var blockTableRecord = acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var acRaster = new Autodesk.AutoCAD.DatabaseServices.RasterImage())
                using (var rasterImageDef = acTrans.GetObject(imageId, OpenMode.ForWrite) as RasterImageDef)
                {
                    Debug.Assert(rasterImageDef != null, $"{nameof(rasterImageDef)} != null");
                    Debug.Assert(blockTableRecord != null, $"{nameof(blockTableRecord)} != null");

                    acRaster.ImageDefId = imageId;
                    acRaster.Orientation = GetCoordinateSystem(insertionPoint, acCurDb, rasterImageDef, unitsPerPixel);
                    acRaster.Rotation = 0;

                    blockTableRecord.AppendEntity(acRaster);
                    acTrans.AddNewlyCreatedDBObject(acRaster, true);

                    // Connect the raster definition and image together so the definition does not appear as "unreferenced" in the External References palette.
                    Autodesk.AutoCAD.DatabaseServices.RasterImage.EnableReactors(true);
                    acRaster.AssociateRasterDef(rasterImageDef);
                }

                acTrans.Commit();
            }
        }

        private static CoordinateSystem3d GetCoordinateSystem(Point3d insertionPoint, Database acCurDb, RasterImageDef rasterImageDef, double metersPerPixel)
        {
            // Use ImageWidth and ImageHeight to get the size of the image in pixels (1024 x 768).
            // Use ResolutionMMPerPixel to determine the number of millimeters in a pixel so you 
            // can convert the size of the drawing into other units or millimeters based on the 
            // drawing units used in the current drawing.

            // Check to see if the measurement is set to English (Imperial) or Metric units
            double ratio;
            switch (acCurDb.Measurement)
            {
                case MeasurementValue.English:
                    ratio = 25.4;
                    break;
                case MeasurementValue.Metric:
                    ratio = 1;
                    break;
                default:
                    throw new NotImplementedException(acCurDb.Measurement.ToString());
            }
            /*
            var vectorX = rasterImageDef.ResolutionMMPerPixel.X*rasterImageDef.Size.X/ratio;
            var vectorY = rasterImageDef.ResolutionMMPerPixel.Y*rasterImageDef.Size.Y/ratio;
            */
            var vectorX = metersPerPixel * rasterImageDef.Size.X / ratio;
            var vectorY = metersPerPixel * rasterImageDef.Size.Y / ratio;

            var width = new Vector3d(vectorX, 0, 0);
            var height = new Vector3d(0, vectorY, 0);

            var insertionPoint2 = new Point3d(insertionPoint.X, insertionPoint.Y - vectorY, 0);
            var coordinateSystem = new CoordinateSystem3d(insertionPoint2, width, height);

            return coordinateSystem;
        }



        private static ObjectId AddOrGetImage(Database database, Transaction transaction, FileInfo imageFileinfo)
        {
            var imageDictionaryForRead = AddOrGetImageDictionary(database, transaction);

            var imageName = imageFileinfo.Name;
            var imageId = imageDictionaryForRead.Contains(imageName)
                ? imageDictionaryForRead.GetAt(imageName)
                : CreateImage(transaction, imageFileinfo, imageDictionaryForRead, imageName);
            return imageId;
        }

        private static ObjectId CreateImage(Transaction transaction, FileInfo imageFileinfo, DBDictionary imageDictionaryForRead, string imageName)
        {
            var acRasterDefNew = new RasterImageDef {SourceFileName = imageFileinfo.FullName};

            acRasterDefNew.Load();
            imageDictionaryForRead.UpgradeOpen();

            var imageId = imageDictionaryForRead.SetAt(imageName, acRasterDefNew);

            transaction.AddNewlyCreatedDBObject(acRasterDefNew, true);
            return imageId;
        }

        private static DBDictionary AddOrGetImageDictionary(Database database, Transaction transaction)
        {
            var imageDictionary = RasterImageDef.GetImageDictionary(database);
            if (imageDictionary.IsNull)
            {
                imageDictionary = RasterImageDef.CreateImageDictionary(database);
            }
            var imageDictionaryForRead = transaction.GetObject(imageDictionary, OpenMode.ForRead) as DBDictionary;
            return imageDictionaryForRead;
        }
    }
}