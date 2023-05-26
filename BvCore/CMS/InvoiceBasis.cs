using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    //            DateTime start = new DateTime(Year, Month, 1);
    //        var rows = AppCommon.HistoryRow.Find(
    //            new DataDynamic.Eq("n_maklarid", AgentId),
    //            new DataDynamic.GtEq("dat_datum", start),
    //            new DataDynamic.Lt("dat_datum", start.AddMonths(1)));

    //        AppCommon.AdHistory hist = new AppCommon.AdHistory();
    //        hist.Add(rows, AppCommon.AdHistoryFilter.BillableTypesOV);
    //        List<int> ads = hist.GetObjects(AgentId, 0);
    //        if (ads.Count > 0)
    //        {

    //            var list = UEstate.Find(new DataDynamic.In("Id", ads));
    //            int diff = ads.Count - list.Count;
    //            string diffhtml = diff>0 ?  string.Format("<br />Ytterligare {0} objekt fanns under månaden men har raderats permanent och kan inte visas.",diff):"";
    //            return Template.Run(@"<table class=""full clean"" id=""fhtable"">
    //        				<tr><th>Kategori</th><th>Kommun</th><th>Område</th><th>Adress</th></tr>
    //        				[@:<tr>
    //        				<td>[Type.NameSingular]</td><td>[Municipality.Name]</td><td>[AreaName]</td><td>[Address]</td>
    //        				</tr>]
    //        				</table>"+diffhtml, list);
    //        }
    //        return "";
    [DataDynamic]
    public class V_BVFAKTURA : BaseDataReadOnly<V_BVFAKTURA>
    {
        [DataDynamic("N_MAKLARID")]
        public int AgentId;
        [DataDynamic("L_OBJEKTNR")]
        public int ObjectId;
        [DataDynamic("DAT_DATUM")]
        public DateTime Date;
        [DataDynamic("REALSTART")]
        public DateTime Start;
        [DataDynamic("REALSTOP")]
        public DateTime Stop;
        [DataDynamic("VISIBLE")]
        public short Visible;
        [DataDynamic("BVBETALARPEROBJEKT")]
        public int PayPerObject;
    }
    public class AdHistory
    {
        private Dictionary<int, Dictionary<int, IntervalCollection>> dict = new Dictionary<int, Dictionary<int, IntervalCollection>>();
        public AdHistory() { }
        public List<int> GetActiveObjects(int AgentId, DateTime month)
        {
            var start = new DateTime(month.Year, month.Month, 1);
            var rows = HistoryRow.Find(
                    new DataDynamic.Eq("n_maklarid", AgentId),
                    new DataDynamic.GtEq("dat_datum", start),
                    new DataDynamic.Lt("dat_datum", start.AddMonths(1)));
            Add(rows);
            return GetObjects(AgentId, 0);
        }

        private void Add(int AgentID, int ObjectID, DateTime start, DateTime stop)
        {
            var d2 = dict.ContainsKey(AgentID) ? dict[AgentID] : dict[AgentID] = new Dictionary<int, IntervalCollection>();
            IntervalCollection f;
            if (!d2.TryGetValue(ObjectID, out f))
                f = d2[ObjectID] = new IntervalCollection();
            f.Add(start, stop);
        }
        private List<int> GetObjects(int AgentID, int MinHours)
        {
            List<int> objects = new List<int>();
            Dictionary<int, IntervalCollection> a;
            if (dict.TryGetValue(AgentID, out a))
            {
                foreach (var vkp in a)
                    if (vkp.Value.Sum().TotalHours >= MinHours)
                        objects.Add(vkp.Key);
            }
            return objects;
        }
        private void Add(IEnumerable<HistoryRow> rows)
        {
            HistoryRow LastRow = null;
            DateTime? dstop = null;
            DateTime? start = null;

            foreach (var r in rows.OrderBy(x => x.l_objektnr).ThenBy(x => x.dat_datum))
            {
                if (start.HasValue)
                {
                    bool change = LastRow.l_objektnr != r.l_objektnr;
                    if (!change && r.dat_datum < dstop)
                        dstop = r.dat_datum;
                    if (start < r.dat_datum || change)
                        this.Add(LastRow.n_maklarid, LastRow.l_objektnr, start.Value, dstop.Value);
                }
                if (InMonth(r) && r.Visible == 1)
                {
                    start = r.RealStart;
                    dstop = r.RealStop;
                }
                else
                    start = dstop = null;
                LastRow = r;
            }
            if (start.HasValue)
                this.Add(LastRow.n_maklarid, LastRow.l_objektnr, start.Value, dstop.Value);
        }
        private bool InMonth(HistoryRow r)
        {
            DateTime firstday = GetFirstDay(r.dat_datum);
            if (r.RealStart < firstday.AddMonths(1) && r.RealStart >= firstday)
                if (r.RealStop > r.RealStart)
                    return true;
            return false;
        }
        private DateTime GetFirstDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
    }

    class IntervalCollection
    {
        private List<Interval> Intervals = new List<Interval>();
        public void Add(DateTime start, DateTime End)
        {
            Intervals.Add(new Interval(start, End));
        }
        public TimeSpan Sum()
        {
            TimeSpan ts = TimeSpan.Zero;
            foreach (var i in Intervals)
                ts = ts.Add(i.Span);
            return ts;
        }
    }

    public class Interval
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public Interval(DateTime Start, DateTime End)
        {
            this.Start = Start;
            this.End = End;
        }
        public TimeSpan Span
        {
            get { return End.Subtract(Start); }
        }
    }

    [DataDynamic("V_bvfaktura")]
    public class HistoryRow : BaseDataReadOnly<HistoryRow>
    {
        [DataDynamic()]
        public int l_objektnr;
        [DataDynamic()]
        public DateTime RealStart;
        [DataDynamic()]
        public DateTime RealStop;
        [DataDynamic()]
        public Int16 Visible;
        [DataDynamic()]
        public DateTime dat_datum;
        [DataDynamic()]
        public int n_maklarid;
    }
}
