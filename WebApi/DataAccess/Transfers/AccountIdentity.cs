namespace DataAccess.Transfers;

using System;

public sealed record AccountIdentity(
    Guid PublicId,
    string Username
);
