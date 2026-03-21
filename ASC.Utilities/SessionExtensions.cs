using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ASC.Utilities
{
    public static class SessionExtensions
    {
        public static void SetSession(this ISession session, string key, object value)
        {
            // Thêm kiểm tra null cho value trước khi serialize nếu cần thiết
            session.Set(key, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value) ?? string.Empty));
        }

        // Đổi T thành T? để cho phép trả về null nếu không tìm thấy key trong session
        public static T? GetSession<T>(this ISession session, string key)
        {
            // Khai báo byte[]? để đánh dấu value có thể null
            if (session.TryGetValue(key, out byte[]? value) && value != null)
            {
                return JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(value));
            }

            return default;
        }
    }
}