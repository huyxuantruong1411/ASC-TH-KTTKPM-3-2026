using ASC.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASC.Business.Interfaces
{
    public interface IServiceRequestMessageOperations
    {
        Task CreateServiceRequestMessageAsync(ServiceRequestMessage message);
        Task<List<ServiceRequestMessage>> GetServiceRequestMessageAsync(string serviceRequestId);
    }
}