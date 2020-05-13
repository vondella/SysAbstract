using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sys.Web.Models
{
    public class Audit
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime DateTime { get; set; }
        public String Username { get; set; }
        [Required]
        [MaxLength(128)]
        public String TableName { get; set; }
        [Required]
        [MaxLength(50)]
        public String Action { get; set; }
        public String KeyValues { get; set; }
        public String OldValues { get; set; }
        public String NewValues { get; set; }
    }
}
