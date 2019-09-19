using System.Collections.Generic;
using System.IO;
using TestApp.Models;

namespace TestApp.ViewModels.Dialogs.Menu
{
    public class MenuItemProvider
    {
        public List<MenuItem> GetItems(string path)
        {
            var items = new List<MenuItem>();

            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists) return new List<MenuItem>();

            foreach (var directory in dirInfo.GetDirectories())
            {
                var item = new DirectoryItem
                {
                    Name = directory.Name,
                    Path = directory.FullName,
                    Items = GetItems(directory.FullName)
                };

                items.Add(item);
            }

            foreach (var file in dirInfo.GetFiles($"*.{Constants.ExtName}"))
            {
                var item = new FileItem
                {
                    Name = Path.GetFileNameWithoutExtension(file.Name),
                    Path = file.FullName
                };

                items.Add(item);
            }

            return items;
        }
    }
}