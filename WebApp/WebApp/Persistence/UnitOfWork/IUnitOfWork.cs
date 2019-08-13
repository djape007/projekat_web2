using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Persistence.Repository;

namespace WebApp.Persistence.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IBusRepository Buses { get; set; }
        ILineRepository Lines { get; set; }
        IPriceHistoryRepository PriceHistories { get; set; }
        IPricelistRepository Pricelists { get; set; }
        IProductTypeRepository ProductTypes { get; set; }
        ITimeTableRepository TimeTables { get; set; }
        IBusStopRepository BusStops { get; set; }
        IBusStopsOnLineRepository BusStopsOnLines { get; set; }
        ICoefficientRepository Coefficients { get; set; }
        IPointPathLineRepository PointPathLines { get; set; }
        ISoldTicketRepository SoldTickets { get; set; }
        IUserRepository Users { get; set; }

        int Complete();
    }
}
