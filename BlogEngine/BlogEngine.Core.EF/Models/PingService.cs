using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_PingService")]
    internal class PingService
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PingServiceID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [MaxLength(255)]
        public string Link { get; set; }
    }
}
