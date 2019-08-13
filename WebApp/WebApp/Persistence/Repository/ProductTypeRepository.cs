using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class ProductTypeRepository : Repository<ProductType, Guid>, IProductTypeRepository
    {
        public ProductTypeRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<ProductType> GetAll()
        {
            return context.Set<ProductType>().Include("PriceHistories").ToList();
        }

        new public ProductType Get(Guid id)
        {
            return context.Set<ProductType>().Include("PriceHistories").FirstOrDefault(x => x.Id == id);
        }
    }
}