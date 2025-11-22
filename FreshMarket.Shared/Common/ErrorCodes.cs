namespace FreshMarket.Shared.Common;

/// <summary>
/// Central catalog of application error codes used in API and service responses.
/// Keep codes stable; add new ones instead of renaming old.
/// </summary>
public static class ErrorCodes
{
    public static class Common
    {
        public const string UNKNOWN = "COMMON_UNKNOWN";
        public const string VALIDATION = "COMMON_VALIDATION";
        public const string NOT_FOUND = "COMMON_NOT_FOUND";
        public const string CONFLICT = "COMMON_CONFLICT";
    }

    public static class System
    {
        public const string UNEXPECTED = "SYSTEM_UNEXPECTED";
        public const string MAINTENANCE = "SYSTEM_MAINTENANCE";
        public const string CONFIG_INVALID = "SYSTEM_CONFIG_INVALID";
    }

    public static class Authentication
    {
        public const string INVALID_CREDENTIALS = "AUTH_INVALID_CREDENTIALS";
        public const string EMAIL_NOT_FOUND = "AUTH_EMAIL_NOT_FOUND";
        public const string PASSWORD_INCORRECT = "AUTH_PASSWORD_INCORRECT";
        public const string ACCOUNT_LOCKED = "AUTH_ACCOUNT_LOCKED";
        public const string ACCOUNT_DISABLED = "AUTH_ACCOUNT_DISABLED";
        public const string TOKEN_INVALID = "AUTH_TOKEN_INVALID";
        public const string TOKEN_EXPIRED = "AUTH_TOKEN_EXPIRED";
        public const string TWO_FACTOR_REQUIRED = "AUTH_TWO_FACTOR_REQUIRED";
    }

    public static class Authorization
    {
        public const string FORBIDDEN = "AUTHZ_FORBIDDEN";
        public const string ROLE_REQUIRED = "AUTHZ_ROLE_REQUIRED";
        public const string PERMISSION_DENIED = "AUTHZ_PERMISSION_DENIED";
    }

    public static class Validation
    {
        public const string INVALID_INPUT = "VALIDATION_INVALID_INPUT";
        public const string MISSING_REQUIRED_FIELD = "VALIDATION_MISSING_REQUIRED_FIELD";
        public const string FORMAT_ERROR = "VALIDATION_FORMAT_ERROR";
        public const string RANGE_ERROR = "VALIDATION_RANGE_ERROR";
        public const string DUPLICATE_VALUE = "VALIDATION_DUPLICATE_VALUE";
    }

    public static class Resource
    {
        public const string NOT_FOUND = "RESOURCE_NOT_FOUND";
        public const string ALREADY_EXISTS = "RESOURCE_ALREADY_EXISTS";
        public const string DELETION_FAILED = "RESOURCE_DELETION_FAILED";
    }

    public static class Order
    {
        public const string NOT_FOUND = "ORDER_NOT_FOUND";
        public const string INVALID_STATUS = "ORDER_INVALID_STATUS";
        public const string CREATE_FAILED = "ORDER_CREATE_FAILED";
        public const string CANCEL_FAILED = "ORDER_CANCEL_FAILED";
    }

    public static class Payment
    {
        public const string METHOD_UNSUPPORTED = "PAYMENT_METHOD_UNSUPPORTED";
        public const string GATEWAY_DECLINED = "PAYMENT_GATEWAY_DECLINED";
        public const string INSUFFICIENT_FUNDS = "PAYMENT_INSUFFICIENT_FUNDS";
        public const string CAPTURE_FAILED = "PAYMENT_CAPTURE_FAILED";
        public const string REFUND_FAILED = "PAYMENT_REFUND_FAILED";
    }

    public static class Inventory
    {
        public const string OUT_OF_STOCK = "INVENTORY_OUT_OF_STOCK";
        public const string LOW_STOCK = "INVENTORY_LOW_STOCK";
        public const string RESERVE_FAILED = "INVENTORY_RESERVE_FAILED";
    }

    public static class Coupon
    {
        public const string INVALID = "COUPON_INVALID";
        public const string EXPIRED = "COUPON_EXPIRED";
        public const string ALREADY_USED = "COUPON_ALREADY_USED";
        public const string NOT_APPLICABLE = "COUPON_NOT_APPLICABLE";
        public const string USAGE_LIMIT_REACHED = "COUPON_USAGE_LIMIT_REACHED";
    }

    public static class User
    {
        public const string NOT_FOUND = "USER_NOT_FOUND";
        public const string EMAIL_IN_USE = "USER_EMAIL_IN_USE";
        public const string UPDATE_FAILED = "USER_UPDATE_FAILED";
        public const string PASSWORD_POLICY_VIOLATION = "USER_PASSWORD_POLICY_VIOLATION";
    }
}
