namespace ASC.Web.Areas.Configuration.Models
{
    public class MasterValuesViewModel
    {
        // Thêm ? để cho phép null
        public List<MasterDataValueViewModel>? MasterValues { get; set; }

        public MasterDataValueViewModel MasterValueInContext { get; set; }
        public bool IsEdit { get; set; }
    }
}