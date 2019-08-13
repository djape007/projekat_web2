using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class PointPathLine
    {
        [Key]
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int SequenceNumber { get; set; }
        public string LineId { get; set; }
        public Line Line { get; set; }

    }
}