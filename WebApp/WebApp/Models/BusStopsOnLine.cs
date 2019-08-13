using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BusStopsOnLine
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid BusStopId { get; set; }
        public BusStop BusStop { get; set; }
        [Required]
        public string LineId { get; set; }
        public Line Line { get; set; }

    }
}