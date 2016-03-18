using System;
using Autodesk.AutoCAD.ApplicationServices.Core;

namespace ProvincieGroningen.AutoCad
{
    public static class Utilities
    {
        public static void HandleError(Exception ex)
        {
            Application.ShowAlertDialog(ex.Message + " " + ex.StackTrace);
        }
    }
}