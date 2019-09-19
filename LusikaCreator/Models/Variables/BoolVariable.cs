using System.Runtime.Serialization;
using TestApp.Models.Enums;
using TestApp.Models.Interfaces;

namespace TestApp.Models.Variables
{
    [DataContract(Name = "bool", Namespace = "")]
    public class BoolVariable : IVariable
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "value", Order = 1)]
        public ActionSelectorOperand Value { get; set; }
    }
}