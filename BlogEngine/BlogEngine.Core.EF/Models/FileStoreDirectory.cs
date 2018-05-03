using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_FileStoreDirectory")]
    internal class FileStoreDirectory
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? ParentID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required, MaxLength(1000)]
        public string FullPath { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        public DateTime LastAccess { get; set; }

        [Required]
        public DateTime LastModify { get; set; }
    }
}
