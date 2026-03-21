using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.BaseTypes;
using ASC.Models.BaseTypes;

namespace ASC.Model.Models
{
    public class ServiceRequest : BaseEntity, IAuditTracker
    {
        public ServiceRequest() { }

        public ServiceRequest(string email)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = email;
        }

        public string VehicleName { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RequestedServices { get; set; } = string.Empty;
        public DateTime? RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string ServiceEngineer { get; set; } = string.Empty;
    }
}
