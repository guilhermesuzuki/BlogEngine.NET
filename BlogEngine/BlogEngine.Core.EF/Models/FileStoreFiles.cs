using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_FileStoreFiles")]
    internal class FileStoreFiles
    {
        [Key]
        public Guid FileID { get; set; }

        [Required]
        public Guid ParentDirectoryID { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required, MaxLength(255)]
        public string FullPath { get; set; }

        [Required, MaxLength, DataType("varbinary")]
        public byte[] Contents { get; set; }

        [Required]
        public int Size { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        public DateTime LastAccess { get; set; }

        [Required]
        public DateTime LastModify { get; set; }
    }
}
