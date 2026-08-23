namespace WebApi.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;

using DataAccess.Services;
using DataAccess.Transfers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using WebApi.Contracts;

// TEMPORARY: Remove with the P3.07 readiness demonstration when P7 defines the real health endpoint.
[Controller]
[Route("api/v1/_temporary/database-readiness")]
public sealed class TemporaryDatabaseReadinessController {
    private readonly IDatabaseReadinessService databaseReadinessService;

    public TemporaryDatabaseReadinessController(IDatabaseReadinessService databaseReadinessService) {
        ArgumentNullException.ThrowIfNull(databaseReadinessService);

        this.databaseReadinessService = databaseReadinessService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAsync(CancellationToken cancellationToken) {
        DatabaseReadiness readiness = await databaseReadinessService.CheckReadinessAsync(cancellationToken);
        int statusCode = readiness.IsReady
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return new ObjectResult(
            new TemporaryDatabaseReadinessResponse(readiness.IsReady)
        ) {
            StatusCode = statusCode,
        };
    }
}
