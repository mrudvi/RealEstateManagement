using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateManagement.Models
{
    public class Propertyvideo
    {
        [Key]
        public int Id { get; set; }

        public int PropertyId { get; set; }

        public string VideoPath { get; set; } = string.Empty;

        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }
    }
}