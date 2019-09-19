using System;
using System.Runtime.Serialization;
using TestApp.Models.Interfaces;

namespace TestApp.Models.Variables
{
    [DataContract(Name = "date", Namespace = "")]
    public class DateVariable : IVariable
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "value", Order = 1)]
        public DateTime? Value { get; set; }

        [DataMember(Name = "now", Order = 2)]
        public bool UseCurrentDate { get; set; }

        [DataMember(Name = "format", Order = 3)]
        public string Format { get; set; }
    }
}