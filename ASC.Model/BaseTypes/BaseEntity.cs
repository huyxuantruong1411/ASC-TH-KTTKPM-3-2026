using System;
using System.ComponentModel.DataAnnotations;

namespace ASC.Models.BaseTypes
{
    public abstract class BaseEntity
    {
        // Nhóm các entity lại với nhau (đóng vai trò như category/phân loại trong SQL)
        public string? PartitionKey { get; set; }

        // Khóa chính của bảng
        [Key]
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public bool IsDeleted { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}