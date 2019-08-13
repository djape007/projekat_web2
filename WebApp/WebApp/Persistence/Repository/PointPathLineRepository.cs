using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class PointPathLineRepository : Repository<Models.PointPathLine, Guid>, IPointPathLineRepository
    {
        public PointPathLineRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<PointPathLine> GetAll()
        {
            return context.Set<PointPathLine>().Include("Line").ToList();
        }

        new public PointPathLine Get(Guid id)
        {
            return context.Set<PointPathLine>().Include("Line").FirstOrDefault(x => x.Id == id);
        }
    }
}