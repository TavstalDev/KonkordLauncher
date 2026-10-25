using System.Threading;
using System.Threading.Tasks;

namespace Tavstal.KonkordLauncher.Desktop.Services.Update;

public interface IUpdateService
{
    Task<bool> CheckAsync(CancellationToken cancellationToken = default);

    Task<bool> ApplyAsync(CancellationToken cancellationToken = default);
}
