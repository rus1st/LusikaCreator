using System.Runtime.Serialization;

namespace TestApp.Models.Config
{
    [DataContract(Name = "font", Namespace = "")]
    public class StoredFontSettings
    {
        [DataMember(Name = "size", Order = 0)]
        public byte Size { get; set; }

        [DataMember(Name = "bold", Order = 1)]
        public bool IsBold { get; set; }

        [DataMember(Name = "italic", Order = 2)]
        public bool IsItalic { get; set; }

        [DataMember(Name = "underlined", Order = 3)]
        public bool IsUnderlined { get; set; }

        [DataMember(Name = "color", Order = 4)]
        public string HexColor { get; set; }
    }
}