-- Development reference data. Run as shopio_migrator against shop_io.

SET SESSION time_zone = '+00:00';

START TRANSACTION;

INSERT INTO `account_role` (
    `role_code`,
    `display_name`,
    `display_order`,
    `is_privileged`
)
VALUES
    ('administrator', 'Administrator', 10, 1),
    ('moderator', 'Moderator', 20, 1);

INSERT INTO `product_status` (
    `status_code`,
    `display_name`,
    `display_order`,
    `is_public`,
    `allows_seller_edit`,
    `is_terminal`
)
VALUES
    ('draft', 'Draft', 10, 0, 1, 0),
    ('waitingapproval', 'Waiting approval', 20, 0, 0, 0),
    ('active', 'Active', 30, 1, 0, 0),
    ('deleted', 'Deleted', 40, 0, 0, 1);

INSERT INTO `product_status_transition` (
    `from_status_code`,
    `to_status_code`,
    `actor_kind`,
    `requires_reason`,
    `display_order`
)
VALUES
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
    ('active', 'deleted', 'moderator', 1, 110);

INSERT INTO `sales_order_status` (
    `status_code`,
    `display_name`,
    `display_order`,
    `is_terminal`,
    `is_payment_complete`
)
VALUES
    ('pending_payment', 'Pending payment', 10, 0, 0),
    ('paid', 'Paid', 20, 0, 1),
    ('payment_failed', 'Payment failed', 30, 0, 0),
    ('cancelled', 'Cancelled', 40, 1, 0),
    ('refunded', 'Refunded', 50, 1, 0);

INSERT INTO `payment_attempt_status` (
    `status_code`,
    `display_name`,
    `display_order`,
    `is_terminal`,
    `is_successful`
)
VALUES
    ('pending', 'Pending', 10, 0, 0),
    ('succeeded', 'Succeeded', 20, 1, 1),
    ('failed', 'Failed', 30, 1, 0),
    ('cancelled', 'Cancelled', 40, 1, 0),
    ('expired', 'Expired', 50, 1, 0);

INSERT INTO `category` (
    `title`,
    `canonical_slug`,
    `display_order`,
    `is_active`
)
VALUES
    ('Computers', 'computers', 10, 1),
    ('Smartphones', 'smartphones', 20, 1),
    ('Electronics', 'electronics', 30, 1);

COMMIT;
