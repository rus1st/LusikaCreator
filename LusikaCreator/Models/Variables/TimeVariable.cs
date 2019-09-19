using System;
using System.Runtime.Serialization;
using TestApp.Models.Interfaces;

namespace TestApp.Models.Variables
{
    [DataContract(Name = "time", Namespace = "")]
    public class TimeVariable : IVariable
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "value", Order = 1)]
        public TimeSpan? Value { get; set; }

        [DataMember(Name = "now", Order = 2)]
        public bool UseCurrentTime { get; set; }

        [DataMember(Name = "seconds", Order = 3)]
        public bool UseSeconds { get; set; }

        [DataMember(Name = "format", Order = 4)]
        public string Format { get; set; }
    }
}