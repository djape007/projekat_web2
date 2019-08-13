using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class BusRepository : Repository<Bus,string>, IBusRepository
    {
        public BusRepository(System.Data.Entity.DbContext context) : base(context) { }

        new public IEnumerable<Bus> GetAll()
        {
            return context.Set<Bus>().Include("Line").ToList();
        }

        new public Bus Get(string id)
        {
            return context.Set<Bus>().Include("Line").FirstOrDefault(x => x.Id == id);
        }
    }
}