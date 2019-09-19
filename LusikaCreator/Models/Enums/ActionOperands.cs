using System.ComponentModel;
using System.Runtime.Serialization;

namespace TestApp.Models.Enums
{
    [DataContract(Name = "ActionInputOperand")]
    public enum ActionInputOperand
    {
        [Description("Задано")]
        [EnumMember(Value = "Set")]
        Set = 1,

        [Description("Не задано")]
        [EnumMember(Value = "notSet")]
        NotSet = 2,

        [Description("Равно")]
        [EnumMember(Value = "equal")]
        Equal = 3,

        [Description("Не равно")]
        [EnumMember(Value = "notEqual")]
        NotEqual = 4,

        [Description("Содержит")]
        [EnumMember(Value = "contains")]
        Contains = 5
    }

    [DataContract(Name = "ActionSelectorOperand", Namespace = "")]
    public enum ActionSelectorOperand
    {
        [Description("Выбрано")]
        [EnumMember(Value = "set")]
        Set = 1,

        [Description("Не выбрано")]
        [EnumMember(Value = "notSet")]
        NotSet = 2
    }

    [DataContract(Name = "ActionDateOperand")]
    public enum ActionDateOperand
    {
        [Description("Не задано")]
        [EnumMember(Value = "notSet")]
        NotSet = 1,

        [Description("Сегодняшняя дата")]
        [EnumMember(Value = "isYeaterday")]
        IsYeaterday = 2,

        [Description("Раньше даты")]
        [EnumMember(Value = "earlier")]
        Earlier = 3,

        [Description("Позже даты")]
        [EnumMember(Value = "later")]
        Later = 4
    }

    [DataContract(Name = "ActionTimeOperand")]
    public enum ActionTimeOperand
    {
        [Description("Не задано")]
        [EnumMember(Value = "notSet")]
        NotSet = 1,

        [Description("Задано")]
        [EnumMember(Value = "isSet")]
        IsSet = 2,

        [Description("Меньше")]
        [EnumMember(Value = "less")]
        Less = 3,

        [Description("Больше")]
        [EnumMember(Value = "more")]
        More = 4
    }

    [DataContract(Name = "ActionVisibilityOperand")]
    public enum ActionVisibilityOperand
    {
        [Description("Показать")]
        [EnumMember(Value = "show")]
        Show = 1,

        [Description("Скрыть")]
        [EnumMember(Value = "hide")]
        Hide = 2
    }
}