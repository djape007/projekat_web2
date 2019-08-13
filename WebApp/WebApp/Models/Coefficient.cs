using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Coefficient
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public Double Value { get; set; }

    }
}