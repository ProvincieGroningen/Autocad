﻿using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Gis.Map.Platform;

namespace ProvincieGroningen.AutoCad
{
    static class AutocadUtils
    {
        public static string GetCommand(string message, string[] commands, string defaultCommand)
        {
            try
            {
                var options = new PromptKeywordOptions(message)
                {
                    AllowNone = false,
                };
                foreach (var command in commands)
                {
                    options.Keywords.Add(command);
                }
                options.Keywords.Default = defaultCommand;

                var result = Application.DocumentManager.CurrentDocument.Editor.GetKeywords(options);
                return commands.First(c => c.StartsWith(result.StringResult));
            }
            catch (Exception ex)
            {
                Utilities.HandleError(ex);
                return null;
            }
        }

        public static Point3d[] GetRectangle(string opdracht)
        {
            try
            {
                var start =
                    Application.DocumentManager
                        .MdiActiveDocument
                        .Editor
                        .GetPoint(new PromptPointOptions($"{opdracht}: Geef het eerst hoekpunt"));

                if (start.Status == PromptStatus.Cancel)
                    return null;

                var corner = Application.DocumentManager
                    .MdiActiveDocument
                    .Editor
                    .GetCorner($"{ opdracht}:Geef het volgende hoekpunt", start.Value);

                if (corner.Status == PromptStatus.Cancel)
                    return null;

                return new[] {start.Value, corner.Value};
            }
            catch (Exception ex)
            {
                Utilities.HandleError(ex);
                return null;
            }
        }


        public static bool IsCoordinateSystem(string coordinateSystemId)
        {
            return AcMapMap.GetCurrentMap().GetCoordinateSystemId() == coordinateSystemId;
        }
    }
}