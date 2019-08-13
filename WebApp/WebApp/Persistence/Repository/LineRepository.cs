using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using WebApp.Models;

namespace WebApp.Persistence.Repository
{
    public class LineRepository : Repository<Line, string>, ILineRepository
    {
        public LineRepository(DbContext context) : base(context)
        {
        }

        new public IEnumerable<Line> GetAll()
        {
            return context.Set<Line>().Include("Buses").Include("BusStopsOnLines").Include("PointLinePaths").ToList();
        }

        new public Line Get(string id)
        {
            Line line = context.Set<Line>().Include("Buses").Include("BusStopsOnLines").Include("BusStopsOnLines.BusStop").Include("PointLinePaths").FirstOrDefault(x => x.Id == id);

            if (line == null) {
                return line;
            }
            
            //na adresu stanice ce zakaciti string sa ostalim linijama koje prolaze kroz stanicu
            //ADRESA||2B,11A,8B...
            foreach(BusStopsOnLine busStopsonLine in line.BusStopsOnLines) {
                string linijeNaStanici_STR = "";
                //nadje ostale veze (izmedju stajalista i linija) koje koriste istu stanicu
                var linijeNaStanici = context.Set<BusStopsOnLine>().Where(x => x.BusStopId == busStopsonLine.BusStopId).ToList();
                foreach(var vezaLinijaStanica in linijeNaStanici) {
                    linijeNaStanici_STR += vezaLinijaStanica.LineId + ",";
                }

                //znaci da ima bar jedna linija (a uvek ce biti :D )
                if (linijeNaStanici_STR != "") {
                    //skine se zarez sa kraja
                    linijeNaStanici_STR = linijeNaStanici_STR.Remove(linijeNaStanici_STR.Length - 1);
                }

                busStopsonLine.BusStop.Address = linijeNaStanici_STR;
            }

            return line;
        }
    }
}