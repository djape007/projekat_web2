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
    public class PricelistsController : ApiController
    {
        IUnitOfWork unitOfWork;

        public PricelistsController(IUnitOfWork unitOfWork) {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/Pricelists
        public IEnumerable<Pricelist> GetPricelists()
        {
            return unitOfWork.Pricelists.GetAll();
        }

        // GET: api/Pricelists/5
        [ResponseType(typeof(Pricelist))]
        public IHttpActionResult GetPricelist(Guid id)
        {
            Pricelist pricelist = unitOfWork.Pricelists.Get(id);
            if (pricelist == null)
            {
                return NotFound();
            }

            return Ok(pricelist);
        }

        // PUT: api/Pricelists/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PutPricelist(Guid id, Pricelist pricelist)
        {
            if (!ModelState.IsValid || pricelist == null)
            {
                return BadRequest(ModelState);
            }

            if (id != pricelist.Id)
            {
                return BadRequest();
            }

            try
            {
                Pricelist db_pricelist = unitOfWork.Pricelists.Get(id);
                string eTag = HttpContext.Current.Request.Headers.Get("etag");

                string prices = "";
                db_pricelist.PriceHistories.ToList().ForEach(x => prices += x.Price.ToString());

                string from = db_pricelist.From.ToString("yyyy-M-d");
                string to = db_pricelist.To.ToString("yyyy-M-d");

                string rawValue = db_pricelist.Id + from + to + prices;
                string db_eTag = ComputeEtag(rawValue);

                if (eTag != db_eTag)
                {
                    return StatusCode(HttpStatusCode.PreconditionFailed);
                }

                db_pricelist.From = pricelist.From;
                db_pricelist.To = pricelist.To;

                unitOfWork.Pricelists.Update(db_pricelist);
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PricelistExists(id))
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

        // POST: api/Pricelists
        [ResponseType(typeof(Pricelist))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PostPricelist(Pricelist pricelist)
        {
            if (!ModelState.IsValid || pricelist == null)
            {
                return BadRequest(ModelState);
            }

            unitOfWork.Pricelists.Add(pricelist);

            try
            {
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (PricelistExists(pricelist.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = pricelist.Id }, pricelist);
        }

        // DELETE: api/Pricelists/5
        [ResponseType(typeof(Pricelist))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeletePricelist(Guid id)
        {
            Pricelist pricelist = unitOfWork.Pricelists.Get(id);
            if (pricelist == null)
            {
                return NotFound();
            }

            unitOfWork.Pricelists.Remove(pricelist);
            unitOfWork.Complete();

            return Ok(pricelist);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PricelistExists(Guid id)
        {
            return unitOfWork.Pricelists.Find(e => e.Id == id).Count() > 0;
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