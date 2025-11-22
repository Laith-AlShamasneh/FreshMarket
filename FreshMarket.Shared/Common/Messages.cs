using System.Collections.ObjectModel;

namespace FreshMarket.Shared.Common;

public static class Messages
{
    private static readonly IReadOnlyDictionary<MessageType, IReadOnlyDictionary<Lang, string>> _messages =
        new ReadOnlyDictionary<MessageType, IReadOnlyDictionary<Lang, string>>(
            new Dictionary<MessageType, IReadOnlyDictionary<Lang, string>>
            {
                // ───────── CRUD ─────────
                [MessageType.SaveSuccessfully] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم حفظ المعلومات بنجاح", [Lang.En] = "Data saved successfully" }),
                [MessageType.SaveFailed] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "حدثت مشكلة، لم يتم حفظ البيانات", [Lang.En] = "Failed to save data" }),
                [MessageType.UpdateSuccessfully] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم تعديل المعلومات بنجاح", [Lang.En] = "Data updated successfully" }),
                [MessageType.UpdateFailed] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "حدثت مشكلة، لم يتم تعديل البيانات", [Lang.En] = "Failed to update data" }),
                [MessageType.DeleteSuccessfully] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم حذف المعلومات بنجاح", [Lang.En] = "Data deleted successfully" }),
                [MessageType.DeleteFailed] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "حدثت مشكلة، لم يتم حذف البيانات", [Lang.En] = "Failed to delete data" }),
                [MessageType.RetrieveSuccessfully] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم جلب البيانات بنجاح", [Lang.En] = "Data retrieved successfully" }),
                [MessageType.RetrieveFailed] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "حدثت مشكلة، لم يتم جلب البيانات", [Lang.En] = "Failed to retrieve data" }),
                [MessageType.NoDataFound] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "لا توجد بيانات", [Lang.En] = "No data found" }),

                // ───────── Auth ─────────
                [MessageType.UserLoginSuccess] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم تسجيل الدخول بنجاح", [Lang.En] = "Logged in successfully" }),
                [MessageType.InvalidUserLogin] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "اسم المستخدم أو كلمة المرور غير صحيحة", [Lang.En] = "Invalid username or password" }),
                [MessageType.PasswordIncorrect] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "كلمة المرور غير صحيحة", [Lang.En] = "Incorrect password" }),
                [MessageType.EmailNotFound] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "البريد الإلكتروني غير موجود", [Lang.En] = "Email not found" }),

                // ───────── Order / Payment / Inventory ─────────
                [MessageType.OrderPlacedSuccessfully] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم إنشاء الطلب بنجاح", [Lang.En] = "Order placed successfully" }),
                [MessageType.PaymentSuccess] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم الدفع بنجاح", [Lang.En] = "Payment succeeded" }),
                [MessageType.PaymentFailed] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "فشل الدفع", [Lang.En] = "Payment failed" }),
                [MessageType.OutOfStock] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "المنتج غير متوفر في المخزون", [Lang.En] = "Product out of stock" }),

                // ───────── Coupon ─────────
                [MessageType.CouponApplied] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "تم تطبيق القسيمة بنجاح", [Lang.En] = "Coupon applied successfully" }),
                [MessageType.InvalidCoupon] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "القسيمة غير صالحة", [Lang.En] = "Invalid coupon" }),

                // ───────── System ─────────
                [MessageType.SystemProblem] = new ReadOnlyDictionary<Lang, string>(new Dictionary<Lang, string> { [Lang.Ar] = "مشكلة في النظام", [Lang.En] = "System error occurred" }),
            });

    private const string FallbackMessageEn = "Message not found";
    private const string FallbackMessageAr = "لم يتم العثور على الرسالة";

    /// <summary>
    /// Gets a localized message for the given type and language. Falls back to English, then a default string.
    /// </summary>
    public static string Get(MessageType type, Lang lang = Lang.En)
    {
        if (!_messages.TryGetValue(type, out var localized))
            return lang == Lang.Ar ? FallbackMessageAr : FallbackMessageEn;

        if (localized.TryGetValue(lang, out var msg))
            return msg;

        // Fallback to English if Arabic missing (or vice versa)
        if (localized.TryGetValue(Lang.En, out var en))
            return en;

        // Final fallback
        return lang == Lang.Ar ? FallbackMessageAr : FallbackMessageEn;
    }

    /// <summary>
    /// Gets a message and replaces tokens (e.g. ("OrderNumber", "12345") in text "Order {OrderNumber} placed").
    /// Token format: {Key}
    /// </summary>
    public static string Get(MessageType type, Lang lang, params (string Key, string Value)[] tokens)
    {
        var baseMessage = Get(type, lang);
        if (tokens is null || tokens.Length == 0) return baseMessage;

        foreach (var (key, value) in tokens)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            baseMessage = baseMessage.Replace($"{{{key}}}", value ?? string.Empty);
        }
        return baseMessage;
    }
}