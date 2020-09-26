using System.Collections.Generic;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.Config;
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

        /// <summary>
        /// Создание нового объекта
        /// </summary>
        public LabelViewModel(uint id, string name, DataProvider dataProvider)
        {
            if (string.IsNullOrEmpty(name)) return;

            Properties = new ObjectBaseProperties(id, name, dataProvider.CommonSettings.AppMode, dataProvider.ObjectsRepository);
            TextProperties = new ObjectTextProperties(dataProvider.VariablesRepository);
        }

        /// <summary>
        /// Восстановление объекта из xml
        /// </summary>
        public LabelViewModel(LabelObject storedObj, DataProvider dataProvider)
        {
            if (storedObj == null || dataProvider?.CommonSettings == null) return;

            Properties = new ObjectBaseProperties(storedObj.Id, storedObj.Name, dataProvider.CommonSettings.AppMode, dataProvider.ObjectsRepository);
            Properties.FontSettings.Update(storedObj.FontSettings);
            TextProperties = new ObjectTextProperties(dataProvider.VariablesRepository)
            {
                Text = storedObj.Text
            };
        }

        public Brush Color => Properties.GetVisibility() ? Properties.FontSettings?.Color
            ?? Brushes.Black : Brushes.LightGray;

        public void Update(IObjectViewModel buffer)
        {
            if (!(buffer is LabelViewModel)) return;
            var viewModel = (LabelViewModel)buffer;

            Properties.Update(viewModel.Properties);
            TextProperties.Update(viewModel.TextProperties);
            RaisePropertyChanged(nameof(Color));
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