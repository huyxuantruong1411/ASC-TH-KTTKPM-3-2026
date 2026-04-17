using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Configuration.Models
{
    public class MasterDataValueViewModel
    {
        // Thêm ? để cho phép null khi Create
        public string? RowKey { get; set; }

        [Required]
        [Display(Name = "Partition Key")]
        public string PartitionKey { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}