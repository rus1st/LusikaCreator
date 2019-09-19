using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using MaterialDesignThemes.Wpf;
using Ninject;
using Ninject.Modules;

namespace TestApp.ViewModels.Helpers
{
    public abstract class Locator
    {
        /// <summary>
        /// Соответствия типов моделей-представлений и представлений
        /// </summary>
        protected Dictionary<Type, Type> Mapping;

        /// <summary>
        /// DI-контейнер для создания и передачи экземпляров ViewModel
        /// </summary>
        public IKernel Container { get; set; }

        protected Locator()
        {
            Container = new StandardKernel();
            Mapping = CreateMapping();
        }

        protected Locator(INinjectModule ninjectModule)
        {
            Container = new StandardKernel(ninjectModule);
            Mapping = CreateMapping();
        }

        /// <summary>
        /// Создает и отображает новое представление
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewModel"></param>
        /// <param name="needOwner"></param>
        public Window Show<T>(T viewModel = null, bool needOwner = true) where T : ViewModelBase
        {
            var viewModelIsRegistered = Container.CanResolve<T>();
            var viewType = Mapping[typeof (T)];
            Window view;

            if (viewModelIsRegistered)
            {
                view = CreateAndShowView<T>(viewType, needOwner);
            }
            else if (viewModel != null)
            {
                Container.Bind<T>().ToMethod<T>(context => viewModel);
                view = CreateAndShowView<T>(viewType, needOwner);
                Container.Unbind<T>();
            }
            else
            {
                throw new NullReferenceException("В качестве аргумента должен быть передан ViewModel открываемого окна");
            }
            return view;
        }

        /// <summary>
        /// Создает и отображает новое представление
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dialogHostIdentifier">Идентификатор хоста для отображения диалога</param>
        /// <param name="closingEventHandler">Выполняемая функция при закрытии диалога</param>
        /// <param name="viewModel">Модель-представление диалога</param>
        public void ShowDialog<T>(string dialogHostIdentifier,
            DialogClosingEventHandler closingEventHandler, T viewModel = null) where T : ViewModelBase
        {
            var viewModelIsRegistered = Container.CanResolve<T>();
            var viewType = Mapping[typeof (T)];

            if (viewModelIsRegistered)
            {
                CreateAndShowDialog(viewType, dialogHostIdentifier, closingEventHandler);
            }
            else if (viewModel != null)
            {
                Container.Bind<T>().ToMethod<T>(context => viewModel);
                CreateAndShowDialog(viewType, dialogHostIdentifier, closingEventHandler);
                Container.Unbind<T>();
            }
            else
            {
                throw new NullReferenceException("В качестве аргумента должен быть передан ViewModel открываемого окна");
            }
        }

        /// <summary>
        /// Асинхронно создает и отображает новое представление
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dialogHostIdentifier">Идентификатор хоста для отображения диалога</param>
        /// <param name="closingEventHandler">Выполняемая функция при закрытии диалога</param>
        /// <param name="viewModel">Модель-представление диалога</param>
        public async Task ShowDialogAsync<T>(string dialogHostIdentifier,
            DialogClosingEventHandler closingEventHandler, T viewModel = null) where T : ViewModelBase
        {
            var viewModelIsRegistered = Container.CanResolve<T>();
            var viewType = Mapping[typeof (T)];

            if (viewModelIsRegistered)
            {
                await CreateAndShowDialog(viewType, dialogHostIdentifier, closingEventHandler);
            }
            else if (viewModel != null)
            {
                Container.Bind<T>().ToMethod<T>(context => viewModel);
                await CreateAndShowDialog(viewType, dialogHostIdentifier, closingEventHandler);
                Container.Unbind<T>();
            }
            else
            {
                throw new NullReferenceException("В качестве аргумента должен быть передан ViewModel открываемого окна");
            }
        }

        private Window CreateAndShowView<T>(Type viewType, bool needOwner) where T : ViewModelBase
        {
            var view = (Window) Activator.CreateInstance(viewType);

            if (needOwner && !Equals(view, Application.Current.MainWindow))
                view.Owner = Application.Current.MainWindow;

            view.Show();
            return view;
        }

        private async Task CreateAndShowDialog(Type dialogType, string dialogHostIdentifier,
            DialogClosingEventHandler closingEventHandler)
        {
            var view = (UserControl) Activator.CreateInstance(dialogType);
            await DialogHost.Show(view, dialogHostIdentifier, closingEventHandler);
        }

        /// <summary>
        /// Шаблонный метод для переопределения соответствий типов ViewModel и View
        /// </summary>
        /// <returns>Словарь соответствий</returns>
        protected abstract Dictionary<Type, Type> CreateMapping();
    }
}