using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class PriceHistoryRepository : Repository<PriceHistory, Guid>, IPriceHistoryRepository
    {
        public PriceHistoryRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<PriceHistory> GetAll()
        {
            return context.Set<PriceHistory>().Include("Pricelist").Include("ProductType").ToList();
        }

        new public PriceHistory Get(Guid id)
        {
            return context.Set<PriceHistory>().Include("Pricelist").Include("ProductType").FirstOrDefault(x => x.Id == id);
        }
    }
}