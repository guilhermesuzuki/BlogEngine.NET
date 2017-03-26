using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Settings")]
    internal class Settings
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [MaxLength(50)]
        public string SettingName { get; set; }

        public string SettingValue { get; set; }
    }
}
