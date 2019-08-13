using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Line
    {
        [Key]
        public string Id { get; set; }
        public string Direction { get; set; }
        public ICollection<Bus> Buses { get; set; }
        public ICollection<BusStopsOnLine> BusStopsOnLines { get; set; }
        public ICollection<PointPathLine> PointLinePaths { get; set; }
    }
}