using System.Runtime.Serialization;
using TestApp.Models.Interfaces;

namespace TestApp.Models.Variables
{
    [DataContract(Name = "string", Namespace = "")]
    public class StringVariable : IVariable
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "value", Order = 1)]
        public string Value { get; set; }
    }
}