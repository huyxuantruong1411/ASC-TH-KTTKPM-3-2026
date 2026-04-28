using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class ServiceRequestMessageOperations : IServiceRequestMessageOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestMessageOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestMessageAsync(ServiceRequestMessage message)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequestMessage>().AddAsync(message);
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task<List<ServiceRequestMessage>> GetServiceRequestMessageAsync(string serviceRequestId)
        {
            // Tìm tất cả tin nhắn theo serviceRequestId (được lưu trong PartitionKey)
            var serviceRequestMessages = await _unitOfWork.Repository<ServiceRequestMessage>()
                .FindAllByPartitionKeyAsync(serviceRequestId);

            return serviceRequestMessages.ToList();
        }
    }
}