using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Blogs")]
    internal class Blogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [Required, MaxLength(255)]
        public string BlogName { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Hostname { get; set; } = string.Empty;

        [Required]
        public bool IsAnyTextBeforeHostnameAccepted { get; set; }

        [Required, MaxLength(255)]
        public string StorageContainerName { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string VirtualPath { get; set; } = string.Empty;

        [Required]
        public bool IsPrimary { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool IsSiteAggregation { get; set; }
    }
}
