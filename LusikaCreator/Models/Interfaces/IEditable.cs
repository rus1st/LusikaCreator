using System.Collections.Generic;

namespace TestApp.Models.Interfaces
{
    public interface IEditable
    {
        IVariable Variable { get; set; }

        string VariableName { get; set; }

        List<ObjectAction> Actions { get; set; }
    }
}