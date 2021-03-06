﻿using System.Runtime.Serialization;
using TestApp.Models.Config;
using TestApp.Models.Interfaces;

namespace TestApp.Models.FormObjects
{
    [DataContract(Name = "label", Namespace = "")]
    public class LabelObject : IFormObject
    {
        [DataMember(Name = "id", Order = 0)]
        public uint Id { get; set; }

        [DataMember(Name = "name", Order = 1)]
        public string Name { get; set; }

        [DataMember(Name = "left", Order = 2)]
        public int Left { get; set; }

        [DataMember(Name = "top", Order = 3)]
        public int Top { get; set; }

        [DataMember(Name = "text", Order = 4)]
        public string Text { get; set; }

        [DataMember(Name = "visible", Order = 5)]
        public bool IsVisible { get; set; }

        [DataMember(Name = "required", Order = 6)]
        public bool IsRequired { get; set; }

        [DataMember(Name = "font", Order = 7)]
        public StoredFontSettings FontSettings { get; set; }

        [DataMember(Name = "tab", Order = 8)]
        public byte TabId { get; set; }
    }
}