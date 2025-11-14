using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class DagloHelper
    { 
        public static IDagloModel GetDagloModel(string dagloChoice)
        {
            return new DagloModel();
        }        

        public static string GetWebSiteUrl()
        {
            return "https://daglo.ai";
        }
         

        public static string GetDagloFolder()
        { 
            var path = Path.Combine(Configuration.DataDirectory, "daglo");
            return Directory.Exists(path) ? path : null;
        }         
               
    }
}
