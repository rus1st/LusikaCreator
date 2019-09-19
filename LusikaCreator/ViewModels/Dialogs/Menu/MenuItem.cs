using System.Collections.Generic;

namespace TestApp.ViewModels.Dialogs.Menu
{
    public class MenuItem
    {
        public string Name { get; set; }

        public string Path { get; set; }
    }

    public class FileItem : MenuItem
    {
    }

    public class DirectoryItem : MenuItem
    {
        public List<MenuItem> Items { get; set; }

        public DirectoryItem()
        {
            Items = new List<MenuItem>();
        }
    }
}