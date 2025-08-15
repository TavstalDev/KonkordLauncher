using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models;

/// <summary>
/// Represents an abstract base class that extends the functionality of the ObservableObject
/// from the CommunityToolkit.Mvvm library. Provides a method to free memory resources.
/// </summary>
public abstract class KonkordObservableObject : ObservableObject
{
    /// <summary>
    /// Abstract method to free memory resources associated with the object.
    /// Must be implemented by derived classes.
    /// </summary>
    public abstract void FreeMemory();
}