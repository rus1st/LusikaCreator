using TestApp.ViewModels.Interfaces;

namespace TestApp.Models
{
    public class Handlers
    {
        public delegate void EmptyHandler();

        public delegate void ObjectChangedHandler(IObjectViewModel viewModel);

        public delegate void ObjectModifiedHandler(uint id);

        public delegate void VariableChangedHandler(IVariableWrapper variable, string oldName = null);

        public delegate void VariableRemovedHandler(string variableName);
    }
}