using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Data;
using TestApp.Models.Enums;
using TestApp.ViewModels.Interfaces;
using TestApp.Views.Converters;

namespace TestApp.Models
{
    public enum ActionTargetType
    {
        Variable,
        Object
    }

    [DataContract(Name = "action", Namespace = "")]
    public class ObjectAction
    {
        [DataMember(Name = "condition", Order = 0)]
        public ActionCondition Condition { get; set; }

        [DataMember(Name = "result", Order = 1)]
        public ActionResult Result { get; set; } = new ActionResult();

        public ActionTargetType TargetType => Result.Operation == ActionOperation.SetValue
            ? ActionTargetType.Variable
            : ActionTargetType.Object;

        public string Description => GetFriendlyName();

        public bool HasCondition => Condition != null;

        public IVariableWrapper Variable { get; set; }

        // Конструктор для сериализации
        public ObjectAction()
        {
        }

        public ObjectAction(IVariableWrapper variable)
        {
            Variable = variable;
        }

        public bool IsEqualTo(ObjectAction compared)
        {
            if (compared == null) return false;
            if (Description != compared.Description || Variable.Name != compared.Variable.Name) return false;

            if (Condition == null && compared.Condition != null) return false;
            if (Condition != null && compared.Condition == null) return false;
            if (Condition != null && !Condition.IsEqualTo(compared.Condition)) return false;

            if (Result == null || compared.Result == null)
            {
                // todo error
                return false;
            }

            return Result.IsEqualTo(compared.Result);
        }

        public void Update(ObjectAction buffer)
        {
            if (buffer == null) return;
            if (!buffer.HasCondition) Condition = null;
            else
            {
                if (Condition == null) Condition = new ActionCondition();
                Condition.Update(buffer.Condition);
            }

            if (buffer.Result == null) return;
            if (Result == null) Result = new ActionResult();
            Result.Update(buffer.Result);
        }

        public ObjectAction Clone()
        {
            return new ObjectAction
            {
                Condition = Condition.Clone(),
                Result = Result.Clone(),
                Variable = Variable
            };
        }

        private string GetFriendlyName()
        {
            var converter = new EnumToStringConverter();

            var condition = string.Empty;
            if (Condition != null)
            {
                var operand = Convert(converter, Condition.Operand);
                var valueString = string.Empty;
                if (Condition.Value != null)
                {
                    var value = Convert(converter, Condition.Value);
                    valueString = $" \"{value.ToString().ToLower()}\"";
                }
                condition = $"Если значение {operand.ToString().ToLower()}{valueString}, ";
            }

            var str2 = string.Empty;
            if (Result.Operation == ActionOperation.SetVisibility)
            {
                if (Result.Value != null && Result.Value.GetType() != typeof (bool))
                {
                    str2 = (ActionVisibilityOperand) Result.Value == ActionVisibilityOperand.Show
                        ? "Отобразить объект"
                        : "Скрыть объект";
                }
            }
            else
            {
                str2 = Common.GetEnumDescription(Result.Operation).Trim();
            }
            if (Condition != null) str2 = str2.ToLower();

            var str3 = string.Empty;
            if (Result.Operation == ActionOperation.SetValue || Result.Operation == ActionOperation.SetObjectText)
            {
                var actionResult = Convert(converter, Result.Value);
                str3 = $" на \"{actionResult}\"";
            }

            return $"{condition}{str2} \"{Result.TargetName}\"{str3}";
        }

        private static object Convert(IValueConverter converter, object value, Type type = null)
        {
            if (type == null) type = value.GetType();
            return converter.Convert(value, type, null, CultureInfo.CurrentCulture);
        }

    }
}