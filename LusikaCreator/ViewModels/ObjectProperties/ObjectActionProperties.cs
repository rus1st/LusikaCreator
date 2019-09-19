using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Repository;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.ObjectProperties
{
    public class ObjectActionProperties : ViewModelBase
    {
        private readonly VariablesRepository _variablesRepository;
        private readonly ObjectsRepository _objectsRepository;

        public IVariableWrapper Variable
        {
            get { return _variable; }
            set
            {
                if (_variable == value) return;
                Set(ref _variable, value);
            }
        }
        private IVariableWrapper _variable;

        public bool HasActions => Actions.Count > 0;

        public List<ObjectAction> Actions { get; private set; }


        public ObjectActionProperties(IVariableWrapper variable, VariablesRepository variablesRepository, ObjectsRepository objectsRepository)
        {
            _variablesRepository = variablesRepository;
            _objectsRepository = objectsRepository;
            Variable = variable;
            Actions = new List<ObjectAction>();
        }

        //public void AddAction(ObjectAction action)
        //{
        //    if (action.Result == null) return;

        //    if (action.TargetType == ActionTargetType.Variable)
        //    {
        //        var variableName = action.Result.TargetName;
        //        var targetVariable = _variablesRepository.Find(variableName);
        //        if (variableName == null) return;

        //        //Variable.DependedVariables.Add(targetVariable);
        //        //targetVariable.RelatedVariables.Add(Variable);
        //    }
        //    else if (action.TargetType == ActionTargetType.Object)
        //    {
        //        var objectName = action.Result.TargetName;
        //        var targetObject = _objectsRepository.Find(objectName);
        //        if (targetObject == null) return;

        //        //Variable.DependedObjects.Add(targetObject);
        //        //targetObject.RelatedVariables.Add(Variable);
        //    }
        //}

        public void RemoveAction(ObjectAction action)
        {
            
        }

        public void UpdateActions(List<ObjectAction> actions)
        {
            //Variable.DependedVariables.Clear();
            //Variable.DependedObjects.Clear();
            //actions.ForEach(AddAction);
            //Actions = actions;

            Actions.Clear();
            foreach (var action in actions)
            {
                var addedAction = action;
                addedAction.Variable = Variable;
                Actions.Add(addedAction);
            }
        }

        public ObjectActionProperties Clone()
        {
            return new ObjectActionProperties(Variable.Clone(), _variablesRepository, _objectsRepository)
            {
                Actions = Actions.ToList()
            };
        }

        public void Update(ObjectActionProperties buffer)
        {
            Variable.Update(buffer.Variable);
            Actions = buffer.Actions.ToList();
        }
    }
}