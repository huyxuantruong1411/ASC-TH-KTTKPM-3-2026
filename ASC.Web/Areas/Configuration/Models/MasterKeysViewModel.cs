namespace ASC.Web.Areas.Configuration.Models
{
    public class MasterKeysViewModel
    {
        // Thêm ? để cho phép null
        public List<MasterDataKeyViewModel>? MasterKeys { get; set; }

        public MasterDataKeyViewModel? MasterKeyInContext { get; set; }
        public bool IsEdit { get; set; }
    }
}