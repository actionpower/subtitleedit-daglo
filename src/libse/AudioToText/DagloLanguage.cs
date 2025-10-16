using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class DagloLanguage
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public DagloLanguage(string code, string name)
        {
            Code = code;
            Name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name);
        }

        public override string ToString()
        {
            return Name;
        }

        public static DagloLanguage[] Languages
        {
            get
            { 
                var languages = new List<DagloLanguage>
                {
                    new DagloLanguage("en", "english"), 
                    new DagloLanguage("ko", "korean"),
                };                 

                return languages.ToArray();
            }
        }
    }
}
