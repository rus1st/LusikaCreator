using System.ComponentModel;
using System.Runtime.Serialization;

namespace TestApp.Models.Enums
{
    [DataContract(Name = "ActionConsequenceType", Namespace = "")]
    public enum ActionOperation
    {
        [Description("Изменить значение переменной")]
        [EnumMember(Value = "setValue")]
        SetValue = 1,

        [Description("Изменить видимость объекта")]
        [EnumMember(Value = "setVisibility")]
        SetVisibility = 2,

        [Description("Изменить текст объекта")]
        [EnumMember(Value = "setObjectText")]
        SetObjectText = 3,

        [Description("Вызвать метод")]
        [EnumMember(Value = "callFunction")]
        CallFunction = 4
    }
}