namespace KonkordLibrary.Models
{
    public interface IProgressWindow
    {
        void UpdateProgressBar(double percent, string text);

        void UpdateProgressBarTranslated(double percent, string text, params object[]? args);

        void ShowWindow();

        bool GetIsVisible();
    }
}
