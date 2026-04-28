using ASC.Business.Interfaces;
using ASC.Utilities;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASC.Web.Hubs
{
    public class ServiceMessagesHub : Hub
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOnlineUsersOperations _onlineUserOperations;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public ServiceMessagesHub(
            UserManager<IdentityUser> userManager,
            IOnlineUsersOperations onlineUserOperations,
            IConfiguration configuration,
            ApplicationDbContext dbContext) // [FIX] Inject thẳng DbContext vào đây
        {
            _userManager = userManager;
            _onlineUserOperations = onlineUserOperations;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var serviceRequestId = httpContext?.Request.Query["serviceRequestId"].ToString();

            if (!string.IsNullOrWhiteSpace(serviceRequestId))
            {
                var email = Context.User.GetCurrentUserDetails()?.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    await _onlineUserOperations.CreateOnlineUserAsync(email);
                    await UpdateServiceRequestClients(serviceRequestId);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var serviceRequestId = httpContext?.Request.Query["serviceRequestId"].ToString();

            if (!string.IsNullOrWhiteSpace(serviceRequestId))
            {
                var email = Context.User.GetCurrentUserDetails()?.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    await _onlineUserOperations.DeleteOnlineUserAsync(email);
                    await UpdateServiceRequestClients(serviceRequestId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateServiceRequestClients(string serviceRequestId)
        {
            var serviceRequest = await _dbContext.ServiceRequests.FirstOrDefaultAsync(r => r.RowKey == serviceRequestId);

            if (serviceRequest == null) return;

            // [FIX]: Đổi cấu hình thành "AppSettings" giống file json
            var adminEmail = _configuration["AppSettings:AdminEmail"] ?? "admin@abc.com";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            var customerUser = await _userManager.FindByEmailAsync(serviceRequest.PartitionKey);

            IdentityUser engineerUser = null;
            if (!string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer))
            {
                engineerUser = await _userManager.FindByEmailAsync(serviceRequest.ServiceEngineer);
            }

            var isAdminOnline = await _onlineUserOperations.GetOnlineUserAsync(adminEmail);
            var isCustomerOnline = await _onlineUserOperations.GetOnlineUserAsync(serviceRequest.PartitionKey);
            var isServiceEngineerOnline = !string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer)
                && await _onlineUserOperations.GetOnlineUserAsync(serviceRequest.ServiceEngineer);

            List<string> userIds = new List<string>();
            if (isAdminOnline && adminUser != null) userIds.Add(adminUser.Id);
            if (isServiceEngineerOnline && engineerUser != null) userIds.Add(engineerUser.Id);
            if (isCustomerOnline && customerUser != null) userIds.Add(customerUser.Id);

            await Clients.Users(userIds).SendAsync("online", new
            {
                isAd = isAdminOnline,
                isSe = isServiceEngineerOnline,
                isCu = isCustomerOnline
            });
        }
    }
}