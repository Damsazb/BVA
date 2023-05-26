using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public class StatsOverview
    {
        public List<StatsOverviewRow> Result;
        private StatsOverview() {  }
        const string sql = @"select top 100
                    sum(N_LISTTRAFF) as ListCount,
	                sum(N_ANTALBESKRIV) as DescriptionCount,
	                DATEADD(mm, (year(dat_datum)-1900) * 12 + (month(dat_datum)-1), 0) as month
	                from statobjekt
	                where dat_datum >= ? and n_maklarid=?
	                group by DATEADD(mm, (year(dat_datum)-1900) * 12 + (month(dat_datum)-1), 0)
	                order by month desc";
        public static StatsOverview GetStats(int AgentId)
        {
            return GetStats(Date.Now.AddMonths(-3), AgentId);
        }
        public static StatsOverview GetStats(DateTime start, int AgentId)
        {
            using(var dbi = new Dbi())
            {
                var list = dbi.Fetch(sql, Date.FirstInMonth(start), AgentId).
                    Select(r => new StatsOverviewRow { ListCount = r.GetInt32(0), DescriptionCount = r.GetInt32(1), Month = r.GetDateTime(2) }).ToList();
                return new StatsOverview { Result = list };
            }
        }
    }
    public class StatsOverviewRow
    {
        public int ListCount = 0;
        public int DescriptionCount = 0;
        public DateTime Month;
        public string MonthName { get { return Date.Month(Month.Month); } }
    }
}
