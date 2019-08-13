using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApp.Persistence.Repository
{
    public class UserRepository : Repository<Models.ApplicationUser, string>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<Models.ApplicationUser> GetAll()
        {
            return context.Set<Models.ApplicationUser>().ToList();
        }

        new public Models.ApplicationUser Get(string id)
        {
            return context.Set<Models.ApplicationUser>().FirstOrDefault(x => x.Id == id);
        }
    }
}