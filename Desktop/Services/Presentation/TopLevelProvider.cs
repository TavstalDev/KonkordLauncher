using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation;

/// <inheritdoc />
public class TopLevelProvider : ITopLevelProvider
{
    private TopLevel? _topLevel;

    /// <inheritdoc />
    public TopLevel? Get()
    {
        return _topLevel;
    }

    /// <inheritdoc />
    public void SetActive(TopLevel? topLevel)
    {
        _topLevel = topLevel;
    }

    /// <inheritdoc />
    public void ClearIfMatches(TopLevel? topLevel)
    {
        if (topLevel != _topLevel)
            return;
        _topLevel = null;
    }
}
