using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ninject;
using TestApp.Repository;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;
using TestApp.ViewModels.Windows;
using TestApp.Views;
using TestApp.Views.Dialogs;
using TestApp.Views.Windows;

namespace TestApp.ViewModels
{
    public class ViewModelLocator : Locator
    {
        public ObjectsRepository ObjectsRepository => Container.Get<ObjectsRepository>();

        public ToolsPanelViewModel ToolsPanel => Container.Get<ToolsPanelViewModel>();

        public MessageDialogViewModel MessageDialog => Container.Get<MessageDialogViewModel>();

        public ObjectBrowserViewModel ObjectBrowser => Container.Get<ObjectBrowserViewModel>();


        /// <summary>
        /// Шаблонный метод для определения соответствий типов ViewModel и View
        /// </summary>
        /// <returns>Словарь соответствий</returns>
        protected override Dictionary<Type, Type> CreateMapping()
        {
            return new Dictionary<Type, Type>
            {
                {typeof (ToolsPanelViewModel), typeof (ToolsPanel)},
                {typeof (MessageDialogViewModel), typeof (MessageDialog)},
                {typeof (ObjectBrowserViewModel), typeof (ObjectBrowser)},
            };
        }

        /// <summary>
        /// Возвращает текущий ViewModelLocator для данного проекта 
        /// (заданный в файле глобальных настроек приложения App.xaml и имеющий идентификатор "Locator")
        /// </summary>
        public static ViewModelLocator Current
        {
            get
            {
                var locatorResourceDictionary =
                    Application.Current.Resources.MergedDictionaries.Single(
                        dict =>
                            dict.Keys.Count == 1 &&
                            dict.Keys.OfType<string>().Any(key => key.Equals("Locator")));

                return (ViewModelLocator)locatorResourceDictionary["Locator"];
            }
        }
    }
}