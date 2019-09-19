using System.Runtime.Serialization;
using TestApp.Models.Enums;

namespace TestApp.Models
{
    [DataContract(Name = "result", Namespace = "")]
    public class ActionResult
    {
        [DataMember(Name = "operation", Order = 0)]
        public ActionOperation Operation { get; set; }

        [DataMember(Name = "targetName", Order = 1)]
        public string TargetName { get; set; }

        [DataMember(Name = "value", Order = 3)]
        public object Value { get; set; }

        public ActionResult()
        {
            TargetName = null;
            Value = null;
        }

        public bool IsEqualTo(ActionResult compared)
        {
            return compared != null &&
                   Operation == compared.Operation && TargetName == compared.TargetName && Value == compared.Value;
        }

        public void Update(ActionResult buffer)
        {
            if (buffer == null) return;

            Operation = buffer.Operation;
            TargetName = buffer.TargetName;
            Value = buffer.Value;
        }

        public ActionResult Clone()
        {
            return new ActionResult
            {
                Operation = Operation,
                TargetName = TargetName,
                Value = Value
            };
        }
    }
}