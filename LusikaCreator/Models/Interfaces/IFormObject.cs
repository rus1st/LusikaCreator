using TestApp.Models.Config;

namespace TestApp.Models.Interfaces
{
    public interface IFormObject
    {
        uint Id { get; set; }

        byte TabId { get; set; }

        string Name { get; set; }

        int Left { get; set; }

        int Top { get; set; }

        bool IsVisible { get; set; }

        StoredFontSettings FontSettings { get; set; }
    }
}