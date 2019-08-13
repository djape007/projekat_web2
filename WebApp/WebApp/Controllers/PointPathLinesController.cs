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
    public class PointPathLinesController : ApiController
    {
        private IUnitOfWork unitOfWork;

        public PointPathLinesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: api/PointPathLines
        public IEnumerable<PointPathLine> GetPointPathLines()
        {
            return unitOfWork.PointPathLines.GetAll();
        }

        // GET: api/PointPathLines/5
        [ResponseType(typeof(PointPathLine))]
        public IHttpActionResult GetPointPathLine(Guid id)
        {
            PointPathLine pointPathLine = unitOfWork.PointPathLines.Get(id);
            if (pointPathLine == null)
            {
                return NotFound();
            }

            return Ok(pointPathLine);
        }

        // PUT: api/PointPathLines/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PutPointPathLine(Guid id, PointPathLine pointPathLine)
        {
            if (!ModelState.IsValid || pointPathLine == null)
            {
                return BadRequest(ModelState);
            }

            if (id != pointPathLine.Id)
            {
                return BadRequest();
            }

            try
            {
                unitOfWork.PointPathLines.Update(pointPathLine);
                unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PointPathLineExists(id))
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

        // POST: api/PointPathLines
        [ResponseType(typeof(PointPathLine))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult PostPointPathLine(PointPathLine pointPathLine)
        {
            if (!ModelState.IsValid || pointPathLine == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Line line = unitOfWork.Lines.Get(pointPathLine.LineId);
                if (line != null && line.PointLinePaths.Count > 0)
                {
                    if(pointPathLine.SequenceNumber <= 0)
                    {
                        pointPathLine.SequenceNumber = 1;
                    }
                    if(line.PointLinePaths.FirstOrDefault(x => x.SequenceNumber == pointPathLine.SequenceNumber) != null)
                    {
                        line.PointLinePaths.Where(x => x.SequenceNumber >= pointPathLine.SequenceNumber).ToList().ForEach(x => ++x.SequenceNumber);
                    }
                    else
                    {
                        int maxSeqNum = 1;
                        try
                        {
                            maxSeqNum = line.PointLinePaths.Max(x => x.SequenceNumber);
                            maxSeqNum++;
                        }
                        catch { }

                        if (pointPathLine.SequenceNumber > maxSeqNum)
                            pointPathLine.SequenceNumber = maxSeqNum;
                    }
                }
                unitOfWork.PointPathLines.Add(pointPathLine);
                unitOfWork.Complete();
            }
            catch (DbUpdateException)
            {
                if (PointPathLineExists(pointPathLine.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = pointPathLine.Id }, pointPathLine);
        }

        // DELETE: api/PointPathLines/5
        [ResponseType(typeof(PointPathLine))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeletePointPathLine(Guid id)
        {
            PointPathLine pointPathLine = unitOfWork.PointPathLines.Get(id);
            if (pointPathLine == null)
            {
                return NotFound();
            }

            Line line = unitOfWork.Lines.Get(pointPathLine.LineId);
            if (line != null)
            {
                line.PointLinePaths.Where(x => x.SequenceNumber > pointPathLine.SequenceNumber).ToList().ForEach(x => --x.SequenceNumber);
            }

            unitOfWork.PointPathLines.Remove(pointPathLine);
            unitOfWork.Complete();

            return Ok(pointPathLine);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PointPathLineExists(Guid id)
        {
            return unitOfWork.PointPathLines.Find(e => e.Id == id).Count() > 0;
        }
    }
}