using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Packages")]
    internal class Packages
    {
        [Key, MaxLength(128)]
        public string PackageId { get; set; }

        [Required, MaxLength(128)]
        public string Version { get; set; }
    }
}
