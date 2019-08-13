using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Bus
    {
        [Key]
        [Required]
        public string Id { get; set; }
        [Required]
        public double X { get; set; }
        [Required]
        public double Y { get; set; }
        public string LineId { get; set; }
        public Line Line { get; set; }
    }
}