using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Unity;
using WebApp.Persistence.Repository;

namespace WebApp.Persistence.UnitOfWork
{
    public class DemoUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
      
        public DemoUnitOfWork(DbContext context)
        {
            _context = context;
        }

        [Dependency]
        public IBusRepository Buses { get; set; }
        [Dependency]
        public ILineRepository Lines { get; set; }
        [Dependency]
        public IPriceHistoryRepository PriceHistories { get; set; }
        [Dependency]
        public IPricelistRepository Pricelists { get; set; }
        [Dependency]
        public IProductTypeRepository ProductTypes { get; set; }
        [Dependency]
        public ITimeTableRepository TimeTables { get; set; }
        [Dependency]
        public IBusStopRepository BusStops { get; set; }
        [Dependency]
        public IBusStopsOnLineRepository BusStopsOnLines { get; set; }
        [Dependency]
        public ICoefficientRepository Coefficients { get; set; }
        [Dependency]
        public IPointPathLineRepository PointPathLines { get; set; }
        [Dependency]
        public ISoldTicketRepository SoldTickets { get; set; }
        [Dependency]
        public IUserRepository Users { get; set; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}