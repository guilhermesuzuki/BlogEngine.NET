using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_QuickSettings")]
    internal class QuickSettings
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuickSettingID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; }

        [Required, MaxLength(255)]
        public string SettingName { get; set; }

        [Required, MaxLength(255)]
        public string SettingValue { get; set; }
    }
}
