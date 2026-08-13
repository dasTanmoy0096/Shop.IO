-- Run first.

CREATE USER IF NOT EXISTS 'root'@'127.0.0.1'
    IDENTIFIED BY 'shopio_local_root';

ALTER USER 'root'@'127.0.0.1'
    IDENTIFIED BY 'shopio_local_root';

GRANT ALL PRIVILEGES
    ON *.* TO 'root'@'127.0.0.1'
    WITH GRANT OPTION;
