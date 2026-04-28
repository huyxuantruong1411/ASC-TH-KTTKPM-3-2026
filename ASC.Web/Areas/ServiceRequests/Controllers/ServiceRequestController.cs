using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Controllers;
using ASC.Web.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using static ASC.Models.BaseTypes.Constants;
using ASC.Utilities;
using Microsoft.AspNetCore.SignalR;
using ASC.Web.Hubs;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore; // [FIX]: Bắt buộc phải có để dùng FirstOrDefaultAsync
using Microsoft.Extensions.DependencyInjection; // [FIX]: Bắt buộc để dùng hàm GetRequiredService()
using Microsoft.AspNetCore.Identity; // [THÊM MỚI]: Dùng cho Identity User
using Microsoft.Extensions.Configuration; // [THÊM MỚI]: Lấy cấu hình AdminEmail

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;

        private readonly IServiceRequestMessageOperations _serviceRequestMessageOperations;
        private readonly IHubContext<ServiceMessagesHub> _signalRHubContext;

        public ServiceRequestController(
            IServiceRequestOperations operations,
            IMapper mapper,
            IMasterDataCacheOperations masterData,
            IServiceRequestMessageOperations serviceRequestMessageOperations,
            IHubContext<ServiceMessagesHub> signalRHubContext)
        {
            _serviceRequestOperations = operations;
            _mapper = mapper;
            _masterData = masterData;
            _serviceRequestMessageOperations = serviceRequestMessageOperations;
            _signalRHubContext = signalRHubContext;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterData.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
                return View(request);
            }

            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);
            serviceRequest.PartitionKey = HttpContext.User.GetCurrentUserDetails()!.Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = Status.New.ToString();
            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);
            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });

            // Dùng IUnitOfWork để lấy ra Service Request (vì IServiceRequestOperations của bạn chưa có hàm GetByRowKey)
            var dbContext = HttpContext.RequestServices.GetRequiredService<ASC.Web.Data.ApplicationDbContext>();

            // [ĐÃ SỬA LỖI Ở ĐÂY]: Thay thế FindAsync(id) bằng FirstOrDefaultAsync() vì có khóa chính kép
            var request = await dbContext.ServiceRequests.FirstOrDefaultAsync(r => r.RowKey == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // ==========================================
        // CODE MỚI THÊM TỪ ĐÂY
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> ServiceRequestMessages(string serviceRequestId)
        {
            var messages = await _serviceRequestMessageOperations.GetServiceRequestMessageAsync(serviceRequestId);
            // Sắp xếp giảm dần theo thời gian
            return Json(messages.OrderByDescending(p => p.MessageDate));
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequestMessage([FromBody] ServiceRequestMessage message)
        {
            // PartitionKey ở đây chính là ServiceRequestId
            if (string.IsNullOrWhiteSpace(message.Message) || string.IsNullOrWhiteSpace(message.PartitionKey))
                return Json(false);

            // Gán thông vị trí hiện tại
            var userDetails = HttpContext.User.GetCurrentUserDetails();
            message.FromEmail = userDetails!.Email;
            message.FromDisplayName = userDetails.Name;
            message.MessageDate = DateTime.UtcNow;
            message.RowKey = Guid.NewGuid().ToString();

            // Lưu tin nhắn xuống DB (Entity Framework)
            await _serviceRequestMessageOperations.CreateServiceRequestMessageAsync(message);

            // Phát tin nhắn qua SignalR cho tất cả Client.
            await _signalRHubContext.Clients.All.SendAsync("publishMessage", message);

            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> MarkOfflineUser(
            [FromServices] IHubContext<ServiceMessagesHub> hubContext,
            [FromServices] IOnlineUsersOperations onlineUserOps,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] IConfiguration configuration)
        {
            var serviceRequestId = HttpContext.Request.Headers["ServiceRequestId"].ToString();
            var email = HttpContext.User.GetCurrentUserDetails()?.Email;

            if (!string.IsNullOrEmpty(email))
            {
                await onlineUserOps.DeleteOnlineUserAsync(email);

                if (!string.IsNullOrWhiteSpace(serviceRequestId))
                {
                    var dbContext = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                    var serviceRequest = await dbContext.ServiceRequests.FirstOrDefaultAsync(r => r.RowKey == serviceRequestId);

                    if (serviceRequest != null)
                    {
                        // [FIX LỖI CHÍNH Ở ĐÂY]: Đổi "ApplicationSettings" thành "AppSettings"
                        var adminEmail = configuration["AppSettings:AdminEmail"] ?? "admin@abc.com";

                        var adminUser = await userManager.FindByEmailAsync(adminEmail);
                        var customerUser = await userManager.FindByEmailAsync(serviceRequest.PartitionKey);

                        IdentityUser engineerUser = null;
                        if (!string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer))
                        {
                            engineerUser = await userManager.FindByEmailAsync(serviceRequest.ServiceEngineer);
                        }

                        var isAdminOnline = await onlineUserOps.GetOnlineUserAsync(adminEmail);
                        var isCustomerOnline = await onlineUserOps.GetOnlineUserAsync(serviceRequest.PartitionKey);
                        var isServiceEngineerOnline = !string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer) && await onlineUserOps.GetOnlineUserAsync(serviceRequest.ServiceEngineer);

                        var onlineUserIds = new System.Collections.Generic.List<string>();
                        if (isAdminOnline && adminUser != null) onlineUserIds.Add(adminUser.Id);
                        if (isCustomerOnline && customerUser != null) onlineUserIds.Add(customerUser.Id);
                        if (isServiceEngineerOnline && engineerUser != null) onlineUserIds.Add(engineerUser.Id);

                        await hubContext.Clients.Users(onlineUserIds).SendAsync("online", new
                        {
                            isAd = isAdminOnline,
                            isSe = isServiceEngineerOnline,
                            isCu = isCustomerOnline
                        });
                    }
                }
            }

            return Json(true);
        }
    }
}