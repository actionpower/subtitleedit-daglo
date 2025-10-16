using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class DagloModel : IDagloModel
    {
        public string[] Urls { get; set; }
        public string Size { get; set; }
        public string Name { get; set; }
        public bool Rename { get; set; }
        public string Folder { get; set; }
        public bool AlreadyDownloaded { get; set; }
        public long Bytes { get; set; }
        public bool Dynamic { get; set; }

        public override string ToString()
        {
            return $"{(AlreadyDownloaded ? "* " : string.Empty)}{Name} ({Size})";
        }

        public string ModelFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "daglo");

        public void CreateModelFolder()
        {
            var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            if (!Directory.Exists(ModelFolder))
            {
                Directory.CreateDirectory(ModelFolder);
            }
        }
        
        public DagloModel[] Models => new[]
        {
            new DagloModel
            {
                Name = "general",
                Size = "",
                Urls = new []{ "" },
            },
        };
    }
}
