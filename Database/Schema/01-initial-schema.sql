-- Initial Shop.IO relational schema. Run as shopio_migrator against shop_io.

SET SESSION time_zone = '+00:00';

CREATE TABLE IF NOT EXISTS `account` (
    `account_id` BIGINT NOT NULL AUTO_INCREMENT,
    `public_id` CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `username` VARCHAR(64) NOT NULL,
    `normalized_username` VARCHAR(64) NOT NULL,
    `password_hash` VARCHAR(512) NOT NULL,
    `security_stamp` CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_account` PRIMARY KEY (`account_id`),
    CONSTRAINT `uq_account_public_id` UNIQUE (`public_id`),
    CONSTRAINT `uq_account_normalized_username` UNIQUE (`normalized_username`),
    CONSTRAINT `ck_account_public_id_length` CHECK (CHAR_LENGTH(`public_id`) = 36),
    CONSTRAINT `ck_account_username_not_empty` CHECK (`username` <> ''),
    CONSTRAINT `ck_account_normalized_username_not_empty` CHECK (`normalized_username` <> ''),
    CONSTRAINT `ck_account_security_stamp_length` CHECK (CHAR_LENGTH(`security_stamp`) = 36),
    CONSTRAINT `ck_account_is_active` CHECK (`is_active` IN (0, 1))
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Local customer and seller account identity.';

CREATE TABLE IF NOT EXISTS `account_role` (
    `role_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `display_name` VARCHAR(64) NOT NULL,
    `display_order` INT NOT NULL,
    `is_privileged` TINYINT(1) NOT NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_account_role` PRIMARY KEY (`role_code`),
    CONSTRAINT `uq_account_role_display_name` UNIQUE (`display_name`),
    CONSTRAINT `ck_account_role_code_not_empty` CHECK (`role_code` <> ''),
    CONSTRAINT `ck_account_role_display_order_non_negative` CHECK (`display_order` >= 0),
    CONSTRAINT `ck_account_role_is_privileged` CHECK (`is_privileged` IN (0, 1))
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Privileged role definitions; ordinary accounts need no seller role.';

CREATE TABLE IF NOT EXISTS `account_role_assignment` (
    `account_id` BIGINT NOT NULL,
    `role_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `assigned_by_account_id` BIGINT NULL,
    `assigned_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_account_role_assignment` PRIMARY KEY (`account_id`, `role_code`),
    KEY `ix_account_role_assignment_role` (`role_code`),
    KEY `ix_account_role_assignment_assigned_by` (`assigned_by_account_id`),
    CONSTRAINT `fk_account_role_assignment_account`
        FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_account_role_assignment_role`
        FOREIGN KEY (`role_code`) REFERENCES `account_role` (`role_code`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_account_role_assignment_assigned_by`
        FOREIGN KEY (`assigned_by_account_id`) REFERENCES `account` (`account_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Explicit privileged-role assignments.';

CREATE TABLE IF NOT EXISTS `category` (
    `category_id` BIGINT NOT NULL AUTO_INCREMENT,
    `title` VARCHAR(100) NOT NULL,
    `canonical_slug` VARCHAR(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `display_order` INT NOT NULL DEFAULT 0,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_category` PRIMARY KEY (`category_id`),
    CONSTRAINT `uq_category_title` UNIQUE (`title`),
    CONSTRAINT `uq_category_canonical_slug` UNIQUE (`canonical_slug`),
    KEY `ix_category_active_display_order` (`is_active`, `display_order`),
    CONSTRAINT `ck_category_title_not_empty` CHECK (`title` <> ''),
    CONSTRAINT `ck_category_slug_not_empty` CHECK (`canonical_slug` <> ''),
    CONSTRAINT `ck_category_display_order_non_negative` CHECK (`display_order` >= 0),
    CONSTRAINT `ck_category_is_active` CHECK (`is_active` IN (0, 1))
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Public product catalogue taxonomy.';

CREATE TABLE IF NOT EXISTS `product_status` (
    `status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `display_name` VARCHAR(64) NOT NULL,
    `display_order` INT NOT NULL,
    `is_public` TINYINT(1) NOT NULL,
    `allows_seller_edit` TINYINT(1) NOT NULL,
    `is_terminal` TINYINT(1) NOT NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_product_status` PRIMARY KEY (`status_code`),
    CONSTRAINT `uq_product_status_display_name` UNIQUE (`display_name`),
    CONSTRAINT `ck_product_status_code_not_empty` CHECK (`status_code` <> ''),
    CONSTRAINT `ck_product_status_display_order_non_negative` CHECK (`display_order` >= 0),
    CONSTRAINT `ck_product_status_is_public` CHECK (`is_public` IN (0, 1)),
    CONSTRAINT `ck_product_status_allows_seller_edit` CHECK (`allows_seller_edit` IN (0, 1)),
    CONSTRAINT `ck_product_status_is_terminal` CHECK (`is_terminal` IN (0, 1))
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Product lifecycle state definitions.';

CREATE TABLE IF NOT EXISTS `product_status_transition` (
    `from_status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `to_status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `actor_kind` VARCHAR(16) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `requires_reason` TINYINT(1) NOT NULL DEFAULT 0,
    `display_order` INT NOT NULL DEFAULT 0,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_product_status_transition`
        PRIMARY KEY (`from_status_code`, `to_status_code`, `actor_kind`),
    KEY `ix_product_status_transition_to` (`to_status_code`),
    CONSTRAINT `ck_product_status_transition_distinct_states`
        CHECK (`from_status_code` <> `to_status_code`),
    CONSTRAINT `ck_product_status_transition_actor_kind`
        CHECK (`actor_kind` IN ('seller', 'moderator', 'system')),
    CONSTRAINT `ck_product_status_transition_requires_reason`
        CHECK (`requires_reason` IN (0, 1)),
    CONSTRAINT `ck_product_status_transition_display_order_non_negative`
        CHECK (`display_order` >= 0),
    CONSTRAINT `fk_product_status_transition_from`
        FOREIGN KEY (`from_status_code`) REFERENCES `product_status` (`status_code`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_product_status_transition_to`
        FOREIGN KEY (`to_status_code`) REFERENCES `product_status` (`status_code`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Allowed product lifecycle transitions by actor kind.';

CREATE TABLE IF NOT EXISTS `product` (
    `product_id` BIGINT NOT NULL AUTO_INCREMENT,
    `public_id` CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `seller_account_id` BIGINT NOT NULL,
    `category_id` BIGINT NOT NULL,
    `status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `canonical_slug` VARCHAR(160) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `title` VARCHAR(200) NOT NULL,
    `description` TEXT NULL,
    `unit_price_minor` BIGINT NOT NULL,
    `currency_code` CHAR(3) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `row_version` BIGINT NOT NULL DEFAULT 1,
    `submitted_utc` DATETIME(6) NULL,
    `retired_utc` DATETIME(6) NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_product` PRIMARY KEY (`product_id`),
    CONSTRAINT `uq_product_public_id` UNIQUE (`public_id`),
    CONSTRAINT `uq_product_category_canonical_slug` UNIQUE (`category_id`, `canonical_slug`),
    KEY `ix_product_public_latest` (`status_code`, `created_utc`),
    KEY `ix_product_public_catalogue` (`status_code`, `category_id`, `created_utc`),
    KEY `ix_product_seller_inventory` (`seller_account_id`, `status_code`, `updated_utc`),
    KEY `ix_product_seller_catalogue` (`seller_account_id`, `status_code`, `created_utc`),
    FULLTEXT KEY `ft_product_search` (`title`, `description`),
    CONSTRAINT `ck_product_public_id_length` CHECK (CHAR_LENGTH(`public_id`) = 36),
    CONSTRAINT `ck_product_slug_not_empty` CHECK (`canonical_slug` <> ''),
    CONSTRAINT `ck_product_title_not_empty` CHECK (`title` <> ''),
    CONSTRAINT `ck_product_unit_price_positive` CHECK (`unit_price_minor` > 0),
    CONSTRAINT `ck_product_currency_code_length` CHECK (CHAR_LENGTH(`currency_code`) = 3),
    CONSTRAINT `ck_product_row_version_positive` CHECK (`row_version` > 0),
    CONSTRAINT `fk_product_seller_account`
        FOREIGN KEY (`seller_account_id`) REFERENCES `account` (`account_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_product_category`
        FOREIGN KEY (`category_id`) REFERENCES `category` (`category_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_product_status`
        FOREIGN KEY (`status_code`) REFERENCES `product_status` (`status_code`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Seller-owned catalogue product with optimistic concurrency.';

CREATE TABLE IF NOT EXISTS `product_image` (
    `product_image_id` BIGINT NOT NULL AUTO_INCREMENT,
    `product_id` BIGINT NOT NULL,
    `storage_key` VARCHAR(512) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `content_type` VARCHAR(127) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `content_length_bytes` BIGINT NOT NULL,
    `width_pixels` INT NOT NULL,
    `height_pixels` INT NOT NULL,
    `display_order` INT NOT NULL DEFAULT 0,
    `is_primary` TINYINT(1) NOT NULL DEFAULT 0,
    `primary_for_product_id` BIGINT NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_product_image` PRIMARY KEY (`product_image_id`),
    CONSTRAINT `uq_product_image_storage_key` UNIQUE (`storage_key`),
    CONSTRAINT `uq_product_image_display_order` UNIQUE (`product_id`, `display_order`),
    CONSTRAINT `uq_product_image_primary_for_product` UNIQUE (`primary_for_product_id`),
    CONSTRAINT `ck_product_image_storage_key_not_empty` CHECK (`storage_key` <> ''),
    CONSTRAINT `ck_product_image_content_type_not_empty` CHECK (`content_type` <> ''),
    CONSTRAINT `ck_product_image_content_length_positive` CHECK (`content_length_bytes` > 0),
    CONSTRAINT `ck_product_image_width_positive` CHECK (`width_pixels` > 0),
    CONSTRAINT `ck_product_image_height_positive` CHECK (`height_pixels` > 0),
    CONSTRAINT `ck_product_image_display_order_non_negative` CHECK (`display_order` >= 0),
    CONSTRAINT `ck_product_image_is_primary` CHECK (`is_primary` IN (0, 1)),
    CONSTRAINT `ck_product_image_primary_marker`
        CHECK (
            (`is_primary` = 0 AND `primary_for_product_id` IS NULL)
            OR (
                `is_primary` = 1
                AND `primary_for_product_id` IS NOT NULL
                AND `primary_for_product_id` = `product_id`
            )
        ),
    CONSTRAINT `fk_product_image_product`
        FOREIGN KEY (`product_id`) REFERENCES `product` (`product_id`)
        ON DELETE CASCADE ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Validated product-image metadata and opaque storage keys.';

CREATE TABLE IF NOT EXISTS `shopping_cart` (
    `shopping_cart_id` BIGINT NOT NULL AUTO_INCREMENT,
    `account_id` BIGINT NULL,
    `guest_capability_hash` CHAR(64) CHARACTER SET ascii COLLATE ascii_bin NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),
    `last_activity_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_shopping_cart` PRIMARY KEY (`shopping_cart_id`),
    CONSTRAINT `uq_shopping_cart_account` UNIQUE (`account_id`),
    CONSTRAINT `uq_shopping_cart_guest_capability_hash` UNIQUE (`guest_capability_hash`),
    CONSTRAINT `ck_shopping_cart_guest_capability_hash_not_empty`
        CHECK (`guest_capability_hash` IS NULL OR `guest_capability_hash` <> ''),
    CONSTRAINT `ck_shopping_cart_guest_capability_hash_length`
        CHECK (`guest_capability_hash` IS NULL OR CHAR_LENGTH(`guest_capability_hash`) = 64),
    CONSTRAINT `ck_shopping_cart_single_owner`
        CHECK (
            (`account_id` IS NOT NULL AND `guest_capability_hash` IS NULL)
            OR (`account_id` IS NULL AND `guest_capability_hash` IS NOT NULL)
        ),
    CONSTRAINT `fk_shopping_cart_account`
        FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='One live cart per account or opaque guest capability.';

CREATE TABLE IF NOT EXISTS `cart_item` (
    `cart_item_id` BIGINT NOT NULL AUTO_INCREMENT,
    `shopping_cart_id` BIGINT NOT NULL,
    `product_id` BIGINT NOT NULL,
    `quantity` INT NOT NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_cart_item` PRIMARY KEY (`cart_item_id`),
    CONSTRAINT `uq_cart_item_cart_product` UNIQUE (`shopping_cart_id`, `product_id`),
    KEY `ix_cart_item_product` (`product_id`),
    CONSTRAINT `ck_cart_item_quantity_positive` CHECK (`quantity` > 0),
    CONSTRAINT `fk_cart_item_shopping_cart`
        FOREIGN KEY (`shopping_cart_id`) REFERENCES `shopping_cart` (`shopping_cart_id`)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    CONSTRAINT `fk_cart_item_product`
        FOREIGN KEY (`product_id`) REFERENCES `product` (`product_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Current cart quantities; product values are revalidated at checkout.';

CREATE TABLE IF NOT EXISTS `sales_order_status` (
    `status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `display_name` VARCHAR(64) NOT NULL,
    `display_order` INT NOT NULL,
    `is_terminal` TINYINT(1) NOT NULL,
    `is_payment_complete` TINYINT(1) NOT NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_sales_order_status` PRIMARY KEY (`status_code`),
    CONSTRAINT `uq_sales_order_status_display_name` UNIQUE (`display_name`),
    CONSTRAINT `ck_sales_order_status_code_not_empty` CHECK (`status_code` <> ''),
    CONSTRAINT `ck_sales_order_status_display_order_non_negative` CHECK (`display_order` >= 0),
    CONSTRAINT `ck_sales_order_status_is_terminal` CHECK (`is_terminal` IN (0, 1)),
    CONSTRAINT `ck_sales_order_status_is_payment_complete` CHECK (`is_payment_complete` IN (0, 1))
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Sales-order lifecycle state definitions.';

CREATE TABLE IF NOT EXISTS `sales_order` (
    `sales_order_id` BIGINT NOT NULL AUTO_INCREMENT,
    `public_id` CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `buyer_account_id` BIGINT NOT NULL,
    `status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `recipient_given_name` VARCHAR(100) NOT NULL,
    `recipient_family_name` VARCHAR(100) NOT NULL,
    `delivery_address_line1` VARCHAR(255) NOT NULL,
    `delivery_address_line2` VARCHAR(255) NULL,
    `delivery_locality` VARCHAR(100) NOT NULL,
    `delivery_region` VARCHAR(100) NULL,
    `delivery_postal_code` VARCHAR(32) NOT NULL,
    `delivery_country_code` CHAR(2) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `currency_code` CHAR(3) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `subtotal_minor` BIGINT NOT NULL,
    `total_minor` BIGINT NOT NULL,
    `paid_utc` DATETIME(6) NULL,
    `cancelled_utc` DATETIME(6) NULL,
    `refunded_utc` DATETIME(6) NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_sales_order` PRIMARY KEY (`sales_order_id`),
    CONSTRAINT `uq_sales_order_public_id` UNIQUE (`public_id`),
    CONSTRAINT `uq_sales_order_id_currency` UNIQUE (`sales_order_id`, `currency_code`),
    CONSTRAINT `uq_sales_order_id_currency_total`
        UNIQUE (`sales_order_id`, `currency_code`, `total_minor`),
    KEY `ix_sales_order_buyer_created` (`buyer_account_id`, `created_utc`),
    KEY `ix_sales_order_status_created` (`status_code`, `created_utc`),
    CONSTRAINT `ck_sales_order_public_id_length` CHECK (CHAR_LENGTH(`public_id`) = 36),
    CONSTRAINT `ck_sales_order_recipient_given_name_not_empty` CHECK (`recipient_given_name` <> ''),
    CONSTRAINT `ck_sales_order_recipient_family_name_not_empty` CHECK (`recipient_family_name` <> ''),
    CONSTRAINT `ck_sales_order_address_line1_not_empty` CHECK (`delivery_address_line1` <> ''),
    CONSTRAINT `ck_sales_order_locality_not_empty` CHECK (`delivery_locality` <> ''),
    CONSTRAINT `ck_sales_order_postal_code_not_empty` CHECK (`delivery_postal_code` <> ''),
    CONSTRAINT `ck_sales_order_country_code_length` CHECK (CHAR_LENGTH(`delivery_country_code`) = 2),
    CONSTRAINT `ck_sales_order_currency_code_length` CHECK (CHAR_LENGTH(`currency_code`) = 3),
    CONSTRAINT `ck_sales_order_subtotal_positive` CHECK (`subtotal_minor` > 0),
    CONSTRAINT `ck_sales_order_total_positive` CHECK (`total_minor` > 0),
    CONSTRAINT `fk_sales_order_buyer_account`
        FOREIGN KEY (`buyer_account_id`) REFERENCES `account` (`account_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_sales_order_status`
        FOREIGN KEY (`status_code`) REFERENCES `sales_order_status` (`status_code`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Buyer delivery snapshot and immutable purchase header.';

CREATE TABLE IF NOT EXISTS `sales_order_item` (
    `sales_order_item_id` BIGINT NOT NULL AUTO_INCREMENT,
    `sales_order_id` BIGINT NOT NULL,
    `product_id` BIGINT NULL,
    `seller_account_id` BIGINT NOT NULL,
    `seller_public_id` CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `seller_username` VARCHAR(64) NOT NULL,
    `product_public_id` CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `product_title` VARCHAR(200) NOT NULL,
    `product_image_storage_key` VARCHAR(512) CHARACTER SET ascii COLLATE ascii_bin NULL,
    `unit_price_minor` BIGINT NOT NULL,
    `quantity` INT NOT NULL,
    `line_total_minor` BIGINT NOT NULL,
    `currency_code` CHAR(3) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_sales_order_item` PRIMARY KEY (`sales_order_item_id`),
    CONSTRAINT `uq_sales_order_item_order_product` UNIQUE (`sales_order_id`, `product_public_id`),
    KEY `ix_sales_order_item_order_currency` (`sales_order_id`, `currency_code`),
    KEY `ix_sales_order_item_product` (`product_id`),
    KEY `ix_sales_order_item_seller_order` (`seller_account_id`, `sales_order_id`),
    CONSTRAINT `ck_sales_order_item_seller_public_id_length`
        CHECK (CHAR_LENGTH(`seller_public_id`) = 36),
    CONSTRAINT `ck_sales_order_item_seller_username_not_empty` CHECK (`seller_username` <> ''),
    CONSTRAINT `ck_sales_order_item_product_public_id_length`
        CHECK (CHAR_LENGTH(`product_public_id`) = 36),
    CONSTRAINT `ck_sales_order_item_product_title_not_empty` CHECK (`product_title` <> ''),
    CONSTRAINT `ck_sales_order_item_unit_price_positive` CHECK (`unit_price_minor` > 0),
    CONSTRAINT `ck_sales_order_item_quantity_positive` CHECK (`quantity` > 0),
    CONSTRAINT `ck_sales_order_item_line_total_positive` CHECK (`line_total_minor` > 0),
    CONSTRAINT `ck_sales_order_item_line_total_matches_unit_price`
        CHECK (`line_total_minor` = `unit_price_minor` * `quantity`),
    CONSTRAINT `ck_sales_order_item_currency_code_length` CHECK (CHAR_LENGTH(`currency_code`) = 3),
    CONSTRAINT `fk_sales_order_item_order_currency`
        FOREIGN KEY (`sales_order_id`, `currency_code`)
        REFERENCES `sales_order` (`sales_order_id`, `currency_code`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_sales_order_item_product`
        FOREIGN KEY (`product_id`) REFERENCES `product` (`product_id`)
        ON DELETE SET NULL ON UPDATE RESTRICT,
    CONSTRAINT `fk_sales_order_item_seller_account`
        FOREIGN KEY (`seller_account_id`) REFERENCES `account` (`account_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Immutable seller and product snapshots for a purchase line.';

CREATE TABLE IF NOT EXISTS `payment_attempt_status` (
    `status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `display_name` VARCHAR(64) NOT NULL,
    `display_order` INT NOT NULL,
    `is_terminal` TINYINT(1) NOT NULL,
    `is_successful` TINYINT(1) NOT NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_payment_attempt_status` PRIMARY KEY (`status_code`),
    CONSTRAINT `uq_payment_attempt_status_display_name` UNIQUE (`display_name`),
    CONSTRAINT `ck_payment_attempt_status_code_not_empty` CHECK (`status_code` <> ''),
    CONSTRAINT `ck_payment_attempt_status_display_order_non_negative` CHECK (`display_order` >= 0),
    CONSTRAINT `ck_payment_attempt_status_is_terminal` CHECK (`is_terminal` IN (0, 1)),
    CONSTRAINT `ck_payment_attempt_status_is_successful` CHECK (`is_successful` IN (0, 1))
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Payment-attempt lifecycle state definitions.';

CREATE TABLE IF NOT EXISTS `payment_attempt` (
    `payment_attempt_id` BIGINT NOT NULL AUTO_INCREMENT,
    `sales_order_id` BIGINT NOT NULL,
    `currency_code` CHAR(3) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `status_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `provider_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `attempt_number` INT NOT NULL,
    `provider_checkout_session_id` VARCHAR(255) CHARACTER SET ascii COLLATE ascii_bin NULL,
    `provider_payment_id` VARCHAR(255) CHARACTER SET ascii COLLATE ascii_bin NULL,
    `amount_minor` BIGINT NOT NULL,
    `provider_failure_code` VARCHAR(128) CHARACTER SET ascii COLLATE ascii_bin NULL,
    `provider_created_utc` DATETIME(6) NULL,
    `completed_utc` DATETIME(6) NULL,
    `created_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT `pk_payment_attempt` PRIMARY KEY (`payment_attempt_id`),
    CONSTRAINT `uq_payment_attempt_order_number` UNIQUE (`sales_order_id`, `attempt_number`),
    CONSTRAINT `uq_payment_attempt_checkout_session`
        UNIQUE (`provider_code`, `provider_checkout_session_id`),
    CONSTRAINT `uq_payment_attempt_provider_payment`
        UNIQUE (`provider_code`, `provider_payment_id`),
    KEY `ix_payment_attempt_order_currency_amount`
        (`sales_order_id`, `currency_code`, `amount_minor`),
    KEY `ix_payment_attempt_status_created` (`status_code`, `created_utc`),
    CONSTRAINT `ck_payment_attempt_provider_code_not_empty` CHECK (`provider_code` <> ''),
    CONSTRAINT `ck_payment_attempt_attempt_number_positive` CHECK (`attempt_number` > 0),
    CONSTRAINT `ck_payment_attempt_amount_positive` CHECK (`amount_minor` > 0),
    CONSTRAINT `ck_payment_attempt_currency_code_length` CHECK (CHAR_LENGTH(`currency_code`) = 3),
    CONSTRAINT `fk_payment_attempt_order_currency_amount`
        FOREIGN KEY (`sales_order_id`, `currency_code`, `amount_minor`)
        REFERENCES `sales_order` (`sales_order_id`, `currency_code`, `total_minor`)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT `fk_payment_attempt_status`
        FOREIGN KEY (`status_code`) REFERENCES `payment_attempt_status` (`status_code`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='One provider payment attempt for a pending sales order.';

CREATE TABLE IF NOT EXISTS `payment_webhook_event` (
    `payment_webhook_event_id` BIGINT NOT NULL AUTO_INCREMENT,
    `payment_attempt_id` BIGINT NULL,
    `provider_code` VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `provider_event_id` VARCHAR(255) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `event_type` VARCHAR(128) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `processing_status` VARCHAR(16) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `payload_sha256` CHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    `received_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `verified_utc` DATETIME(6) NULL,
    `applied_utc` DATETIME(6) NULL,

    CONSTRAINT `pk_payment_webhook_event` PRIMARY KEY (`payment_webhook_event_id`),
    CONSTRAINT `uq_payment_webhook_event_provider_event`
        UNIQUE (`provider_code`, `provider_event_id`),
    KEY `ix_payment_webhook_event_attempt` (`payment_attempt_id`),
    KEY `ix_payment_webhook_event_status_received` (`processing_status`, `received_utc`),
    CONSTRAINT `ck_payment_webhook_event_provider_code_not_empty`
        CHECK (`provider_code` <> ''),
    CONSTRAINT `ck_payment_webhook_event_provider_event_id_not_empty`
        CHECK (`provider_event_id` <> ''),
    CONSTRAINT `ck_payment_webhook_event_type_not_empty` CHECK (`event_type` <> ''),
    CONSTRAINT `ck_payment_webhook_event_processing_status`
        CHECK (`processing_status` IN ('received', 'verified', 'rejected', 'applied', 'failed')),
    CONSTRAINT `ck_payment_webhook_event_payload_sha256_length`
        CHECK (CHAR_LENGTH(`payload_sha256`) = 64),
    CONSTRAINT `ck_payment_webhook_event_verified_after_received`
        CHECK (`verified_utc` IS NULL OR `verified_utc` >= `received_utc`),
    CONSTRAINT `ck_payment_webhook_event_applied_after_received`
        CHECK (`applied_utc` IS NULL OR `applied_utc` >= `received_utc`),
    CONSTRAINT `ck_payment_webhook_event_applied_is_verified`
        CHECK (
            `processing_status` <> 'applied'
            OR (
                `verified_utc` IS NOT NULL
                AND `applied_utc` IS NOT NULL
                AND `applied_utc` >= `verified_utc`
            )
        ),
    CONSTRAINT `fk_payment_webhook_event_payment_attempt`
        FOREIGN KEY (`payment_attempt_id`) REFERENCES `payment_attempt` (`payment_attempt_id`)
        ON DELETE RESTRICT ON UPDATE RESTRICT
)
ENGINE=InnoDB
DEFAULT CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci
COMMENT='Verified provider webhook receipt and idempotency record.';
