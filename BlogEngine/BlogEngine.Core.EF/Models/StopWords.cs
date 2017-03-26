using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_StopWords")]
    internal class StopWords
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StopWordRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [MaxLength(50), Required]
        public string StopWord { get; set; }
    }
}
