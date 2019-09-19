using System.Runtime.Serialization;

namespace TestApp.Models.Config
{
    [DataContract(Name = "settings", Namespace = "")]
    public class StoredSettings
    {
        [DataMember(Name = "width", Order = 0)]
        public uint Width { get; set; }

        [DataMember(Name = "height", Order = 1)]
        public uint Height { get; set; }

        [DataMember(Name = "title", Order = 2)]
        public string Title { get; set; }

        [DataMember(Name = "templateName", Order = 3)]
        public string TemplateFileName { get; set; }

        [DataMember(Name = "outPath", Order = 4)]
        public string OutPath { get; set; }

        [DataMember(Name = "fileName", Order = 5)]
        public string ResultFileName { get; set; }

        [DataMember(Name = "open", Order = 6)]
        public bool ShowTargetFile { get; set; }

        [DataMember(Name = "show", Order = 7)]
        public bool OpenTargetFolder { get; set; }

        [DataMember(Name = "overwrite", Order = 8)]
        public bool IsOverwrite { get; set; }

        public StoredSettings()
        {
        }

        public StoredSettings(ProjectSettings settings)
        {
            Width = settings.Width;
            Height = settings.Height;
            Title = settings.Title;
            TemplateFileName = settings.TemplateFileName;
            OutPath = settings.OutPath;
            ResultFileName = settings.SavedFileName;
            ShowTargetFile = settings.ShowTargetFile;
            OpenTargetFolder = settings.OpenTargetFolder;
            IsOverwrite = settings.IsOverwrite;
        }
    }
}