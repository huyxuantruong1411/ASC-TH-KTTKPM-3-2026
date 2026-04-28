using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class OnlineUsersOperations : IOnlineUsersOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public OnlineUsersOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateOnlineUserAsync(string email)
        {
            // [FIX]: Đã xóa using (_unitOfWork) gây lỗi ObjectDisposedException
            var userList = await _unitOfWork.Repository<OnlineUser>().FindAllByPartitionKeyAsync(email);
            if (userList.Any())
            {
                var updateUser = userList.FirstOrDefault();
                updateUser!.IsDeleted = false;
                _unitOfWork.Repository<OnlineUser>().Update(updateUser);
            }
            else
            {
                await _unitOfWork.Repository<OnlineUser>().AddAsync(new OnlineUser(email) { IsDeleted = false });
            }
            _unitOfWork.CommitTransaction();
        }

        public async Task DeleteOnlineUserAsync(string email)
        {
            // [FIX]: Đã xóa using (_unitOfWork) gây lỗi ObjectDisposedException
            var userList = await _unitOfWork.Repository<OnlineUser>().FindAllByPartitionKeyAsync(email);
            if (userList.Any())
            {
                var userToDelete = userList.FirstOrDefault();
                userToDelete!.IsDeleted = true; // Soft delete
                _unitOfWork.Repository<OnlineUser>().Update(userToDelete);
            }
            _unitOfWork.CommitTransaction();
        }

        public async Task<bool> GetOnlineUserAsync(string email)
        {
            var userList = await _unitOfWork.Repository<OnlineUser>().FindAllByPartitionKeyAsync(email);
            return userList.Any() && userList.FirstOrDefault()!.IsDeleted == false;
        }
    }
}