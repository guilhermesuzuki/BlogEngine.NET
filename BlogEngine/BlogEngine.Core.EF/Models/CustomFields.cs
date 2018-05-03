using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_CustomFields")]
    internal class CustomFields
    {
        [Column(Order = 1)]
        [Required, MaxLength(25), Key]
        public string CustomType { get; set; }

        [Column(Order = 2)]
        [Required, MaxLength(100), Key]
        public string ObjectId { get; set; }

        [Column(Order = 3)]
        [Required, Key]
        public Guid BlogId { get; set; }

        [Column(Order = 4)]
        [Required, MaxLength(150), Key]
        public string Key { get; set; }

        public string Value { get; set; }

        [MaxLength(250)]
        public string Attribute { get; set; }
    }
}
