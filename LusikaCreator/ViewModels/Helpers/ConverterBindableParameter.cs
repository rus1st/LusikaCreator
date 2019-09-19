using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace TestApp.ViewModels.Helpers
{
    /// <summary>
    ///     <example>
    ///         <TextBox>
    ///             <TextBox.Text>
    ///                 <wpfAdditions:ConverterBindableParameter Binding="{Binding FirstName}"
    ///                     Converter="{StaticResource TestValueConverter}"
    ///                     ConverterParameterBinding="{Binding ConcatSign}" />
    ///             </TextBox.Text>
    ///         </TextBox>
    ///     </example>
    /// </summary>
    public class ConverterBindableParameter : MarkupExtension
    {
        #region Public Properties

        public Binding Binding { get; set; }

        public IValueConverter Converter { get; set; }

        public Binding ConverterParameterBinding { get; set; }

        #endregion

        #region Overridden Methods

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(Binding);
            multiBinding.Bindings.Add(ConverterParameterBinding);
            var adapter = new MultiValueConverterAdapter
            {
                Converter = Converter
            };
            multiBinding.Converter = adapter;
            return multiBinding.ProvideValue(serviceProvider);
        }

        #endregion
    }
}