using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Referrers")]
    internal class Referrers
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReferrerRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [Required]
        public Guid ReferrerId { get; set; }

        [Required]
        public DateTime ReferralDay { get; set; }

        [Required, MaxLength(255)]
        public string ReferrerUrl { get; set; }

        [Required]
        public int ReferralCount { get; set; }

        [MaxLength(255)]
        public string Url { get; set; }

        public bool? IsSpam { get; set; }
    }
}
