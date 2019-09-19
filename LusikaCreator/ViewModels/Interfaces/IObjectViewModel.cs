using TestApp.Models.Interfaces;
using TestApp.ViewModels.ObjectProperties;

namespace TestApp.ViewModels.Interfaces
{
    public interface IObjectViewModel
    {
        ObjectBaseProperties Properties { get; set; }

        void Update(IObjectViewModel buffer);

        IObjectViewModel Clone();

        IFormObject ToStoredObject();
    }
}