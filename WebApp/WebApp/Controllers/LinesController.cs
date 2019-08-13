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
using System.Web.Http.Results;
using WebApp.Models;
using WebApp.Persistence;
using WebApp.Persistence.UnitOfWork;

namespace WebApp.Controllers
{
    public class LinesController : ApiController
    {
        private IUnitOfWork unitOfWork;
        public LinesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/Lines
        public IEnumerable<Line> GetLines()
        {
            return unitOfWork.Lines.GetAll();
        }

        // GET: api/Lines/WithBuses
        [Route("api/Lines/WithBuses")]
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<Line> GetLinesWithBuses() {
            return unitOfWork.Lines.GetAll().Where(x=> x.Buses.Count > 0);
        }

        // GET: api/Lines/5
        [ResponseType(typeof(Line))]
        public IHttpActionResult GetLine(string id)
        {
            Line line = unitOfWork.Lines.Get(id);
            if (line == null)
            {
                return NotFound();
            }

            string rawValue = line.Id + line.Direction + line.PointLinePaths.Count();
            string eTag = ComputeEtag(rawValue);
            HttpContext.Current.Response.AddHeader("Access-Control-Expose-Headers", "etag");
            HttpContext.Current.Response.Headers.Add("etag", eTag);
            return Ok(line);
        }

        // PUT: api/Lines/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PutLine(string id,Line line)
        {
            if (!ModelState.IsValid || line == null)
            {
                return BadRequest(ModelState);
            }

            if (id != line.Id)
            {
                return BadRequest();
            }

            try
            {
                string eTag = HttpContext.Current.Request.Headers.Get("etag");

                Line db_line = unitOfWork.Lines.Get(id);
                string rawValue = db_line.Id + db_line.Direction + db_line.PointLinePaths.Count();
                string db_eTag = ComputeEtag(rawValue);

                if (eTag != db_eTag)
                {
                    return StatusCode(HttpStatusCode.PreconditionFailed);
                }

                db_line.Direction = line.Direction;

                unitOfWork.PointPathLines.RemoveRange(db_line.PointLinePaths);
                unitOfWork.Complete();

                unitOfWork.PointPathLines.AddRange(line.PointLinePaths);
                unitOfWork.Complete();

                unitOfWork.Lines.Update(db_line);
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineExists(id))
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

        // POST: api/Lines
        [ResponseType(typeof(Line))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PostLine(Line line)
        {
            if (!ModelState.IsValid || line == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                unitOfWork.Lines.Add(line);
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (LineExists(line.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = line.Id }, line);
        }

        // DELETE: api/Lines/5
        [ResponseType(typeof(Line))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeleteLine(string id)
        {
            Line line = unitOfWork.Lines.Get(id);
            if (line == null)
            {
                return NotFound();
            }

            unitOfWork.Lines.Remove(line);
            unitOfWork.Complete();

            return Ok(line);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LineExists(string id)
        {
            return unitOfWork.Lines.Find(e => e.Id == id).Count() > 0;
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