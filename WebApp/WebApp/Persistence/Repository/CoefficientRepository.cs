using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApp.Persistence.Repository
{
    public class CoefficientRepository : Repository<Models.Coefficient, Guid>, ICoefficientRepository
    {
        public CoefficientRepository(DbContext context) : base(context)
        {
        }
    }
}