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

        public void RemoveAction(ObjectAction action)
        {
        }

        public void UpdateActions(List<ObjectAction> actions)
        {
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