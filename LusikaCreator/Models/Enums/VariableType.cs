using System.ComponentModel;
using System.Runtime.Serialization;

namespace TestApp.Models.Enums
{
    [DataContract(Name = "VariableType")]
    public enum VariableType
    {
        [Description("Тип не определен")]
        [EnumMember(Value = "unknown")]
        Unknown,

        [Description("Логическая переменная")]
        [EnumMember(Value = "bool")]
        Bool,

        [Description("Строка")]
        [EnumMember(Value = "string")]
        String,

        [Description("Дата")]
        [EnumMember(Value = "date")]
        Date,

        [Description("Время")]
        [EnumMember(Value = "time")]
        Time,

        [Description("Любой тип")]
        [EnumMember(Value = "any")]
        Any
    }
}