using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using WebApp.Models;
using WebApp.Persistence.UnitOfWork;

namespace WebApp.Controllers
{
    public class BusesController : ApiController {
        private IUnitOfWork unitOfWork;
        private Hubs.BusesPositionsHub hub;

        public BusesController(IUnitOfWork unitOfWork, Hubs.BusesPositionsHub hub) {
            this.unitOfWork = unitOfWork;
            this.hub = hub;
        }

        // GET: api/Buses
        public IEnumerable<Bus> GetBuses()
        {
            return unitOfWork.Buses.GetAll();
        }
        
        [Route("api/Buses/WithLine")]
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<Bus> GetBusesWithLine() {
            return unitOfWork.Buses.GetAll().Where(x => x.LineId != null);
        }

        // GET: api/Buses/5
        [ResponseType(typeof(Bus))]
        public IHttpActionResult GetBus(string id)
        {
            Bus bus = unitOfWork.Buses.Get(id);
            if (bus == null)
            {
                return NotFound();
            }

            return Ok(bus);
        }

        [ResponseType(typeof(void))]
        [Route("api/Buses/UpdatePosition")]
        [HttpPost]
        [AllowAnonymous]
        //public IHttpActionResult UpdateBusesPosition([ModelBinder(typeof(BusPosition))]BusPosition busevi) {
        public IHttpActionResult UpdateBusesPosition() {
            var request = System.Web.HttpContext.Current.Request;
            string data = GetDocumentContents(request);

            //SLANJE NA WEBSOCKET
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Hubs.BusesPositionsHub>();
            hubContext.Clients.All.newPositions(data);//odma saljem svim klijentima koji slusaju na WS
            //pa nek oni sami parsiraju podatke. 
            //u sustini, ovo ispod ni nije potrebno

            foreach (string busData in data.Split('|'))
            {
                //busData => NS335XY,45.45453,19.345435,12A
                var busDataArray = busData.Split(',');
                Bus bus = unitOfWork.Buses.Get(busDataArray[0]);

                bus.X = double.Parse(busDataArray[1]);
                bus.Y = double.Parse(busDataArray[2]);
                bus.LineId = busDataArray[3];
                unitOfWork.Buses.Update(bus);
            }
            unitOfWork.Complete();

            return Ok();
        }

        private string GetDocumentContents(System.Web.HttpRequest Request) {
            string documentContents;
            using (Stream receiveStream = Request.InputStream) {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8)) {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }

        // PUT: api/Buses/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBus(string id, Bus bus)
        {
            if (!ModelState.IsValid || bus == null)
            {
                return BadRequest(ModelState);
            }

            if (id != bus.Id)
            {
                return BadRequest();
            }

            try
            {
                unitOfWork.Buses.Update(bus);
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BusExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Buses
        [ResponseType(typeof(Bus))]
        public IHttpActionResult PostBus(Bus bus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                unitOfWork.Buses.Add(bus);
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (BusExists(bus.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = bus.Id }, bus);
        }

        // DELETE: api/Buses/5
        [ResponseType(typeof(Bus))]
        public IHttpActionResult DeleteBus(string id)
        {
            Bus bus = unitOfWork.Buses.Get(id);
            if (bus == null)
            {
                return NotFound();
            }

            unitOfWork.Buses.Remove(bus);
            unitOfWork.Complete();

            return Ok(bus);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BusExists(string id)
        {
            return unitOfWork.Buses.Find(e => e.Id == id).Count() > 0;
        }
    }
}