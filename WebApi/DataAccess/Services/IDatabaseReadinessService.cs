namespace DataAccess.Services;

using System.Threading;
using System.Threading.Tasks;

using DataAccess.Transfers;

public interface IDatabaseReadinessService {
    Task<DatabaseReadiness> CheckReadinessAsync(CancellationToken cancellationToken);
}
