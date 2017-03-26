using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_FileStoreFileThumbs")]
    internal class FileStoreFileThumbs
    {
        [Key, Required]
        public Guid thumbnailId { get; set; }

        [Required]
        public Guid FileId { get; set; }

        [Required]
        public int Size { get; set; }

        [Required, MaxLength, DataType("varbinary")]
        public Byte[] Contents { get; set; }
    }
}
