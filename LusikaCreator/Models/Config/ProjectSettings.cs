using GalaSoft.MvvmLight;
using TestApp.Models.Helpers;
using TestApp.Repository;

namespace TestApp.Models.Config
{
    public class ProjectSettings : ViewModelBase
    {
        private readonly VariablesRepository _variablesRepository;

        public uint Width
        {
            get { return _width; }
            set { Set(ref _width, value); }
        }
        private uint _width;

        public uint Height
        {
            get { return _height; }
            set { Set(ref _height, value); }
        }
        private uint _height;

        public string Title
        {
            get { return _title; }
            set
            {
                Set(ref _title, value);
                var tagReplacer = new TagReplacer(_variablesRepository);
                FormattedTitle = tagReplacer.GetFormattedString(value);
            }
        }
        private string _title;

        public string FormattedTitle
        {
            get { return _formattedTitle; }
            set { Set(ref _formattedTitle, value); }
        }
        private string _formattedTitle;

        public string OutPath
        {
            get { return _outPath; }
            set { Set(ref _outPath, value); }
        }
        private string _outPath;

        public bool ShowTargetFile
        {
            get { return _showTargetFile; }
            set { Set(ref _showTargetFile, value); }
        }
        private bool _showTargetFile;

        public bool OpenTargetFolder
        {
            get { return _openTargetFolder; }
            set { Set(ref _openTargetFolder, value); }
        }
        private bool _openTargetFolder;

        public bool IsOverwrite
        {
            get { return _isOverwrite; }
            set { Set(ref _isOverwrite, value); }
        }
        private bool _isOverwrite;

        public string TemplateFileName { get; set; }

        public string SavedFileName { get; set; }

        public bool IsCompleted =>
            !string.IsNullOrEmpty(OutPath) &&
            !string.IsNullOrEmpty(TemplateFileName) &&
            !string.IsNullOrEmpty(SavedFileName);

        public ProjectSettings(VariablesRepository variablesRepository)
        {
            _variablesRepository = variablesRepository;
            Width = 450;
            Height = 600;
            Title = "Новый проект";
            ShowTargetFile = false;
            OpenTargetFolder = true;
        }

        public void UpdateTitle()
        {
            Title = Title;
        }

        public void Update(StoredSettings settings)
        {
            Width = settings.Width;
            Height = settings.Height;
            Title = settings.Title;
            OutPath = settings.OutPath;
            TemplateFileName = settings.TemplateFileName;
            SavedFileName = settings.ResultFileName;
            ShowTargetFile = settings.ShowTargetFile;
            OpenTargetFolder = settings.OpenTargetFolder;
            IsOverwrite = settings.IsOverwrite;
        }

        public void Update(ProjectSettings settings)
        {
            Width = settings.Width;
            Height = settings.Height;
            Title = settings.Title;
            OutPath = settings.OutPath;
            TemplateFileName = settings.TemplateFileName;
            SavedFileName = settings.SavedFileName;
            ShowTargetFile = ShowTargetFile;
            OpenTargetFolder = OpenTargetFolder;
            IsOverwrite = settings.IsOverwrite;
        }
    }
}
