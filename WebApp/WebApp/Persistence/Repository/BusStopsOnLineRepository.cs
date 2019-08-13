using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class BusStopsOnLineRepository : Repository<Models.BusStopsOnLine, Guid>, IBusStopsOnLineRepository
    {
        public BusStopsOnLineRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<BusStopsOnLine> GetAll()
        {
            return context.Set<BusStopsOnLine>().Include("Line").Include("BusStop").ToList();
        }

        new public BusStopsOnLine Get(Guid id)
        {
            return context.Set<BusStopsOnLine>().Include("Line").Include("BusStop").FirstOrDefault(x => x.Id == id);
        }
    }
}