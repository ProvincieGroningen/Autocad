using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Linq;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using OpenFileDialog = Autodesk.AutoCAD.Windows.OpenFileDialog;

namespace PGAutocad
{
    internal static class Achtergrond
    {
        private const string GeenAchtergrondGevonden =
            "Kan geen achtergrond vinden op deze plek. Gebruikt u wel het juiste coördinaten systeem of zit u in Layout?";

        public static void AchtergrondCommand()
        {
            var tileConfig = VraagConfiguratie();
            AchtergrondCommand(tileConfig);
        }

        private static TileConfig VraagConfiguratie()
        {
            var tilesConfig = TilesConfig.Get();
            var tileNames = tilesConfig.Select(t => t.Naam).ToArray();
            var tileName = AutocadUtils.GetCommand("\nSelecteer de achtergrond: ", tileNames, tileNames.First());
            Application.ShowAlertDialog(tileName);
            var tileConfig = tilesConfig.Single(t => t.Naam == tileName);
            return tileConfig;
        }

        private static void AchtergrondCommand(TileConfig tileConfig)
        {
            using (var tr = Application.DocumentManager.CurrentDocument.TransactionManager.StartTransaction()) { 
                var i = new GeomapImage();
                tr.GetObject(Application.DocumentManager.CurrentDocument.Database.CurrentSpaceId, OpenMode.ForWrite);
                var ri = RasterImageDef.GetImageDictionary(Application.DocumentManager.MdiActiveDocument.Database);

            }


        }

        private static void AchtergrondCommand_(TileConfig tileConfig)
        {
            try
            {
                var location = AutocadUtils.GetRectangle();
                if (location == null)
                    return;

                var tileNames = tileConfig.GetTileByLocation(location).ToArray();
                if (!tileNames.Any())
                {
                    Application.ShowAlertDialog(GeenAchtergrondGevonden);
                }
                foreach (var tileName in tileNames)
                {
                    var num = (short) Application.GetSystemVariable("FILEDIA");
                    Application.DocumentManager.CurrentDocument.SendStringToExecute("_FILEDIA " + 0 + " ", true, false,
                        false);
                    var str = $"_MAPIINSERT \"{tileName}\" N ";
                    Application.DocumentManager.CurrentDocument.SendStringToExecute(str, false, true, true);
                    Application.DocumentManager.CurrentDocument.SendStringToExecute("_FILEDIA " + num + " ", true, false,
                        false);
                }
            }
            catch (Exception ex)
            {
                Utilities.HandleError(ex);
            }
        }

        private static void Loadlufo(string luchtFotoFile)
        {
            const string dictName = "MY_IMAGE_NAME";
            RasterImageDef imageDef = null;
            var imageDefId = new ObjectId();
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            Application.DocumentManager.MdiActiveDocument.LockDocument();
            using (var trans = database.TransactionManager.StartTransaction())
                DefineImage(luchtFotoFile, dictName, ref imageDef, ref imageDefId, database, trans);
        }

        private static void DefineImage(string luchtFotoFile, string dictName, ref RasterImageDef imageDef,
            ref ObjectId imageDefId, Database db, Transaction trans)
        {
            try
            {
                var imageDictionary = RasterImageDef.GetImageDictionary(db);
                // ISSUE: explicit reference operation
                var imageDict = trans.GetObject(imageDictionary, 0) as DBDictionary;
                if (imageDict != null && imageDict.Contains(dictName))
                {
                    imageDefId = imageDict.GetAt(dictName);
                    imageDef = trans.GetObject(imageDefId, (OpenMode) 1) as RasterImageDef;
                }
                else
                    CreateTheRasterFile(luchtFotoFile, dictName, ref imageDef, ref imageDefId, trans, imageDict);
                imageDefId = CreateImage(imageDef, imageDefId, db, trans);
            }
            catch (Exception ex)
            {
                Utilities.HandleError(ex);
            }
        }

        private static void CreateTheRasterFile(string luchtFotoFile, string dictName, ref RasterImageDef imageDef,
            ref ObjectId imageDefId, Transaction trans, DBDictionary imageDict)
        {
            var openFileDialog = new OpenFileDialog("Open an Image file", luchtFotoFile, "tif", "Image File",
                OpenFileDialog.OpenFileDialogFlags.NoUrls);
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            imageDef = new RasterImageDef {SourceFileName = openFileDialog.Filename};
            imageDef.Load();
            imageDict.UpgradeOpen();
            imageDefId = imageDict.SetAt(dictName, imageDef);
            trans.AddNewlyCreatedDBObject(imageDef, true);
        }

        private static ObjectId CreateImage(RasterImageDef imageDef, ObjectId imageDefId, Database db, Transaction trans)
        {
            var rasterImage = new RasterImage
            {
                ImageDefId = imageDefId,
                ImageTransparency = true,
                ShowImage = true
            };
            trans.GetObject(db.BlockTableId, 0);
            var currentSpaceId = db.CurrentSpaceId;
            using (var blockTableRecord = currentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord)
            {
                blockTableRecord?.AppendEntity(rasterImage);
            }
            trans.AddNewlyCreatedDBObject(rasterImage, true);
            RasterImage.EnableReactors(true);
            rasterImage.AssociateRasterDef(imageDef);
            trans.Commit();
            return imageDefId;
        }
    }
}