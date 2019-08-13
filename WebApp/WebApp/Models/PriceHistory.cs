using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class PriceHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PricelistId { get; set; }
        public Pricelist Pricelist { get; set; }
        public Guid ProductTypeId { get; set; }
        public ProductType ProductType { get; set; }
        public Double Price { get; set; }
    }
}