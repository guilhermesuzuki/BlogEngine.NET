using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_PostNotify")]
    internal class PostNotify
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostNotifyID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required]
        public Guid PostID { get; set; }

        [MaxLength(255)]
        public string NotifyAddress { get; set; }
    }
}
