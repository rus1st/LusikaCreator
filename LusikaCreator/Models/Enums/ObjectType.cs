using System.ComponentModel;
using System.Runtime.Serialization;

namespace TestApp.Models.Enums
{
    [DataContract(Name = "ObjectType")]
    public enum ObjectType
    {
        [Description("Формат не определен")]
        [EnumMember(Value = "unknown")]
        Unknown = 0,

        [Description("Простой текст")]
        [EnumMember(Value = "label")]
        Label = 1,

        [Description("Поле для ввода")]
        [EnumMember(Value = "textbox")]
        TextBox = 2,

        [Description("Флажок")]
        [EnumMember(Value = "checkbox")]
        CheckBox = 3,

        [Description("Переключатель")]
        [EnumMember(Value = "radio")]
        RadioButton = 4,

        [Description("Выпадающий список")]
        [EnumMember(Value = "dropdown")]
        ComboBox = 5,

        [Description("Дата")]
        [EnumMember(Value = "date")]
        DatePicker = 6,

        [Description("Время")]
        [EnumMember(Value = "time")]
        TimePicker = 7
    }
}