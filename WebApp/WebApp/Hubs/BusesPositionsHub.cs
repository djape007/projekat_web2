using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WebApp.Persistence.UnitOfWork;

namespace WebApp.Hubs {

    [HubName("busPositions")]
    public class BusesPositionsHub : Hub {

        private static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<BusesPositionsHub>();
        
        //VRLO JE MOGUCE DA MOZE BEZ OVOGA
        /*public void SendBusesPositions(string newPositions) {
            Clients.All.newPositions(newPositions);
        }*/
    }
}