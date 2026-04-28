using ASC.Models.BaseTypes;
using System;

namespace ASC.Model.Models
{
    public class OnlineUser : BaseEntity
    {
        public OnlineUser() { }

        public OnlineUser(string email)
        {
            this.RowKey = Guid.NewGuid().ToString();
            // Lưu email vào PartitionKey để tiện truy vấn
            this.PartitionKey = email;
        }
    }
}