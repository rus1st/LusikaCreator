using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TestApp.Models.Enums;

namespace TestApp.Models
{
    [XmlRoot("condition")]
    public class ActionCondition
    {
        /// <summary>
        /// Типы условий для работы с переменными
        /// </summary>
        private readonly List<Type> _operationsWithVariable = new List<Type>
        {
            typeof (ActionInputOperand),
            typeof (ActionSelectorOperand),
            typeof (ActionDateOperand),
            typeof (ActionTimeOperand)
        };

        /// <summary>
        /// Типы условий для работы с объектами
        /// </summary>
        private readonly List<Type> _operationsWithObject = new List<Type>
        {
            typeof (ActionVisibilityOperand)
        };

        [XmlElement("operand")]
        public object Operand { get; set; }

        [XmlElement("value")]
        public object Value { get; set; }

        /// <summary>
        /// Определяет, применяется ли условие к переменной
        /// </summary>
        public bool IsVariableTarget => Operand != null && _operationsWithVariable.Any(t => t == Operand.GetType());

        /// <summary>
        /// Определяет, применяется ли условие к объекту
        /// </summary>
        public bool IsObjectTarget => Operand != null && _operationsWithObject.Any(t => t == Operand.GetType());


        // Конструктор для сериализации
        public ActionCondition()
        {
        }

        public bool IsEqualTo(ActionCondition compared)
        {
            return Operand.ToString() == compared.Operand.ToString() && Value == compared.Value;
        }

        public void Update(ActionCondition buffer)
        {
            if (buffer == null) return;
            Operand = buffer.Operand;
            Value = buffer.Value;
        }

        public ActionCondition Clone()
        {
            return new ActionCondition {Operand = Operand, Value = Value};
        }
    }
}