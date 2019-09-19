namespace TestApp.Models.Interfaces
{
    public interface ITextual
    {
        string Text { get; set; }

        string FormattedText { get; set; }

        void UpdateFormattedText();
    }
}