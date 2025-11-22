namespace FreshMarket.Shared.Common;

public static class Messages
{
    private static readonly Dictionary<MessageType, Dictionary<Lang, string>> _messages = new()
    {
        // CRUD
        [MessageType.SaveSuccessfully] = new() { [Lang.Ar] = "تم حفظ المعلومات بنجاح", [Lang.En] = "Data saved successfully" },
        [MessageType.SaveFailed] = new() { [Lang.Ar] = "حدثت مشكلة، لم يتم حفظ البيانات", [Lang.En] = "Failed to save data" },

        [MessageType.UpdateSuccessfully] = new() { [Lang.Ar] = "تم تعديل المعلومات بنجاح", [Lang.En] = "Data updated successfully" },
        [MessageType.UpdateFailed] = new() { [Lang.Ar] = "حدثت مشكلة، لم يتم تعديل البيانات", [Lang.En] = "Failed to update data" },

        [MessageType.DeleteSuccessfully] = new() { [Lang.Ar] = "تم حذف المعلومات بنجاح", [Lang.En] = "Data deleted successfully" },
        [MessageType.DeleteFailed] = new() { [Lang.Ar] = "حدثت مشكلة، لم يتم حذف البيانات", [Lang.En] = "Failed to delete data" },

        [MessageType.RetrieveSuccessfully] = new() { [Lang.Ar] = "تم جلب البيانات بنجاح", [Lang.En] = "Data retrieved successfully" },
        [MessageType.RetrieveFailed] = new() { [Lang.Ar] = "حدثت مشكلة، لم يتم جلب البيانات", [Lang.En] = "Failed to retrieve data" },

        [MessageType.NoDataFound] = new() { [Lang.Ar] = "لا توجد بيانات", [Lang.En] = "No data found" },


        // Authentication
        [MessageType.UserLoginSuccess] = new() { [Lang.Ar] = "تم تسجيل الدخول بنجاح", [Lang.En] = "Logged in successfully" },
        [MessageType.InvalidUserLogin] = new() { [Lang.Ar] = "اسم المستخدم أو كلمة المرور غير صحيحة", [Lang.En] = "Invalid username or password" },
        [MessageType.PasswordIncorrect] = new() { [Lang.Ar] = "كلمة المرور غير صحيحة", [Lang.En] = "Incorrect password" },
        [MessageType.EmailNotFound] = new() { [Lang.Ar] = "البريد الإلكتروني غير موجود", [Lang.En] = "Email not found" },

        // Permissions
        [MessageType.DontHavePermission] = new() { [Lang.Ar] = "ليس لديك صلاحية لتنفيذ هذا الإجراء", [Lang.En] = "You do not have permission to perform this action" },

        // Orders & Payments
        [MessageType.OrderPlacedSuccessfully] = new() { [Lang.Ar] = "تم إنشاء الطلب بنجاح", [Lang.En] = "Order placed successfully" },
        [MessageType.PaymentFailed] = new() { [Lang.Ar] = "فشل في عملية الدفع", [Lang.En] = "Payment failed" },
        [MessageType.PaymentSuccess] = new() { [Lang.Ar] = "تم الدفع بنجاح", [Lang.En] = "Payment successful" },

        // Stock
        [MessageType.OutOfStock] = new() { [Lang.Ar] = "المنتج نفد من المخزون", [Lang.En] = "Product is out of stock" },

        // Coupons
        [MessageType.CouponApplied] = new() { [Lang.Ar] = "تم تطبيق الكوبون بنجاح", [Lang.En] = "Coupon applied successfully" },
        [MessageType.InvalidCoupon] = new() { [Lang.Ar] = "كود الكوبون غير صالح", [Lang.En] = "Invalid coupon code" },

        // System
        [MessageType.SystemProblem] = new() { [Lang.Ar] = "حدث خطأ في النظام، يرجى المحاولة لاحقاً أو التواصل مع الإدارة", [Lang.En] = "A system error occurred. Please try again later or contact support." },
    };

    public static string Get(MessageType type, Lang lang = Lang.En)
        => _messages.TryGetValue(type, out var dict) && dict.TryGetValue(lang, out var msg)
            ? msg
            : _messages[MessageType.SystemProblem][Lang.En];
}