using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_DataStoreSettings")]
    internal class DataStoreSettings
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DataStoreSettingRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [Required, MaxLength(50)]
        public string ExtensionType { get; set; }

        [Required, MaxLength(100)]
        public string ExtensionId { get; set; }

        [Required, MaxLength]
        public string Settings { get; set; }
    }
}
