using Microsoft.AspNetCore.Identity;

namespace ASC.Web.Areas.Accounts.Models
{
    public class ServiceEngineerViewModel
    {
        public List<IdentityUser>? ServiceEngineers { get; set; } //Lưu trữ danh sách nhân viên

        // Fix CS8618: Khởi tạo giá trị mặc định
        public ServiceEngineerRegistrationViewModel Registration { get; set; } = new ServiceEngineerRegistrationViewModel(); // Lưu trữ nhân viên thêm mới hoặc cập nhật
    }
}