using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_PackageFiles")]
    internal class PackageFiles
    {
        [Column(Order = 1)]
        [Key, MaxLength(128)]
        public string PackageId { get; set; }

        [Required]
        public int FileOrder { get; set; }

        [Column(Order = 2)]
        [Key, MaxLength(255)]
        public string FilePath { get; set; }

        [Required]
        public bool IsDirectory { get; set; }
    }
}
