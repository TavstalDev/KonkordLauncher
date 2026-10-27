using Avalonia.Controls;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

public interface ITopLevelProvider
{
    TopLevel? Get();

    void SetActive(TopLevel? topLevel);

    void ClearIfMatches(TopLevel? topLevel);
}
