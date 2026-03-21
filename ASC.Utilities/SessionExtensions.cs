using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ASC.Utilities
{
    public static class SessionExtensions
    {
        // Ghi dữ liệu vào Session dưới dạng chuỗi JSON
        public static void SetSession<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // Đọc dữ liệu từ Session và giải mã JSON về lại Object
        public static T? GetSession<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}