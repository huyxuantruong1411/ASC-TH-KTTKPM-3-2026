using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ASC.Web.Hubs
{
    // Bắt buộc phải đăng nhập mới được chat
    [Authorize]
    public class ChatHub : Hub
    {
        // Hàm này sẽ được Client gọi
        public async Task SendMessage(string message)
        {
            // Lấy tên/email của user đang đăng nhập hiện tại từ Identity
            var user = Context.User?.Identity?.Name ?? "Ẩn danh";

            // Gửi lại cho toàn bộ Client đang kết nối với sự kiện "ReceiveMessage"
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}