using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Profiles")]
    internal class Profiles
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProfileID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [MaxLength(100)]
        public string UserName { get; set; }

        [MaxLength(200)]
        public string SettingName { get; set; }

        [MaxLength]
        public string SettingValue { get; set; }
    }
}
