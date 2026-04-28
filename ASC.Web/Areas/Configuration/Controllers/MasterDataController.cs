using System.ComponentModel;
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);
            // Hold all Master Keys in session
            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);
            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel == null ? new List<MasterDataKeyViewModel>() : masterKeysViewModel.ToList(),
                IsEdit = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel model)
        {
            model.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys")
                               ?? new List<MasterDataKeyViewModel>();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(model.MasterKeyInContext!);

            if (model.IsEdit)
            {
                await _masterData.UpdateMasterKeyAsync(model.MasterKeyInContext!.PartitionKey!, masterKey);
            }
            else
            {
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = masterKey.Name;
                await _masterData.InsertMasterKeyAsync(masterKey);
            }
            return RedirectToAction("MasterKeys");
        }

        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            // Get all Master Keys and hold them in ViewBag for Select tag
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();
            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),

                // FIX LỖI TIỀM ẨN: Phải khởi tạo object này, nếu không file View cshtml 
                // khi gọi asp-for="MasterValueInContext.RowKey" sẽ bị Crash do Null Reference
                MasterValueInContext = new MasterDataValueViewModel(),

                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            // Get Master values based on master key.
            return Json(new { data = await _masterData.GetAllMasterValuesByKeyAsync(key) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid)
            {
                return Json("Error");
            }

            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
            if (isEdit)
            {
                await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey!, masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                // Lưu ý: Nếu GetCurrentUserDetails() hoặc Name có thể null, hãy đảm bảo fallback (vd: ?? "System")
                masterDataValue.CreatedBy = HttpContext.User.GetCurrentUserDetails()?.Name ?? "Admin";
                await _masterData.InsertMasterValueAsync(masterDataValue);
            }
            return Json(true);
        }

        private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();
            using (var memoryStream = new MemoryStream())
            {
                await excelFile.CopyToAsync(memoryStream);

                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelPackage.License.SetNonCommercialPersonal("ASC Project");

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var masterDataValue = new MasterDataValue();
                        masterDataValue.RowKey = Guid.NewGuid().ToString();
                        masterDataValue.PartitionKey = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty;
                        masterDataValue.Name = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty;
                        masterDataValue.IsActive = Boolean.Parse(worksheet.Cells[row, 3].Value?.ToString() ?? "false");
                        masterValueList.Add(masterDataValue);
                    }
                }
            }
            return masterValueList;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var files = Request.Form.Files;

            if (!files.Any())
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var excelFile = files.First();
            if (excelFile.Length <= 0)
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var masterData = await ParseMasterDataExcel(excelFile);
            var result = await _masterData.UploadBulkMasterData(masterData);

            return Json(new { Success = result });
        }
    }
}