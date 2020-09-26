using System.Runtime.Serialization;

namespace TestApp.Models.Config
{
    [DataContract(Name = "font", Namespace = "")]
    public class StoredFontSettings
    {
        [DataMember(Name = "size", Order = 0)]
        public byte Size { get; set; }

        [DataMember(Name = "bold", Order = 1)]
        public bool Bold { get; set; }

        [DataMember(Name = "italic", Order = 2)]
        public bool Italic { get; set; }

        [DataMember(Name = "underlined", Order = 3)]
        public bool Underline { get; set; }

        [DataMember(Name = "color", Order = 4)]
        public string HexColor { get; set; }
    }
}