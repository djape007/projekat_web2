using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class PricelistRepository : Repository<Pricelist, Guid>, IPricelistRepository
    {
        public PricelistRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<Pricelist> GetAll()
        {
            return context.Set<Pricelist>().Include("PriceHistories").ToList();
        }

        new public Pricelist Get(Guid id)
        {
            return context.Set<Pricelist>().Include("PriceHistories").FirstOrDefault(x => x.Id == id);
        }
    }
}