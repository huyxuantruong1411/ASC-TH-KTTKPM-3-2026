using ASC.Models.BaseTypes;
using System;

namespace ASC.Model.Models
{
    public class ServiceRequestMessage : BaseEntity
    {
        public ServiceRequestMessage() { }

        public ServiceRequestMessage(string serviceRequestId)
        {
            this.RowKey = Guid.NewGuid().ToString();
            // PartitionKey ở đây sẽ lưu ServiceRequestId để biết tin nhắn thuộc về Request nào
            this.PartitionKey = serviceRequestId;
        }

        public string FromDisplayName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? MessageDate { get; set; }
    }
}