using System.Runtime.Serialization;
using TestApp.Models.Enums;

namespace TestApp.Models
{
    [DataContract(Name = "function", Namespace = "")]
    public class UserScript
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "description", Order = 1)]
        public string Description { get; set; }

        [DataMember(Name = "input", Order = 2)]
        public VariableType InputType { get; set; }

        [DataMember(Name = "output", Order = 3)]
        public VariableType OutputType { get; set; }

        [DataMember(Name = "javaScript", Order = 4)]
        public string Code { get; set; }

        public UserScript()
        {
        }
    }
}
