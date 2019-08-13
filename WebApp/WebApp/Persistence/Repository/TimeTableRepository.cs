using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class TimeTableRepository : Repository<Timetable, Guid>, ITimeTableRepository
    {
        public TimeTableRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<Timetable> GetAll()
        {
            return context.Set<Timetable>().Include("Line").ToList();
        }

        new public Timetable Get(Guid id)
        {
            return context.Set<Timetable>().Include("Line").FirstOrDefault(x => x.Id == id);
        }
    }
}