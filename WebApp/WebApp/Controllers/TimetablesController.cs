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
    public class TimetablesController : ApiController
    {
        IUnitOfWork unitOfWork;

        public TimetablesController(IUnitOfWork unitOfWork) {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/Timetables
        public IEnumerable<Timetable> GetTimetables()
        {
            return unitOfWork.TimeTables.GetAll();
        }

        // GET: api/Timetables/5
        [ResponseType(typeof(Timetable))]
        public IHttpActionResult GetTimetable(Guid id)
        {
            Timetable timetable = unitOfWork.TimeTables.Get(id);
            if (timetable == null)
            {
                return NotFound();
            }

            return Ok(timetable);
        }

        // PUT: api/Timetables/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PutTimetable(Guid id, Timetable timetable)
        {
            if (!ModelState.IsValid || timetable == null)
            {
                return BadRequest(ModelState);
            }

            if (id != timetable.Id)
            {
                return BadRequest();
            }

            string eTag = HttpContext.Current.Request.Headers.Get("etag");

            Timetable db_timetable = unitOfWork.TimeTables.Get(timetable.Id);
            if (db_timetable == null)
                return NotFound();

            string rawValue = db_timetable.Id + db_timetable.LineId + db_timetable.Times;
            string db_eTag = ComputeEtag(rawValue);

            if (eTag != db_eTag)
            {
                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            db_timetable.Times = timetable.Times;
            db_timetable.LineId = timetable.LineId;
            unitOfWork.TimeTables.Update(db_timetable);

            try
            {
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimetableExists(id))
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

        // POST: api/Timetables
        [ResponseType(typeof(Timetable))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PostTimetable(Timetable timetable)
        {
            if (!ModelState.IsValid || timetable == null)
            {
                return BadRequest(ModelState);
            }

            timetable.ValidFrom = DateTime.Now;

            unitOfWork.TimeTables.Add(timetable);

            try
            {
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (TimetableExists(timetable.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = timetable.Id }, timetable);
        }

        // DELETE: api/Timetables/5
        [ResponseType(typeof(Timetable))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeleteTimetable(Guid id)
        {
            Timetable timetable = unitOfWork.TimeTables.Get(id);
            if (timetable == null)
            {
                return NotFound();
            }
            
            unitOfWork.TimeTables.Remove(timetable);
            unitOfWork.Complete();

            return Ok(timetable);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TimetableExists(Guid id)
        {
            return unitOfWork.TimeTables.Find(e => e.Id == id).Count() > 0;
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