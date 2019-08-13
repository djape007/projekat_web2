using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebApp.Models;
using WebApp.Persistence;
using WebApp.Persistence.UnitOfWork;

namespace WebApp.Controllers
{
    public class BusStopsOnLinesController : ApiController
    {
        private IUnitOfWork unitOfWork;
        public BusStopsOnLinesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/BusStopsOnLines
        public IEnumerable<BusStopsOnLine> GetBusStopsOnLines()
        {
            return unitOfWork.BusStopsOnLines.GetAll();
        }

        // GET: api/BusStopsOnLines/5
        [ResponseType(typeof(BusStopsOnLine))]
        public IHttpActionResult GetBusStopsOnLine(Guid id)
        {
            BusStopsOnLine busStopsOnLine = unitOfWork.BusStopsOnLines.Get(id);
            if (busStopsOnLine == null)
            {
                return NotFound();
            }

            return Ok(busStopsOnLine);
        }

        // PUT: api/BusStopsOnLines/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PutBusStopsOnLine(Guid id, BusStopsOnLine busStopsOnLine)
        {
            if (!ModelState.IsValid || busStopsOnLine == null)
            {
                return BadRequest(ModelState);
            }

            if (id != busStopsOnLine.Id)
            {
                return BadRequest();
            }

            try
            {
                unitOfWork.BusStopsOnLines.Update(busStopsOnLine);
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BusStopsOnLineExists(id))
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

        // POST: api/BusStopsOnLines
        [ResponseType(typeof(BusStopsOnLine))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PostBusStopsOnLine(BusStopsOnLine busStopsOnLine)
        {
            if (!ModelState.IsValid || busStopsOnLine == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                unitOfWork.BusStopsOnLines.Add(busStopsOnLine);
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (BusStopsOnLineExists(busStopsOnLine.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = busStopsOnLine.Id }, busStopsOnLine);
        }

        // DELETE: api/BusStopsOnLines/5
        [ResponseType(typeof(BusStopsOnLine))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeleteBusStopsOnLine(Guid id)
        {
            BusStopsOnLine busStopsOnLine = unitOfWork.BusStopsOnLines.Get(id);
            if (busStopsOnLine == null)
            {
                return NotFound();
            }

            unitOfWork.BusStopsOnLines.Remove(busStopsOnLine);
            unitOfWork.Complete();

            return Ok(busStopsOnLine);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BusStopsOnLineExists(Guid id)
        {
            return unitOfWork.BusStopsOnLines.Find(e => e.Id == id).Count() > 0;
        }
    }
}