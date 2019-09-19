namespace TestApp.ViewModels.Interfaces
{
    public interface IRequired
    {
        bool IsRequired { get; set; }

        bool IsComplete { get; }

        /// <summary>
        /// Обновляет значение, чтобы при переключении между режимами пропадала/появлялась надпись ValidationRules'а
        /// </summary>
        void Refresh();
    }
}