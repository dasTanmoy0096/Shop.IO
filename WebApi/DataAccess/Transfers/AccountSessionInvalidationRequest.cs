namespace DataAccess.Transfers;

using System;

public sealed record AccountSessionInvalidationRequest(Guid AccountPublicId);
