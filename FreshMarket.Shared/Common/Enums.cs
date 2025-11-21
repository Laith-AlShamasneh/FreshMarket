using System.ComponentModel;

namespace FreshMarket.Shared.Common;

public enum Lang
{
    Ar = 1,
    En = 2
}

public enum HttpResponseStatus
{
    [Description("OK")] OK = 200,
    [Description("Created")] Created = 201,
    [Description("Bad Request")] BadRequest = 400,
    [Description("Unauthorized")] Unauthorized = 401,
    [Description("Forbidden")] Forbidden = 403,
    [Description("Not Found")] DataNotFound = 404,
    [Description("Internal Server Error")] InternalServerError = 500
}

public enum MessageType
{
    SaveSuccessfully, SaveFailed,
    UpdateSuccessfully, UpdateFailed,
    DeleteSuccessfully, DeleteFailed,
    RetrieveSuccessfully, RetrieveFailed,
    NoDataFound,

    ActiveSuccessfully, ActiveFailed,
    DeactiveSuccessfully, DeactiveFailed,

    UserLoginSuccess, InvalidUserLogin,
    PasswordIncorrect, EmailNotFound,
    LoginAllowedLimitFailed,

    ChangePasswordSuccessfully, ChangePasswordFailed,
    ResetPasswordSuccessfully, ResetPasswordFailed,

    DontHavePermission,

    OrderPlacedSuccessfully,
    PaymentSuccess, PaymentFailed,
    OutOfStock, LowStockWarning,
    CouponApplied, InvalidCoupon,

    SystemProblem
}

public enum LoginFailureReason
{
    [Description("Invalid email or password")] InvalidCredentials = 1,
    [Description("Email not confirmed")] EmailNotConfirmed = 2,
    [Description("Account locked")] AccountLocked = 3,
    [Description("Account disabled")] AccountDisabled = 4,
    [Description("Password expired")] PasswordExpired = 5,
    [Description("Invalid 2FA")] InvalidTwoFactorCode = 6,
    [Description("User not found")] UserNotFound = 7,
    [Description("System error")] SystemError = 99
}

public enum SortDirection
{
    Ascending = 1,
    Descending = 2
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Returned = 7,
    Refunded = 8,
    Failed = 9
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4,
    PartiallyRefunded = 5,
    Cancelled = 6
}

/// <summary>
/// Represents the available payment methods for customer orders
/// </summary>
public enum PaymentMethodType
{
    CreditCard = 1,
    DebitCard = 2,
    CashOnDelivery = 3,
    DigitalWallet = 4,
    BankTransfer = 5,
    Check = 6
}

/// <summary>
/// Represents the available shipping methods for order delivery
/// </summary>
public enum ShippingMethodType
{
    Standard = 1,
    Express = 2,
    Overnight = 3,
    SameDay = 4,
    InStorePickup = 5,
    Courier = 6
}