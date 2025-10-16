namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public interface IDagloModel
    {
        string ModelFolder { get;  }
        void CreateModelFolder();
        DagloModel[] Models { get;  }
    }
}