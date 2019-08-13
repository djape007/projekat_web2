using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class BusStopRepository : Repository<Models.BusStop, Guid>, IBusStopRepository
    {
        public BusStopRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<BusStop> GetAll()
        {
            return context.Set<BusStop>().Include("BusStopsOnLines").ToList();
        }

        new public BusStop Get(Guid id)
        {
            return context.Set<BusStop>().Include("BusStopsOnLines").FirstOrDefault(x => x.Id == id);
        }
    }
}