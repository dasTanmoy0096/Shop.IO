# Shop.IO Bootstrap SQL

Follow the [MariaDB local setup guide](../MARIADB_SETUP_GUIDE.md) before
running these scripts.

## Script Order

| Order | File | Purpose |
|---:|---|---|
| 1 | `00-create-root-tcp-account.sql` | Creates the local TCP root account. |
| 2 | `01-create-database.sql` | Creates the `shop_io` database. |
| 3 | `02-create-principals.sql` | Creates the local migration and runtime accounts. |

Run the files in numeric order with the local `root` account.

## Accounts

| Account | Host | Use |
|---|---|---|
| `shopio_migrator` | `127.0.0.1` | Schema, seed, migration, and verification scripts |
| `shopio_runtime` | `127.0.0.1` | Application runtime |

`shopio_runtime` has `SELECT`, `INSERT`, `UPDATE`, and `DELETE` permissions on
`shop_io`. `shopio_migrator` also has `CREATE`, `ALTER`, and `INDEX`
permissions.
