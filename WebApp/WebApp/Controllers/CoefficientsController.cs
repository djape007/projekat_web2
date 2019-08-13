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
    public class CoefficientsController : ApiController
    {
        private IUnitOfWork unitOfWork;
        public CoefficientsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/Coefficients
        public IEnumerable<Coefficient> GetCoefficients()
        {
            return unitOfWork.Coefficients.GetAll();
        }

        // GET: api/Coefficients/5
        [ResponseType(typeof(Coefficient))]
        public IHttpActionResult GetCoefficient(Guid id)
        {
            Coefficient coefficient = unitOfWork.Coefficients.Get(id);
            if (coefficient == null)
            {
                return NotFound();
            }

            return Ok(coefficient);
        }

        // PUT: api/Coefficients/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCoefficient(Guid id, Coefficient coefficient)
        {
            if (!ModelState.IsValid || coefficient == null)
            {
                return BadRequest(ModelState);
            }

            if (id != coefficient.Id)
            {
                return BadRequest();
            }

            try
            {
                unitOfWork.Coefficients.Update(coefficient);
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoefficientExists(id))
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

        // POST: api/Coefficients
        [ResponseType(typeof(Coefficient))]
        public IHttpActionResult PostCoefficient(Coefficient coefficient)
        {
            if (!ModelState.IsValid || coefficient == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                unitOfWork.Coefficients.Add(coefficient);
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (CoefficientExists(coefficient.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = coefficient.Id }, coefficient);
        }

        // DELETE: api/Coefficients/5
        [ResponseType(typeof(Coefficient))]
        public IHttpActionResult DeleteCoefficient(Guid id)
        {
            Coefficient coefficient = unitOfWork.Coefficients.Get(id);
            if (coefficient == null)
            {
                return NotFound();
            }

            unitOfWork.Coefficients.Remove(coefficient);
            unitOfWork.Complete();

            return Ok(coefficient);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CoefficientExists(Guid id)
        {
            return unitOfWork.Coefficients.Find(e => e.Id == id).Count() > 0;
        }
    }
}