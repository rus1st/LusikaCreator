using System.Activities.Statements;
using System.Collections.Generic;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.FormObjects;
using TestApp.Models.Interfaces;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.ObjectProperties;

namespace TestApp.ViewModels.Controls
{
    public class LabelViewModel : ViewModelBase, IObjectViewModel, ITextProperties
    {
        public ObjectBaseProperties Properties { get; set; }

        public ObjectTextProperties TextProperties { get; set; }

        public List<IVariableWrapper> RelatedVariables { get; set; } = new List<IVariableWrapper>();


        public LabelViewModel()
        {
        }

        public LabelViewModel(uint id, string name,
            DataProvider dataProvider)
        {
            Properties = new ObjectBaseProperties(id, name, dataProvider.CommonSettings.AppMode, dataProvider.ObjectsRepository);
            TextProperties = new ObjectTextProperties(dataProvider.VariablesRepository);
        }

        public Brush Color
            => Properties.GetVisibility() ? Properties.FontSettings?.Color ?? Brushes.Black : Brushes.LightGray;

        public void Update(IObjectViewModel buffer)
        {
            if (!(buffer is LabelViewModel)) return;
            var viewModel = (LabelViewModel)buffer;

            Properties.Update(viewModel.Properties);
            TextProperties.Update(viewModel.TextProperties);
            RaisePropertyChanged("Color");
        }

        public IObjectViewModel Clone()
        {
            return new LabelViewModel
            {
                Properties = Properties.Clone(),
                TextProperties = TextProperties.Clone()
            };
        }

        public IFormObject ToStoredObject()
        {
            return new LabelObject
            {
                Id = Properties.Id,
                Name = Properties.Name,
                Left = Properties.Left,
                Top = Properties.Top,
                Text = TextProperties.Text,
                IsVisible = Properties.GetVisibility(),
                FontSettings = Properties.FontSettings?.ToStoredObject(),
                TabId = Properties.TabId
            };
        }
    }
}