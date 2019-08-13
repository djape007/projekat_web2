using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using WebApp.Models;

namespace WebApp.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Pricelist> Pricelists { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Line> Lines { get; set; }
        public DbSet<Timetable> Timetables { get; set; }
        public DbSet<BusStop> BusStops { get; set; }
        public DbSet<BusStopsOnLine> BusStopsOnLines { get; set; }
        public DbSet<Coefficient> Coefficients { get; set; }
        public DbSet<SoldTicket> SoldTickets { get; set; }
        public DbSet<PointPathLine> PointPathLines { get; set; }
        //public DbSet<ApplicationUser> Userss { get; set; }


        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            this.Configuration.LazyLoadingEnabled = true;
            base.Database.CommandTimeout = 120;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


    }
}