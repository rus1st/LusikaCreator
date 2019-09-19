using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Models.Interfaces;

namespace TestApp.ViewModels.Interfaces
{
    public interface IVariableWrapper
    {
        event Handlers.EmptyHandler ValueChanged;

        string Name { get; set; }

        VariableType Type { get; }

        bool IsAssigned { get; set; }

        string StringValue { get; set; }

        void Set(object value);

        bool IsEqualTo(IVariableWrapper compared);

        void Update(IVariableWrapper buffer);

        IVariableWrapper Clone();

        void SubscribeToUpdates();

        void UnSubscribeToUpdates();

        string ToString();

        IVariable ToStoredObject();
    }
}