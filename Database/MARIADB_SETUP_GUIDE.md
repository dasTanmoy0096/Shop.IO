# Shop.IO MariaDB Local Setup Guide

This guide creates one local MariaDB instance for Shop.IO. Run every command
from the solution directory containing `ShopIO.slnx`. The instance directory is
`../Database`, a sibling of the solution directory.

## Quick Reference: Official Documentation

For complete command options and configuration details, see the MariaDB
documentation:

1. [Windows `mariadb-install-db` options](https://mariadb.com/docs/server/server-management/install-and-upgrade-mariadb/installing-mariadb/installing-system-tables-mariadb-install-db/mariadb-install-db-exe)
2. [Unix `mariadb-install-db` options](https://mariadb.com/docs/server/clients-and-utilities/deployment-tools/mariadb-install-db)
3. [`mariadbd` options](https://mariadb.com/docs/server/server-management/starting-and-stopping-mariadb/mariadbd-options)
4. [`mariadb` client options](https://mariadb.com/docs/server/clients-and-utilities/mariadb-client/mariadb-command-line-client)
5. [`mariadb-admin` options](https://mariadb.com/docs/server/clients-and-utilities/administrative-tools/mariadb-admin)
6. [MariaDB option files](https://mariadb.com/docs/server/server-management/install-and-upgrade-mariadb/configuring-mariadb/configuring-mariadb-with-option-files)
7. [Server system variables](https://mariadb.com/docs/server/server-management/variables-and-modes/server-system-variables)
8. [CREATE DATABASE](https://mariadb.com/docs/server/reference/sql-statements/data-definition/create/create-database), [CREATE USER](https://mariadb.com/docs/server/reference/sql-statements/account-management-sql-statements/create-user), [ALTER USER](https://mariadb.com/docs/server/reference/sql-statements/account-management-sql-statements/alter-user), and [GRANT](https://mariadb.com/docs/server/reference/sql-statements/account-management-sql-statements/grant)
9. [CREATE TABLE](https://mariadb.com/docs/server/reference/sql-statements/data-definition/create/create-table), [constraints](https://mariadb.com/docs/server/reference/sql-statements/data-definition/constraint), and [foreign keys](https://mariadb.com/docs/server/architecture/server-constraints/foreign-key-constraints)
10. [`START TRANSACTION` and `COMMIT`](https://mariadb.com/docs/server/reference/sql-statements/transactions/start-transaction)

## Local Development Credentials

| Account | Password | Purpose |
|---|---|---|
| `root` | `shopio_local_root` | Server administration and bootstrap scripts |
| `shopio_migrator` | `shopio_local_migrator` | Schema, seed, migration, and verification scripts |
| `shopio_runtime` | `shopio_local_runtime` | Application runtime |

These values are for the local development instance only.

## A. Create the Server Instance

Create the `../Database` folder.

PowerShell Core:

```powershell
New-Item -ItemType Directory -Path "../Database"
```

Bash/zsh:

```bash
mkdir -p "../Database"
```

Initialize MariaDB in that folder.

PowerShell Core on Windows:

```powershell
mariadb-install-db --datadir="../Database" --port=5024 --password=shopio_local_root
```

Bash/zsh on Unix-like systems:

```bash
mariadb-install-db --no-defaults --skip-test-db --auth-root-authentication-method=socket --datadir="../Database"
```

The Windows initializer creates `../Database/my.ini`. The Unix initializer
does not create an option file.

## B. Configure `my.ini`

On Windows, open the generated `../Database/my.ini`. Keep its generated
`datadir` value and ensure the server section contains these values:

```ini
bind-address=localhost
port=5024
autocommit=1
default_time_zone=+00:00
```

On Unix-like systems, create `../Database/my.ini` with this content:

```ini
[mariadbd]
datadir=../Database
bind-address=localhost
port=5024
autocommit=1
default_time_zone=+00:00
```

Do not add `pid-file` or `log-error` entries.

## C. Start the Server

Start MariaDB and leave this terminal open.

PowerShell Core on Windows:

```powershell
mariadbd --defaults-file="../Database/my.ini" --console
```

Bash/zsh on Unix-like systems:

```bash
mariadbd --defaults-file="../Database/my.ini"
```

## D. Check the Server

Open a second terminal and check that MariaDB is running.

PowerShell Core on Windows:

```powershell
mariadb-admin --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root ping
```

Bash/zsh on Unix-like systems:

```bash
sudo mariadb-admin --defaults-file="../Database/my.ini" --user=root ping
```

## E. Execute Bootstrap, Schema, Seed, Migration, and Verification Scripts

Run the following SQL files in this order.

### 1. Create the Local TCP Root Account

PowerShell Core on Windows:

```powershell
Get-Content "./Database/Bootstrap/00-create-root-tcp-account.sql" | mariadb --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root
```

Bash/zsh on Unix-like systems:

```bash
sudo mariadb --defaults-file="../Database/my.ini" --user=root < "./Database/Bootstrap/00-create-root-tcp-account.sql"
```

### 2. Create the `shop_io` Database

PowerShell Core:

```powershell
Get-Content "./Database/Bootstrap/01-create-database.sql" | mariadb --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root
```

Bash/zsh:

```bash
mariadb --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root < "./Database/Bootstrap/01-create-database.sql"
```

### 3. Create the Migration and Runtime Accounts

PowerShell Core:

```powershell
Get-Content "./Database/Bootstrap/02-create-principals.sql" | mariadb --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root
```

Bash/zsh:

```bash
mariadb --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root < "./Database/Bootstrap/02-create-principals.sql"
```

### 4. Create the Initial Schema

PowerShell Core:

```powershell
Get-Content "./Database/Schema/01-initial-schema.sql" | mariadb --host=127.0.0.1 --port=5024 --user=shopio_migrator --password=shopio_local_migrator --database=shop_io
```

Bash/zsh:

```bash
mariadb --host=127.0.0.1 --port=5024 --user=shopio_migrator --password=shopio_local_migrator --database=shop_io < "./Database/Schema/01-initial-schema.sql"
```

### 5. Seed Reference Data

PowerShell Core:

```powershell
Get-Content "./Database/Seed/01-reference-data.sql" | mariadb --host=127.0.0.1 --port=5024 --user=shopio_migrator --password=shopio_local_migrator --database=shop_io
```

Bash/zsh:

```bash
mariadb --host=127.0.0.1 --port=5024 --user=shopio_migrator --password=shopio_local_migrator --database=shop_io < "./Database/Seed/01-reference-data.sql"
```

Verify the database and grants.

```text
mariadb --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root --execute="SHOW DATABASES LIKE 'shop_io'; SHOW GRANTS FOR 'shopio_migrator'@'127.0.0.1'; SHOW GRANTS FOR 'shopio_runtime'@'127.0.0.1';"
```

Use `shopio_migrator` for schema, seed, migration, and verification scripts.
Do not use `shopio_runtime` to execute SQL scripts.

## F. Stop the Server

Stop MariaDB cleanly from a second terminal.

```text
mariadb-admin --host=127.0.0.1 --port=5024 --user=root --password=shopio_local_root shutdown
```

The terminal running `mariadbd` closes after the shutdown succeeds.
