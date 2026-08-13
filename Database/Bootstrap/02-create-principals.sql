-- Run after 01-create-database.sql.

CREATE USER IF NOT EXISTS 'shopio_migrator'@'127.0.0.1' ACCOUNT LOCK;
CREATE USER IF NOT EXISTS 'shopio_runtime'@'127.0.0.1' ACCOUNT LOCK;

REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'shopio_migrator'@'127.0.0.1';
REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'shopio_runtime'@'127.0.0.1';

GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX
    ON `shop_io`.* TO 'shopio_migrator'@'127.0.0.1';

GRANT SELECT, INSERT, UPDATE, DELETE
    ON `shop_io`.* TO 'shopio_runtime'@'127.0.0.1';

ALTER USER 'shopio_migrator'@'127.0.0.1'
    IDENTIFIED BY 'shopio_local_migrator'
    ACCOUNT UNLOCK;

ALTER USER 'shopio_runtime'@'127.0.0.1'
    IDENTIFIED BY 'shopio_local_runtime'
    ACCOUNT UNLOCK;
