namespace FreshMarket.Domain.Common;

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

public enum PaymentMethodType
{
    CreditCard = 1,
    DebitCard = 2,
    CashOnDelivery = 3,
    DigitalWallet = 4,
    BankTransfer = 5
}

public enum ShippingMethodType
{
    Standard = 1,
    Express = 2,
    Overnight = 3,
    SameDay = 4,
    InStorePickup = 5,
    Courier = 6
}