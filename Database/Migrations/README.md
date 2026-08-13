# Shop.IO Forward SQL Migrations

Use this directory for ordered database evolutions after the initial schema and
reference seed.

## Naming and Order

Name each forward migration `NNNN-lowercase-hyphen-slug.sql`, beginning at
`0001`. Execute files in ascending numeric order. A released migration's name
and contents are immutable; correct it with a new, higher-numbered migration.

Each migration has one paired read-only verifier at:

```text
Database/Verification/NNNN-verify-lowercase-hyphen-slug.sql
```

Add the exact migration command and its verifier command to section E of the
[MariaDB local setup guide](../MARIADB_SETUP_GUIDE.md) when adding the concrete
SQL files. Commands use `shopio_migrator`, `--skip-reconnect`, and never
`--force`.

Before `0001`, the initial-state verifier must pass. Before every later
migration, the preceding migration's verifier must pass. Run the new paired
verifier immediately after its migration.

## Required Migration Notes

At the start of every migration, document:

1. Prerequisites and the expected preceding database state.
2. Whether a repeat is a deliberately safe no-op or a deliberate failure.
3. The paired verifier that must pass after execution.
4. Recovery: a compensating forward migration, compatibility release, or
   restore from a verified backup.

Do not use broad `IF EXISTS`, `IF NOT EXISTS`, `INSERT IGNORE`, or upserts to
hide unexpected drift. Stop after an error, inspect the actual state, and use
the documented recovery path.

## Execution Rules

Apply a migration only with the required explicit authorization and only with
the `shopio_migrator` account. Run the preceding verifier first, then the
migration, then its paired verifier. The web application and `shopio_runtime`
account never apply migrations. Do not create down scripts.

MariaDB DDL can commit implicitly. Do not claim that a multi-statement DDL file
is one rollbackable transaction. A local transaction is appropriate only for a
DML-only portion whose statements are transactional.

## Official Documentation

- [MariaDB client options](https://mariadb.com/docs/server/clients-and-utilities/mariadb-client/mariadb-command-line-client)
- [Statements that cause an implicit commit](https://mariadb.com/docs/server/reference/sql-statements/transactions/sql-statements-that-cause-an-implicit-commit)
- [CREATE TABLE](https://mariadb.com/docs/server/reference/sql-statements/data-definition/create/create-table)
- [INSERT IGNORE](https://mariadb.com/docs/server/reference/sql-statements/data-manipulation/inserting-loading-data/insert-ignore)
- [INSERT ... ON DUPLICATE KEY UPDATE](https://mariadb.com/docs/server/reference/sql-statements/data-manipulation/inserting-loading-data/insert-on-duplicate-key-update)
