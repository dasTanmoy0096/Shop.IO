-- Verifies the initial Shop.IO schema and reference data. Run as shopio_migrator against shop_io.

DELIMITER //

BEGIN NOT ATOMIC
    IF COALESCE(DATABASE(), '') <> 'shop_io' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Verification must target shop_io.';
    END IF;

    IF @@SESSION.time_zone <> '+00:00' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'The session time zone must be +00:00.';
    END IF;

    IF @@SESSION.check_constraint_checks <> 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'CHECK constraint enforcement must be enabled.';
    END IF;

    IF @@SESSION.foreign_key_checks <> 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Foreign-key enforcement must be enabled.';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.SCHEMATA
        WHERE SCHEMA_NAME = DATABASE()
            AND DEFAULT_CHARACTER_SET_NAME = 'utf8mb4'
            AND DEFAULT_COLLATION_NAME = 'utf8mb4_unicode_ci'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'shop_io character set or collation is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME IN (
                'account',
                'account_role',
                'account_role_assignment',
                'category',
                'product_status',
                'product_status_transition',
                'product',
                'product_image',
                'shopping_cart',
                'cart_item',
                'sales_order_status',
                'sales_order',
                'sales_order_item',
                'payment_attempt_status',
                'payment_attempt',
                'payment_webhook_event'
            )
    ) <> 16 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An expected initial table is missing.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME IN (
                'account',
                'account_role',
                'account_role_assignment',
                'category',
                'product_status',
                'product_status_transition',
                'product',
                'product_image',
                'shopping_cart',
                'cart_item',
                'sales_order_status',
                'sales_order',
                'sales_order_item',
                'payment_attempt_status',
                'payment_attempt',
                'payment_webhook_event'
            )
            AND (
                TABLE_TYPE <> 'BASE TABLE'
                OR ENGINE <> 'InnoDB'
                OR TABLE_COLLATION <> 'utf8mb4_unicode_ci'
            )
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An initial table has the wrong type, engine, or collation.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT 'account' AS table_name, 9 AS expected_column_count
            UNION ALL SELECT 'account_role', 6
            UNION ALL SELECT 'account_role_assignment', 4
            UNION ALL SELECT 'category', 7
            UNION ALL SELECT 'product_status', 8
            UNION ALL SELECT 'product_status_transition', 6
            UNION ALL SELECT 'product', 15
            UNION ALL SELECT 'product_image', 12
            UNION ALL SELECT 'shopping_cart', 6
            UNION ALL SELECT 'cart_item', 6
            UNION ALL SELECT 'sales_order_status', 7
            UNION ALL SELECT 'sales_order', 20
            UNION ALL SELECT 'sales_order_item', 14
            UNION ALL SELECT 'payment_attempt_status', 7
            UNION ALL SELECT 'payment_attempt', 14
            UNION ALL SELECT 'payment_webhook_event', 10
        ) AS expected
        LEFT JOIN (
            SELECT TABLE_NAME, COUNT(*) AS actual_column_count
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            GROUP BY TABLE_NAME
        ) AS actual
            ON actual.TABLE_NAME = expected.table_name
        WHERE COALESCE(actual.actual_column_count, 0) <> expected.expected_column_count
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An initial table has the wrong column count.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM (
            SELECT
                TABLE_NAME,
                GROUP_CONCAT(
                    COLUMN_NAME
                    ORDER BY ORDINAL_POSITION
                    SEPARATOR ','
                ) AS column_names
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME IN (
                    'account',
                    'account_role',
                    'account_role_assignment',
                    'category',
                    'product_status',
                    'product_status_transition',
                    'product',
                    'product_image',
                    'shopping_cart',
                    'cart_item',
                    'sales_order_status',
                    'sales_order',
                    'sales_order_item',
                    'payment_attempt_status',
                    'payment_attempt',
                    'payment_webhook_event'
                )
            GROUP BY TABLE_NAME
        ) AS column_definitions
        WHERE
            (TABLE_NAME = 'account' AND column_names = 'account_id,public_id,username,normalized_username,password_hash,security_stamp,is_active,created_utc,updated_utc')
            OR (TABLE_NAME = 'account_role' AND column_names = 'role_code,display_name,display_order,is_privileged,created_utc,updated_utc')
            OR (TABLE_NAME = 'account_role_assignment' AND column_names = 'account_id,role_code,assigned_by_account_id,assigned_utc')
            OR (TABLE_NAME = 'category' AND column_names = 'category_id,title,canonical_slug,display_order,is_active,created_utc,updated_utc')
            OR (TABLE_NAME = 'product_status' AND column_names = 'status_code,display_name,display_order,is_public,allows_seller_edit,is_terminal,created_utc,updated_utc')
            OR (TABLE_NAME = 'product_status_transition' AND column_names = 'from_status_code,to_status_code,actor_kind,requires_reason,display_order,created_utc')
            OR (TABLE_NAME = 'product' AND column_names = 'product_id,public_id,seller_account_id,category_id,status_code,canonical_slug,title,description,unit_price_minor,currency_code,row_version,submitted_utc,retired_utc,created_utc,updated_utc')
            OR (TABLE_NAME = 'product_image' AND column_names = 'product_image_id,product_id,storage_key,content_type,content_length_bytes,width_pixels,height_pixels,display_order,is_primary,primary_for_product_id,created_utc,updated_utc')
            OR (TABLE_NAME = 'shopping_cart' AND column_names = 'shopping_cart_id,account_id,guest_capability_hash,created_utc,updated_utc,last_activity_utc')
            OR (TABLE_NAME = 'cart_item' AND column_names = 'cart_item_id,shopping_cart_id,product_id,quantity,created_utc,updated_utc')
            OR (TABLE_NAME = 'sales_order_status' AND column_names = 'status_code,display_name,display_order,is_terminal,is_payment_complete,created_utc,updated_utc')
            OR (TABLE_NAME = 'sales_order' AND column_names = 'sales_order_id,public_id,buyer_account_id,status_code,recipient_given_name,recipient_family_name,delivery_address_line1,delivery_address_line2,delivery_locality,delivery_region,delivery_postal_code,delivery_country_code,currency_code,subtotal_minor,total_minor,paid_utc,cancelled_utc,refunded_utc,created_utc,updated_utc')
            OR (TABLE_NAME = 'sales_order_item' AND column_names = 'sales_order_item_id,sales_order_id,product_id,seller_account_id,seller_public_id,seller_username,product_public_id,product_title,product_image_storage_key,unit_price_minor,quantity,line_total_minor,currency_code,created_utc')
            OR (TABLE_NAME = 'payment_attempt_status' AND column_names = 'status_code,display_name,display_order,is_terminal,is_successful,created_utc,updated_utc')
            OR (TABLE_NAME = 'payment_attempt' AND column_names = 'payment_attempt_id,sales_order_id,currency_code,status_code,provider_code,attempt_number,provider_checkout_session_id,provider_payment_id,amount_minor,provider_failure_code,provider_created_utc,completed_utc,created_utc,updated_utc')
            OR (TABLE_NAME = 'payment_webhook_event' AND column_names = 'payment_webhook_event_id,payment_attempt_id,provider_code,provider_event_id,event_type,processing_status,payload_sha256,received_utc,verified_utc,applied_utc')
    ) <> 16 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An initial table has the wrong column definitions.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT 'account' AS table_name, 'account_id' AS column_name
            UNION ALL SELECT 'category', 'category_id'
            UNION ALL SELECT 'product', 'product_id'
            UNION ALL SELECT 'product_image', 'product_image_id'
            UNION ALL SELECT 'shopping_cart', 'shopping_cart_id'
            UNION ALL SELECT 'cart_item', 'cart_item_id'
            UNION ALL SELECT 'sales_order', 'sales_order_id'
            UNION ALL SELECT 'sales_order_item', 'sales_order_item_id'
            UNION ALL SELECT 'payment_attempt', 'payment_attempt_id'
            UNION ALL SELECT 'payment_webhook_event', 'payment_webhook_event_id'
        ) AS expected
        LEFT JOIN information_schema.COLUMNS AS actual
            ON actual.TABLE_SCHEMA = DATABASE()
            AND actual.TABLE_NAME = expected.table_name
            AND actual.COLUMN_NAME = expected.column_name
        WHERE actual.COLUMN_NAME IS NULL
            OR actual.DATA_TYPE <> 'bigint'
            OR actual.IS_NULLABLE <> 'NO'
            OR actual.EXTRA NOT LIKE '%auto_increment%'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An internal auto-increment identifier is incorrect.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT 'account' AS table_name, 'public_id' AS column_name
            UNION ALL SELECT 'product', 'public_id'
            UNION ALL SELECT 'sales_order', 'public_id'
            UNION ALL SELECT 'sales_order_item', 'seller_public_id'
            UNION ALL SELECT 'sales_order_item', 'product_public_id'
        ) AS expected
        LEFT JOIN information_schema.COLUMNS AS actual
            ON actual.TABLE_SCHEMA = DATABASE()
            AND actual.TABLE_NAME = expected.table_name
            AND actual.COLUMN_NAME = expected.column_name
        WHERE actual.COLUMN_NAME IS NULL
            OR actual.DATA_TYPE <> 'char'
            OR actual.CHARACTER_MAXIMUM_LENGTH <> 36
            OR actual.CHARACTER_SET_NAME <> 'ascii'
            OR actual.COLLATION_NAME <> 'ascii_bin'
            OR actual.IS_NULLABLE <> 'NO'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An opaque public identifier column is incorrect.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT 'product' AS table_name, 'currency_code' AS column_name
            UNION ALL SELECT 'sales_order', 'currency_code'
            UNION ALL SELECT 'sales_order_item', 'currency_code'
            UNION ALL SELECT 'payment_attempt', 'currency_code'
        ) AS expected
        LEFT JOIN information_schema.COLUMNS AS actual
            ON actual.TABLE_SCHEMA = DATABASE()
            AND actual.TABLE_NAME = expected.table_name
            AND actual.COLUMN_NAME = expected.column_name
        WHERE actual.COLUMN_NAME IS NULL
            OR actual.DATA_TYPE <> 'char'
            OR actual.CHARACTER_MAXIMUM_LENGTH <> 3
            OR actual.CHARACTER_SET_NAME <> 'ascii'
            OR actual.COLLATION_NAME <> 'ascii_bin'
            OR actual.IS_NULLABLE <> 'NO'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A currency column is incorrect.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT 'product' AS table_name, 'unit_price_minor' AS column_name
            UNION ALL SELECT 'sales_order', 'subtotal_minor'
            UNION ALL SELECT 'sales_order', 'total_minor'
            UNION ALL SELECT 'sales_order_item', 'unit_price_minor'
            UNION ALL SELECT 'sales_order_item', 'line_total_minor'
            UNION ALL SELECT 'payment_attempt', 'amount_minor'
        ) AS expected
        LEFT JOIN information_schema.COLUMNS AS actual
            ON actual.TABLE_SCHEMA = DATABASE()
            AND actual.TABLE_NAME = expected.table_name
            AND actual.COLUMN_NAME = expected.column_name
        WHERE actual.COLUMN_NAME IS NULL
            OR actual.DATA_TYPE <> 'bigint'
            OR actual.IS_NULLABLE <> 'NO'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A monetary column is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
            AND RIGHT(COLUMN_NAME, 4) = '_utc'
    ) <> 38
        OR EXISTS (
            SELECT 1
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
                AND RIGHT(COLUMN_NAME, 4) = '_utc'
                AND (
                    DATA_TYPE <> 'datetime'
                    OR DATETIME_PRECISION <> 6
                )
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'UTC audit columns must use DATETIME(6).';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = 'shopping_cart'
            AND COLUMN_NAME = 'guest_capability_hash'
            AND (
                DATA_TYPE <> 'char'
                OR CHARACTER_MAXIMUM_LENGTH <> 64
                OR CHARACTER_SET_NAME <> 'ascii'
                OR COLLATION_NAME <> 'ascii_bin'
                OR IS_NULLABLE <> 'YES'
            )
    ) OR NOT EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = 'shopping_cart'
            AND COLUMN_NAME = 'guest_capability_hash'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'The guest cart capability column is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM information_schema.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE()
            AND TABLE_NAME IN (
                'account',
                'account_role',
                'account_role_assignment',
                'category',
                'product_status',
                'product_status_transition',
                'product',
                'product_image',
                'shopping_cart',
                'cart_item',
                'sales_order_status',
                'sales_order',
                'sales_order_item',
                'payment_attempt_status',
                'payment_attempt',
                'payment_webhook_event'
            )
    ) <> 138
        OR (
            SELECT COUNT(*)
            FROM information_schema.TABLE_CONSTRAINTS
            WHERE CONSTRAINT_SCHEMA = DATABASE()
                AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        ) <> 16
        OR (
            SELECT COUNT(*)
            FROM information_schema.TABLE_CONSTRAINTS
            WHERE CONSTRAINT_SCHEMA = DATABASE()
                AND CONSTRAINT_TYPE = 'UNIQUE'
        ) <> 24
        OR (
            SELECT COUNT(*)
            FROM information_schema.TABLE_CONSTRAINTS
            WHERE CONSTRAINT_SCHEMA = DATABASE()
                AND CONSTRAINT_TYPE = 'FOREIGN KEY'
        ) <> 20
        OR (
            SELECT COUNT(*)
            FROM information_schema.TABLE_CONSTRAINTS
            WHERE CONSTRAINT_SCHEMA = DATABASE()
                AND CONSTRAINT_TYPE = 'CHECK'
        ) <> 78 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Initial constraint counts are incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM (
            SELECT
                constraints.TABLE_NAME,
                constraints.CONSTRAINT_NAME,
                constraints.CONSTRAINT_TYPE,
                GROUP_CONCAT(
                    columns_usage.COLUMN_NAME
                    ORDER BY columns_usage.ORDINAL_POSITION
                    SEPARATOR ','
                ) AS column_names
            FROM information_schema.TABLE_CONSTRAINTS AS constraints
            INNER JOIN information_schema.KEY_COLUMN_USAGE AS columns_usage
                ON columns_usage.CONSTRAINT_SCHEMA = constraints.CONSTRAINT_SCHEMA
                AND columns_usage.TABLE_NAME = constraints.TABLE_NAME
                AND columns_usage.CONSTRAINT_NAME = constraints.CONSTRAINT_NAME
            WHERE constraints.CONSTRAINT_SCHEMA = DATABASE()
                AND constraints.CONSTRAINT_TYPE IN ('PRIMARY KEY', 'UNIQUE')
            GROUP BY
                constraints.TABLE_NAME,
                constraints.CONSTRAINT_NAME,
                constraints.CONSTRAINT_TYPE
        ) AS key_definitions
        WHERE
            (TABLE_NAME = 'account' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'account_id')
            OR (TABLE_NAME = 'account' AND CONSTRAINT_NAME = 'uq_account_public_id' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'public_id')
            OR (TABLE_NAME = 'account' AND CONSTRAINT_NAME = 'uq_account_normalized_username' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'normalized_username')
            OR (TABLE_NAME = 'account_role' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'role_code')
            OR (TABLE_NAME = 'account_role' AND CONSTRAINT_NAME = 'uq_account_role_display_name' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'display_name')
            OR (TABLE_NAME = 'account_role_assignment' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'account_id,role_code')
            OR (TABLE_NAME = 'category' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'category_id')
            OR (TABLE_NAME = 'category' AND CONSTRAINT_NAME = 'uq_category_title' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'title')
            OR (TABLE_NAME = 'category' AND CONSTRAINT_NAME = 'uq_category_canonical_slug' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'canonical_slug')
            OR (TABLE_NAME = 'product_status' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'status_code')
            OR (TABLE_NAME = 'product_status' AND CONSTRAINT_NAME = 'uq_product_status_display_name' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'display_name')
            OR (TABLE_NAME = 'product_status_transition' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'from_status_code,to_status_code,actor_kind')
            OR (TABLE_NAME = 'product' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'product_id')
            OR (TABLE_NAME = 'product' AND CONSTRAINT_NAME = 'uq_product_public_id' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'public_id')
            OR (TABLE_NAME = 'product' AND CONSTRAINT_NAME = 'uq_product_category_canonical_slug' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'category_id,canonical_slug')
            OR (TABLE_NAME = 'product_image' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'product_image_id')
            OR (TABLE_NAME = 'product_image' AND CONSTRAINT_NAME = 'uq_product_image_storage_key' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'storage_key')
            OR (TABLE_NAME = 'product_image' AND CONSTRAINT_NAME = 'uq_product_image_display_order' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'product_id,display_order')
            OR (TABLE_NAME = 'product_image' AND CONSTRAINT_NAME = 'uq_product_image_primary_for_product' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'primary_for_product_id')
            OR (TABLE_NAME = 'shopping_cart' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'shopping_cart_id')
            OR (TABLE_NAME = 'shopping_cart' AND CONSTRAINT_NAME = 'uq_shopping_cart_account' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'account_id')
            OR (TABLE_NAME = 'shopping_cart' AND CONSTRAINT_NAME = 'uq_shopping_cart_guest_capability_hash' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'guest_capability_hash')
            OR (TABLE_NAME = 'cart_item' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'cart_item_id')
            OR (TABLE_NAME = 'cart_item' AND CONSTRAINT_NAME = 'uq_cart_item_cart_product' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'shopping_cart_id,product_id')
            OR (TABLE_NAME = 'sales_order_status' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'status_code')
            OR (TABLE_NAME = 'sales_order_status' AND CONSTRAINT_NAME = 'uq_sales_order_status_display_name' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'display_name')
            OR (TABLE_NAME = 'sales_order' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'sales_order_id')
            OR (TABLE_NAME = 'sales_order' AND CONSTRAINT_NAME = 'uq_sales_order_public_id' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'public_id')
            OR (TABLE_NAME = 'sales_order' AND CONSTRAINT_NAME = 'uq_sales_order_id_currency' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'sales_order_id,currency_code')
            OR (TABLE_NAME = 'sales_order' AND CONSTRAINT_NAME = 'uq_sales_order_id_currency_total' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'sales_order_id,currency_code,total_minor')
            OR (TABLE_NAME = 'sales_order_item' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'sales_order_item_id')
            OR (TABLE_NAME = 'sales_order_item' AND CONSTRAINT_NAME = 'uq_sales_order_item_order_product' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'sales_order_id,product_public_id')
            OR (TABLE_NAME = 'payment_attempt_status' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'status_code')
            OR (TABLE_NAME = 'payment_attempt_status' AND CONSTRAINT_NAME = 'uq_payment_attempt_status_display_name' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'display_name')
            OR (TABLE_NAME = 'payment_attempt' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'payment_attempt_id')
            OR (TABLE_NAME = 'payment_attempt' AND CONSTRAINT_NAME = 'uq_payment_attempt_order_number' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'sales_order_id,attempt_number')
            OR (TABLE_NAME = 'payment_attempt' AND CONSTRAINT_NAME = 'uq_payment_attempt_checkout_session' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'provider_code,provider_checkout_session_id')
            OR (TABLE_NAME = 'payment_attempt' AND CONSTRAINT_NAME = 'uq_payment_attempt_provider_payment' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'provider_code,provider_payment_id')
            OR (TABLE_NAME = 'payment_webhook_event' AND CONSTRAINT_NAME = 'PRIMARY' AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND column_names = 'payment_webhook_event_id')
            OR (TABLE_NAME = 'payment_webhook_event' AND CONSTRAINT_NAME = 'uq_payment_webhook_event_provider_event' AND CONSTRAINT_TYPE = 'UNIQUE' AND column_names = 'provider_code,provider_event_id')
    ) <> 40 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Primary-key or unique-key definitions are incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM information_schema.CHECK_CONSTRAINTS AS checks
        INNER JOIN information_schema.TABLE_CONSTRAINTS AS constraints
            ON constraints.CONSTRAINT_SCHEMA = checks.CONSTRAINT_SCHEMA
            AND constraints.CONSTRAINT_NAME = checks.CONSTRAINT_NAME
        WHERE checks.CONSTRAINT_SCHEMA = DATABASE()
            AND constraints.CONSTRAINT_TYPE = 'CHECK'
    ) <> 78
        OR EXISTS (
            SELECT 1
            FROM information_schema.TABLE_CONSTRAINTS AS constraints
            WHERE constraints.CONSTRAINT_SCHEMA = DATABASE()
                AND constraints.CONSTRAINT_TYPE = 'CHECK'
                AND (constraints.TABLE_NAME, constraints.CONSTRAINT_NAME) NOT IN (
                    ('account', 'ck_account_public_id_length'),
                    ('account', 'ck_account_username_not_empty'),
                    ('account', 'ck_account_normalized_username_not_empty'),
                    ('account', 'ck_account_security_stamp_length'),
                    ('account', 'ck_account_is_active'),
                    ('account_role', 'ck_account_role_code_not_empty'),
                    ('account_role', 'ck_account_role_display_order_non_negative'),
                    ('account_role', 'ck_account_role_is_privileged'),
                    ('category', 'ck_category_title_not_empty'),
                    ('category', 'ck_category_slug_not_empty'),
                    ('category', 'ck_category_display_order_non_negative'),
                    ('category', 'ck_category_is_active'),
                    ('product_status', 'ck_product_status_code_not_empty'),
                    ('product_status', 'ck_product_status_display_order_non_negative'),
                    ('product_status', 'ck_product_status_is_public'),
                    ('product_status', 'ck_product_status_allows_seller_edit'),
                    ('product_status', 'ck_product_status_is_terminal'),
                    ('product_status_transition', 'ck_product_status_transition_distinct_states'),
                    ('product_status_transition', 'ck_product_status_transition_actor_kind'),
                    ('product_status_transition', 'ck_product_status_transition_requires_reason'),
                    ('product_status_transition', 'ck_product_status_transition_display_order_non_negative'),
                    ('product', 'ck_product_public_id_length'),
                    ('product', 'ck_product_slug_not_empty'),
                    ('product', 'ck_product_title_not_empty'),
                    ('product', 'ck_product_unit_price_positive'),
                    ('product', 'ck_product_currency_code_length'),
                    ('product', 'ck_product_row_version_positive'),
                    ('product_image', 'ck_product_image_storage_key_not_empty'),
                    ('product_image', 'ck_product_image_content_type_not_empty'),
                    ('product_image', 'ck_product_image_content_length_positive'),
                    ('product_image', 'ck_product_image_width_positive'),
                    ('product_image', 'ck_product_image_height_positive'),
                    ('product_image', 'ck_product_image_display_order_non_negative'),
                    ('product_image', 'ck_product_image_is_primary'),
                    ('product_image', 'ck_product_image_primary_marker'),
                    ('shopping_cart', 'ck_shopping_cart_guest_capability_hash_not_empty'),
                    ('shopping_cart', 'ck_shopping_cart_guest_capability_hash_length'),
                    ('shopping_cart', 'ck_shopping_cart_single_owner'),
                    ('cart_item', 'ck_cart_item_quantity_positive'),
                    ('sales_order_status', 'ck_sales_order_status_code_not_empty'),
                    ('sales_order_status', 'ck_sales_order_status_display_order_non_negative'),
                    ('sales_order_status', 'ck_sales_order_status_is_terminal'),
                    ('sales_order_status', 'ck_sales_order_status_is_payment_complete'),
                    ('sales_order', 'ck_sales_order_public_id_length'),
                    ('sales_order', 'ck_sales_order_recipient_given_name_not_empty'),
                    ('sales_order', 'ck_sales_order_recipient_family_name_not_empty'),
                    ('sales_order', 'ck_sales_order_address_line1_not_empty'),
                    ('sales_order', 'ck_sales_order_locality_not_empty'),
                    ('sales_order', 'ck_sales_order_postal_code_not_empty'),
                    ('sales_order', 'ck_sales_order_country_code_length'),
                    ('sales_order', 'ck_sales_order_currency_code_length'),
                    ('sales_order', 'ck_sales_order_subtotal_positive'),
                    ('sales_order', 'ck_sales_order_total_positive'),
                    ('sales_order_item', 'ck_sales_order_item_seller_public_id_length'),
                    ('sales_order_item', 'ck_sales_order_item_seller_username_not_empty'),
                    ('sales_order_item', 'ck_sales_order_item_product_public_id_length'),
                    ('sales_order_item', 'ck_sales_order_item_product_title_not_empty'),
                    ('sales_order_item', 'ck_sales_order_item_unit_price_positive'),
                    ('sales_order_item', 'ck_sales_order_item_quantity_positive'),
                    ('sales_order_item', 'ck_sales_order_item_line_total_positive'),
                    ('sales_order_item', 'ck_sales_order_item_line_total_matches_unit_price'),
                    ('sales_order_item', 'ck_sales_order_item_currency_code_length'),
                    ('payment_attempt_status', 'ck_payment_attempt_status_code_not_empty'),
                    ('payment_attempt_status', 'ck_payment_attempt_status_display_order_non_negative'),
                    ('payment_attempt_status', 'ck_payment_attempt_status_is_terminal'),
                    ('payment_attempt_status', 'ck_payment_attempt_status_is_successful'),
                    ('payment_attempt', 'ck_payment_attempt_provider_code_not_empty'),
                    ('payment_attempt', 'ck_payment_attempt_attempt_number_positive'),
                    ('payment_attempt', 'ck_payment_attempt_amount_positive'),
                    ('payment_attempt', 'ck_payment_attempt_currency_code_length'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_provider_code_not_empty'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_provider_event_id_not_empty'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_type_not_empty'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_processing_status'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_payload_sha256_length'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_verified_after_received'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_applied_after_received'),
                    ('payment_webhook_event', 'ck_payment_webhook_event_applied_is_verified')
                )
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'CHECK constraint definitions are incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM (
            SELECT
                statistics.TABLE_NAME,
                statistics.INDEX_NAME,
                MIN(statistics.NON_UNIQUE) AS non_unique,
                MIN(statistics.INDEX_TYPE) AS index_type,
                GROUP_CONCAT(
                    statistics.COLUMN_NAME
                    ORDER BY statistics.SEQ_IN_INDEX
                    SEPARATOR ','
                ) AS column_names
            FROM information_schema.STATISTICS AS statistics
            WHERE statistics.TABLE_SCHEMA = DATABASE()
                AND statistics.INDEX_NAME IN (
                    'ix_account_role_assignment_role',
                    'ix_account_role_assignment_assigned_by',
                    'ix_category_active_display_order',
                    'ix_product_status_transition_to',
                    'ix_product_public_latest',
                    'ix_product_public_catalogue',
                    'ix_product_seller_inventory',
                    'ix_product_seller_catalogue',
                    'ft_product_search',
                    'ix_cart_item_product',
                    'ix_sales_order_buyer_created',
                    'ix_sales_order_status_created',
                    'ix_sales_order_item_order_currency',
                    'ix_sales_order_item_product',
                    'ix_sales_order_item_seller_order',
                    'ix_payment_attempt_order_currency_amount',
                    'ix_payment_attempt_status_created',
                    'ix_payment_webhook_event_attempt',
                    'ix_payment_webhook_event_status_received'
                )
            GROUP BY statistics.TABLE_NAME, statistics.INDEX_NAME
        ) AS index_definitions
        WHERE
            (TABLE_NAME = 'account_role_assignment' AND INDEX_NAME = 'ix_account_role_assignment_role' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'role_code')
            OR (TABLE_NAME = 'account_role_assignment' AND INDEX_NAME = 'ix_account_role_assignment_assigned_by' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'assigned_by_account_id')
            OR (TABLE_NAME = 'category' AND INDEX_NAME = 'ix_category_active_display_order' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'is_active,display_order')
            OR (TABLE_NAME = 'product_status_transition' AND INDEX_NAME = 'ix_product_status_transition_to' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'to_status_code')
            OR (TABLE_NAME = 'product' AND INDEX_NAME = 'ix_product_public_latest' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'status_code,created_utc')
            OR (TABLE_NAME = 'product' AND INDEX_NAME = 'ix_product_public_catalogue' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'status_code,category_id,created_utc')
            OR (TABLE_NAME = 'product' AND INDEX_NAME = 'ix_product_seller_inventory' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'seller_account_id,status_code,updated_utc')
            OR (TABLE_NAME = 'product' AND INDEX_NAME = 'ix_product_seller_catalogue' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'seller_account_id,status_code,created_utc')
            OR (TABLE_NAME = 'product' AND INDEX_NAME = 'ft_product_search' AND non_unique = 1 AND index_type = 'FULLTEXT' AND column_names = 'title,description')
            OR (TABLE_NAME = 'cart_item' AND INDEX_NAME = 'ix_cart_item_product' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'product_id')
            OR (TABLE_NAME = 'sales_order' AND INDEX_NAME = 'ix_sales_order_buyer_created' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'buyer_account_id,created_utc')
            OR (TABLE_NAME = 'sales_order' AND INDEX_NAME = 'ix_sales_order_status_created' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'status_code,created_utc')
            OR (TABLE_NAME = 'sales_order_item' AND INDEX_NAME = 'ix_sales_order_item_order_currency' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'sales_order_id,currency_code')
            OR (TABLE_NAME = 'sales_order_item' AND INDEX_NAME = 'ix_sales_order_item_product' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'product_id')
            OR (TABLE_NAME = 'sales_order_item' AND INDEX_NAME = 'ix_sales_order_item_seller_order' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'seller_account_id,sales_order_id')
            OR (TABLE_NAME = 'payment_attempt' AND INDEX_NAME = 'ix_payment_attempt_order_currency_amount' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'sales_order_id,currency_code,amount_minor')
            OR (TABLE_NAME = 'payment_attempt' AND INDEX_NAME = 'ix_payment_attempt_status_created' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'status_code,created_utc')
            OR (TABLE_NAME = 'payment_webhook_event' AND INDEX_NAME = 'ix_payment_webhook_event_attempt' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'payment_attempt_id')
            OR (TABLE_NAME = 'payment_webhook_event' AND INDEX_NAME = 'ix_payment_webhook_event_status_received' AND non_unique = 1 AND index_type = 'BTREE' AND column_names = 'processing_status,received_utc')
    ) <> 19 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A named index definition is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM (
            SELECT
                key_usage.TABLE_NAME,
                key_usage.CONSTRAINT_NAME,
                GROUP_CONCAT(
                    key_usage.COLUMN_NAME
                    ORDER BY key_usage.ORDINAL_POSITION
                    SEPARATOR ','
                ) AS child_columns,
                MIN(key_usage.REFERENCED_TABLE_NAME) AS referenced_table_name,
                GROUP_CONCAT(
                    key_usage.REFERENCED_COLUMN_NAME
                    ORDER BY key_usage.ORDINAL_POSITION
                    SEPARATOR ','
                ) AS referenced_columns,
                MIN(referential_rules.UPDATE_RULE) AS update_rule,
                MIN(referential_rules.DELETE_RULE) AS delete_rule
            FROM information_schema.KEY_COLUMN_USAGE AS key_usage
            INNER JOIN information_schema.REFERENTIAL_CONSTRAINTS AS referential_rules
                ON referential_rules.CONSTRAINT_SCHEMA = key_usage.CONSTRAINT_SCHEMA
                AND referential_rules.TABLE_NAME = key_usage.TABLE_NAME
                AND referential_rules.CONSTRAINT_NAME = key_usage.CONSTRAINT_NAME
            WHERE key_usage.CONSTRAINT_SCHEMA = DATABASE()
                AND key_usage.REFERENCED_TABLE_NAME IS NOT NULL
            GROUP BY key_usage.TABLE_NAME, key_usage.CONSTRAINT_NAME
        ) AS foreign_key_definitions
        WHERE
            (TABLE_NAME = 'account_role_assignment' AND CONSTRAINT_NAME = 'fk_account_role_assignment_account' AND child_columns = 'account_id' AND referenced_table_name = 'account' AND referenced_columns = 'account_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'account_role_assignment' AND CONSTRAINT_NAME = 'fk_account_role_assignment_role' AND child_columns = 'role_code' AND referenced_table_name = 'account_role' AND referenced_columns = 'role_code' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'account_role_assignment' AND CONSTRAINT_NAME = 'fk_account_role_assignment_assigned_by' AND child_columns = 'assigned_by_account_id' AND referenced_table_name = 'account' AND referenced_columns = 'account_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'product_status_transition' AND CONSTRAINT_NAME = 'fk_product_status_transition_from' AND child_columns = 'from_status_code' AND referenced_table_name = 'product_status' AND referenced_columns = 'status_code' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'product_status_transition' AND CONSTRAINT_NAME = 'fk_product_status_transition_to' AND child_columns = 'to_status_code' AND referenced_table_name = 'product_status' AND referenced_columns = 'status_code' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'product' AND CONSTRAINT_NAME = 'fk_product_seller_account' AND child_columns = 'seller_account_id' AND referenced_table_name = 'account' AND referenced_columns = 'account_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'product' AND CONSTRAINT_NAME = 'fk_product_category' AND child_columns = 'category_id' AND referenced_table_name = 'category' AND referenced_columns = 'category_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'product' AND CONSTRAINT_NAME = 'fk_product_status' AND child_columns = 'status_code' AND referenced_table_name = 'product_status' AND referenced_columns = 'status_code' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'product_image' AND CONSTRAINT_NAME = 'fk_product_image_product' AND child_columns = 'product_id' AND referenced_table_name = 'product' AND referenced_columns = 'product_id' AND update_rule = 'RESTRICT' AND delete_rule = 'CASCADE')
            OR (TABLE_NAME = 'shopping_cart' AND CONSTRAINT_NAME = 'fk_shopping_cart_account' AND child_columns = 'account_id' AND referenced_table_name = 'account' AND referenced_columns = 'account_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'cart_item' AND CONSTRAINT_NAME = 'fk_cart_item_shopping_cart' AND child_columns = 'shopping_cart_id' AND referenced_table_name = 'shopping_cart' AND referenced_columns = 'shopping_cart_id' AND update_rule = 'RESTRICT' AND delete_rule = 'CASCADE')
            OR (TABLE_NAME = 'cart_item' AND CONSTRAINT_NAME = 'fk_cart_item_product' AND child_columns = 'product_id' AND referenced_table_name = 'product' AND referenced_columns = 'product_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'sales_order' AND CONSTRAINT_NAME = 'fk_sales_order_buyer_account' AND child_columns = 'buyer_account_id' AND referenced_table_name = 'account' AND referenced_columns = 'account_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'sales_order' AND CONSTRAINT_NAME = 'fk_sales_order_status' AND child_columns = 'status_code' AND referenced_table_name = 'sales_order_status' AND referenced_columns = 'status_code' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'sales_order_item' AND CONSTRAINT_NAME = 'fk_sales_order_item_order_currency' AND child_columns = 'sales_order_id,currency_code' AND referenced_table_name = 'sales_order' AND referenced_columns = 'sales_order_id,currency_code' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'sales_order_item' AND CONSTRAINT_NAME = 'fk_sales_order_item_product' AND child_columns = 'product_id' AND referenced_table_name = 'product' AND referenced_columns = 'product_id' AND update_rule = 'RESTRICT' AND delete_rule = 'SET NULL')
            OR (TABLE_NAME = 'sales_order_item' AND CONSTRAINT_NAME = 'fk_sales_order_item_seller_account' AND child_columns = 'seller_account_id' AND referenced_table_name = 'account' AND referenced_columns = 'account_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'payment_attempt' AND CONSTRAINT_NAME = 'fk_payment_attempt_order_currency_amount' AND child_columns = 'sales_order_id,currency_code,amount_minor' AND referenced_table_name = 'sales_order' AND referenced_columns = 'sales_order_id,currency_code,total_minor' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'payment_attempt' AND CONSTRAINT_NAME = 'fk_payment_attempt_status' AND child_columns = 'status_code' AND referenced_table_name = 'payment_attempt_status' AND referenced_columns = 'status_code' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
            OR (TABLE_NAME = 'payment_webhook_event' AND CONSTRAINT_NAME = 'fk_payment_webhook_event_payment_attempt' AND child_columns = 'payment_attempt_id' AND referenced_table_name = 'payment_attempt' AND referenced_columns = 'payment_attempt_id' AND update_rule = 'RESTRICT' AND delete_rule = 'RESTRICT')
    ) <> 20 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A foreign-key definition is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM account_role
    ) <> 2
        OR EXISTS (
            SELECT 1
            FROM account_role
            WHERE (role_code, display_name, display_order, is_privileged) NOT IN (
                ('administrator', 'Administrator', 10, 1),
                ('moderator', 'Moderator', 20, 1)
            )
                OR created_utc IS NULL
                OR updated_utc IS NULL
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Reference account-role data is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM product_status
    ) <> 4
        OR EXISTS (
            SELECT 1
            FROM product_status
            WHERE (status_code, display_name, display_order, is_public, allows_seller_edit, is_terminal) NOT IN (
                ('draft', 'Draft', 10, 0, 1, 0),
                ('waitingapproval', 'Waiting approval', 20, 0, 0, 0),
                ('active', 'Active', 30, 1, 0, 0),
                ('deleted', 'Deleted', 40, 0, 0, 1)
            )
                OR created_utc IS NULL
                OR updated_utc IS NULL
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Reference product-status data is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM product_status_transition
    ) <> 11
        OR EXISTS (
            SELECT 1
            FROM product_status_transition
            WHERE (from_status_code, to_status_code, actor_kind, requires_reason, display_order) NOT IN (
                ('draft', 'waitingapproval', 'seller', 0, 10),
                ('draft', 'deleted', 'seller', 0, 20),
                ('waitingapproval', 'draft', 'seller', 0, 30),
                ('waitingapproval', 'deleted', 'seller', 0, 40),
                ('active', 'draft', 'seller', 0, 50),
                ('active', 'deleted', 'seller', 0, 60),
                ('draft', 'deleted', 'moderator', 1, 70),
                ('waitingapproval', 'active', 'moderator', 0, 80),
                ('waitingapproval', 'draft', 'moderator', 1, 90),
                ('waitingapproval', 'deleted', 'moderator', 1, 100),
                ('active', 'deleted', 'moderator', 1, 110)
            )
                OR created_utc IS NULL
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Reference product-status-transition data is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM sales_order_status
    ) <> 5
        OR EXISTS (
            SELECT 1
            FROM sales_order_status
            WHERE (status_code, display_name, display_order, is_terminal, is_payment_complete) NOT IN (
                ('pending_payment', 'Pending payment', 10, 0, 0),
                ('paid', 'Paid', 20, 0, 1),
                ('payment_failed', 'Payment failed', 30, 0, 0),
                ('cancelled', 'Cancelled', 40, 1, 0),
                ('refunded', 'Refunded', 50, 1, 0)
            )
                OR created_utc IS NULL
                OR updated_utc IS NULL
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Reference sales-order-status data is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM payment_attempt_status
    ) <> 5
        OR EXISTS (
            SELECT 1
            FROM payment_attempt_status
            WHERE (status_code, display_name, display_order, is_terminal, is_successful) NOT IN (
                ('pending', 'Pending', 10, 0, 0),
                ('succeeded', 'Succeeded', 20, 1, 1),
                ('failed', 'Failed', 30, 1, 0),
                ('cancelled', 'Cancelled', 40, 1, 0),
                ('expired', 'Expired', 50, 1, 0)
            )
                OR created_utc IS NULL
                OR updated_utc IS NULL
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Reference payment-attempt-status data is incorrect.';
    END IF;

    IF (
        SELECT COUNT(*)
        FROM category
    ) <> 3
        OR EXISTS (
            SELECT 1
            FROM category
            WHERE (title, canonical_slug, display_order, is_active) NOT IN (
                ('Computers', 'computers', 10, 1),
                ('Smartphones', 'smartphones', 20, 1),
                ('Electronics', 'electronics', 30, 1)
            )
                OR created_utc IS NULL
                OR updated_utc IS NULL
        ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Reference category data is incorrect.';
    END IF;

    IF EXISTS (SELECT 1 FROM account)
        OR EXISTS (SELECT 1 FROM account_role_assignment)
        OR EXISTS (SELECT 1 FROM product)
        OR EXISTS (SELECT 1 FROM product_image)
        OR EXISTS (SELECT 1 FROM shopping_cart)
        OR EXISTS (SELECT 1 FROM cart_item)
        OR EXISTS (SELECT 1 FROM sales_order)
        OR EXISTS (SELECT 1 FROM sales_order_item)
        OR EXISTS (SELECT 1 FROM payment_attempt)
        OR EXISTS (SELECT 1 FROM payment_webhook_event) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Initial verification requires no application data.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM shopping_cart
        WHERE (account_id IS NULL AND guest_capability_hash IS NULL)
            OR (account_id IS NOT NULL AND guest_capability_hash IS NOT NULL)
            OR guest_capability_hash = ''
            OR (
                guest_capability_hash IS NOT NULL
                AND CHAR_LENGTH(guest_capability_hash) <> 64
            )
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A shopping-cart ownership invariant is violated.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM product_image
        WHERE is_primary NOT IN (0, 1)
            OR (is_primary = 0 AND primary_for_product_id IS NOT NULL)
            OR (
                is_primary = 1
                AND (
                    primary_for_product_id IS NULL
                    OR primary_for_product_id <> product_id
                )
            )
    ) OR EXISTS (
        SELECT 1
        FROM product_image
        WHERE is_primary = 1
        GROUP BY product_id
        HAVING COUNT(*) > 1
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A product-image primary-marker invariant is violated.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM product
        WHERE unit_price_minor <= 0
    ) OR EXISTS (
        SELECT 1
        FROM cart_item
        WHERE quantity <= 0
    ) OR EXISTS (
        SELECT 1
        FROM sales_order
        WHERE subtotal_minor <= 0
            OR total_minor <= 0
    ) OR EXISTS (
        SELECT 1
        FROM sales_order_item
        WHERE unit_price_minor <= 0
            OR quantity <= 0
            OR line_total_minor <= 0
            OR line_total_minor <> unit_price_minor * quantity
    ) OR EXISTS (
        SELECT 1
        FROM payment_attempt
        WHERE amount_minor <= 0
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A positive money or quantity invariant is violated.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM sales_order_item AS item
        LEFT JOIN sales_order AS order_header
            ON order_header.sales_order_id = item.sales_order_id
            AND order_header.currency_code = item.currency_code
        WHERE order_header.sales_order_id IS NULL
    ) OR EXISTS (
        SELECT 1
        FROM payment_attempt AS attempt
        LEFT JOIN sales_order AS order_header
            ON order_header.sales_order_id = attempt.sales_order_id
            AND order_header.currency_code = attempt.currency_code
            AND order_header.total_minor = attempt.amount_minor
        WHERE order_header.sales_order_id IS NULL
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An order currency or payment-total invariant is violated.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM payment_webhook_event
        WHERE verified_utc IS NOT NULL
            AND verified_utc < received_utc
    ) OR EXISTS (
        SELECT 1
        FROM payment_webhook_event
        WHERE applied_utc IS NOT NULL
            AND applied_utc < received_utc
    ) OR EXISTS (
        SELECT 1
        FROM payment_webhook_event
        WHERE processing_status = 'applied'
            AND (
                verified_utc IS NULL
                OR applied_utc IS NULL
                OR applied_utc < verified_utc
            )
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A payment-webhook state invariant is violated.';
    END IF;
END//

DELIMITER ;

SELECT 'Initial schema and reference data verification passed.' AS verification_result;
