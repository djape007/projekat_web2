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
    public class ProductTypesController : ApiController
    {
        IUnitOfWork unitOfWork;

        public ProductTypesController(IUnitOfWork unitOfWork) {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/ProductTypes
        public IEnumerable<ProductType> GetProductTypes()
        {
            return unitOfWork.ProductTypes.GetAll();
        }

        // GET: api/ProductTypes/5
        [ResponseType(typeof(ProductType))]
        public IHttpActionResult GetProductType(Guid id)
        {
            ProductType productType = unitOfWork.ProductTypes.Get(id);
            if (productType == null)
            {
                return NotFound();
            }

            return Ok(productType);
        }

        // PUT: api/ProductTypes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProductType(Guid id, ProductType productType)
        {
            if (!ModelState.IsValid || productType == null)
            {
                return BadRequest(ModelState);
            }

            if (id != productType.Id)
            {
                return BadRequest();
            }

            unitOfWork.ProductTypes.Update(productType);

            try
            {
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductTypeExists(id))
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

        // POST: api/ProductTypes
        [ResponseType(typeof(ProductType))]
        public IHttpActionResult PostProductType(ProductType productType)
        {
            if (!ModelState.IsValid || productType == null)
            {
                return BadRequest(ModelState);
            }

            unitOfWork.ProductTypes.Add(productType);

            try
            {
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (ProductTypeExists(productType.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = productType.Id }, productType);
        }

        // DELETE: api/ProductTypes/5
        [ResponseType(typeof(ProductType))]
        public IHttpActionResult DeleteProductType(Guid id)
        {
            ProductType productType = unitOfWork.ProductTypes.Get(id);
            if (productType == null)
            {
                return NotFound();
            }

            unitOfWork.ProductTypes.Remove(productType);
            unitOfWork.Complete();

            return Ok(productType);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductTypeExists(Guid id)
        {
            return unitOfWork.ProductTypes.Find(e => e.Id == id).Count() > 0;
        }
    }
}