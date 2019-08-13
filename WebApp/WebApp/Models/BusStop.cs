using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BusStop
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public double X { get; set; }
        [Required]
        public double Y { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<BusStopsOnLine> BusStopsOnLines { get; set; }


    }
}