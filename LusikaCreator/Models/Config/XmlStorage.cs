using System.Collections.Generic;
using System.Runtime.Serialization;
using TestApp.Models.Interfaces;
using TestApp.Repository;

namespace TestApp.Models.Config
{
    [DataContract(Name = "storage", Namespace = "")]
    public class XmlStorage
    {
        [DataMember(Name = "settings", Order = 0)]
        public StoredSettings Settings { get; set; }

        [DataMember(Name = "tabs", Order = 1)]
        public List<StoredTabItem> Tabs { get; set; } = new List<StoredTabItem>();

        [DataMember(Name = "objects", Order = 2)]
        public List<IFormObject> Objects { get; set; } = new List<IFormObject>();

        [DataMember(Name = "variables", Order = 3)]
        public List<IVariable> Variables { get; set; } = new List<IVariable>();
    }
}