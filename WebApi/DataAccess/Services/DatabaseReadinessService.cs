namespace DataAccess.Services;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using DataAccess.Internals;
using DataAccess.Repositories;
using DataAccess.Transfers;

internal sealed class DatabaseReadinessService : IDatabaseReadinessService {
    private readonly DatabaseReadinessRepository databaseReadinessRepository;

    public DatabaseReadinessService(DatabaseReadinessRepository databaseReadinessRepository) {
        ArgumentNullException.ThrowIfNull(databaseReadinessRepository);

        this.databaseReadinessRepository = databaseReadinessRepository;
    }

    async Task<DatabaseReadiness> IDatabaseReadinessService.CheckReadinessAsync(CancellationToken cancellationToken) {
        try {
            bool isReady = await databaseReadinessRepository.CheckReadinessAsync(cancellationToken);

            return new DatabaseReadiness(IsReady: isReady);
        } catch (DataAccessDatabaseException) {
            return new DatabaseReadiness(IsReady: false);
        } catch (DataException) {
            return new DatabaseReadiness(IsReady: false);
        }
    }
}
