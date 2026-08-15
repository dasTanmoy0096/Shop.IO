namespace DataAccess.Internals;

internal enum DbVersionedMutationOutcome {
    Applied,
    NotFoundOrVersionMismatch,
}
