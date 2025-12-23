using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace YenMay_web.Utilities
{
    public static class SessionHelper
    {
        // 1. Lưu object bất kỳ vào Session
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // 2. Lấy object bất kỳ từ Session
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        // 3. Quản lý CartSessionId (FIXED VERSION)
        public static string GetOrCreateCartSessionId(this HttpContext context)
        {
            const string key = "CART_SESSION_ID";

            try
            {
                // ⚠️ QUAN TRỌNG: Kiểm tra Session có null không
                if (context?.Session == null)
                {
                    throw new InvalidOperationException(
                        "Session is null. Make sure app.UseSession() is configured in Program.cs " +
                        "and placed BEFORE app.UseAuthentication()."
                    );
                }

                var sessionId = context.Session.GetString(key);

                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToString();
                    context.Session.SetString(key, sessionId);

                    // Log để debug
                    Console.WriteLine($"✅ Created new session ID: {sessionId}");
                }
                else
                {
                    Console.WriteLine($"✅ Retrieved existing session ID: {sessionId}");
                }

                return sessionId;
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                Console.WriteLine($"❌ Error in GetOrCreateCartSessionId: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public static bool HasCartSession(this HttpContext? context)
        {
            if (context?.Session == null)
                return false;

            const string key = "CART_SESSION_ID";
            return !string.IsNullOrEmpty(context.Session.GetString(key));
        }

        public static void ClearCartSession(this HttpContext? context)
        {
            if (context?.Session == null)
                return;

            const string key = "CART_SESSION_ID";
            context.Session.Remove(key);
        }
    }
}