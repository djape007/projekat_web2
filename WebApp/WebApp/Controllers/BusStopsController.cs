using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebApp.Models;
using WebApp.Persistence;
using WebApp.Persistence.UnitOfWork;

namespace WebApp.Controllers
{
    public class BusStopsController : ApiController
    {
        private IUnitOfWork unitOfWork;
        public BusStopsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/BusStops
        public IEnumerable<BusStop> GetBusStops()
        {
            return unitOfWork.BusStops.GetAll();
        }

        // GET: api/BusStops/5
        [ResponseType(typeof(BusStop))]
        public IHttpActionResult GetBusStop(Guid id)
        {
            BusStop busStop = unitOfWork.BusStops.Get(id);
            if (busStop == null)
            {
                return NotFound();
            }

            string rawValue = busStop.Id + busStop.Name + busStop.X + busStop.Y + busStop.BusStopsOnLines.Count();
            string eTag = ComputeEtag(rawValue);
            HttpContext.Current.Response.AddHeader("Access-Control-Expose-Headers", "etag");
            HttpContext.Current.Response.Headers.Add("etag", eTag);

            return Ok(busStop);
        }

        // PUT: api/BusStops/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PutBusStop(Guid id, BusStop busStop)
        {
            if (!ModelState.IsValid || busStop == null)
            {
                return BadRequest(ModelState);
            }

            if (id != busStop.Id)
            {
                return BadRequest();
            }

            try
            {
                string eTag = HttpContext.Current.Request.Headers.Get("etag");

                BusStop db_busStop = unitOfWork.BusStops.Get(id);
                string rawValue = db_busStop.Id + db_busStop.Name + db_busStop.X.ToString("#.#####") + db_busStop.Y.ToString("#.#####") + db_busStop.BusStopsOnLines.Count();
                string db_eTag = ComputeEtag(rawValue);

                if (eTag != db_eTag)
                {
                    return StatusCode(HttpStatusCode.PreconditionFailed);
                }

                db_busStop.Name = busStop.Name;
                db_busStop.X = busStop.X;
                db_busStop.Y = busStop.Y;

                if (busStop.Address.Trim().Length > 0) {
                    var LinijeNaTrenutnojStanici = unitOfWork.BusStopsOnLines.Find(x => x.BusStopId == id);
                    HashSet<string> postojeceVeze = new HashSet<string>(LinijeNaTrenutnojStanici.Select(x => x.LineId.ToString()));
                    HashSet<string> noveVezeHS = new HashSet<string>(busStop.Address.Trim().Split(','));

                    var obrisaneVeze = postojeceVeze.Except(noveVezeHS);
                    var noveVeze = noveVezeHS.Except(postojeceVeze);
                    
                    unitOfWork.BusStopsOnLines.RemoveRange(
                    unitOfWork.BusStopsOnLines.Find(x => x.BusStopId == id && obrisaneVeze.Contains(x.LineId)));
                    unitOfWork.Complete();

                    //var noveVeze = busStop.Address.Trim().Split(',');
                    foreach(string veza in noveVeze) {
                        unitOfWork.BusStopsOnLines.Add(new BusStopsOnLine() {
                            Id = Guid.NewGuid(),
                            BusStopId = id,
                            LineId = veza
                        });
                    }
                }
                
                unitOfWork.BusStops.Update(db_busStop);
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BusStopExists(id))
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

        // POST: api/BusStops
        [ResponseType(typeof(BusStop))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PostBusStop(BusStop busStop)
        {
            if (!ModelState.IsValid || busStop == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                unitOfWork.BusStops.Add(busStop);
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (BusStopExists(busStop.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = busStop.Id }, busStop);
        }

        // DELETE: api/BusStops/5
        [ResponseType(typeof(BusStop))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeleteBusStop(Guid id)
        {
            BusStop busStop = unitOfWork.BusStops.Get(id);
            if (busStop == null)
            {
                return NotFound();
            }

            unitOfWork.BusStops.Remove(busStop);
            unitOfWork.Complete();

            return Ok(busStop);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BusStopExists(Guid id)
        {
            return unitOfWork.BusStops.Find(e => e.Id == id).Count() > 0;
        }

        private string ComputeEtag(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}