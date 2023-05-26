using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Bovision.IO;

namespace Bovision
{
    public static class ImpressionStat
    {
        public static void DescriptionImpression(int lObjektnr, int nMaklarid)
        {
            StatData.Current.Add(0, 1, lObjektnr, nMaklarid);
        }
        public static void ListImpression(int lObjektnr, int nMaklarid)
        {
            StatData.Current.Add(1, 0, lObjektnr, nMaklarid);
        }
        public static void ListImpression(IEnumerable<FullEstate> estates)
        {
            DateTime date = Date.Now;
            var data = estates.Select(est => new StatRow(date, est.Id, est.AgentId) { ListImpressions = 1 });
            StatData.Current.Add(data);
        }
        public static void Register(IEnumerable<StatRow> data)
        {
            StatData.Current.Add(data);
        }
        public static void Flush()
        {
            StatData.Current.Flush();
        }
    }
    public class StatData
    {
        private const int FlushLimit = 150;
        private static StatData m_data = new StatData();
        private static readonly object myLock = new object();

        private Dictionary<string, StatRow> m_hash = new Dictionary<string, StatRow>();
        public static StatData Current
        {
            get { return m_data; }
        }
        private string GetKey(DateTime dateStatistics, int estateId, int agentId)
        {
            return agentId.ToString() + estateId + ":" + dateStatistics.ToShortDateString();
        }
        private void Add(string key, int ListImpressions, int DescriptionImpressions, int estateId, int agentId, DateTime date)
        {
            try
            {
                StatRow row = null;
                if (!m_hash.TryGetValue(key, out row))
                {
                    row = new StatRow(date, estateId, agentId);
                    m_hash.Add(key, row);
                }
                row.DescriptionImpressions += DescriptionImpressions;
                row.ListImpressions += ListImpressions;
            }
            catch { }
        }
        public void Add(IEnumerable<StatRow> items)
        {
            lock (myLock)
            {
                foreach (var item in items)
                {
                    string key = GetKey(item.Date, item.EstateId, item.AgentId);
                    Add(key, item.ListImpressions, item.DescriptionImpressions, item.EstateId, item.AgentId, DateTime.Now);
                }
                if (m_hash.Keys.Count > FlushLimit)
                    Flush();
            }
        }
        public void Add(int ListImpressions, int DescriptionImpressions, int estateId, int agentId)
        {
            Add(ListImpressions, DescriptionImpressions, estateId, agentId, Date.Now);
        }
        public void Add(int ListImpressions, int DescriptionImpressions, int estateId, int agentId, DateTime date)
        {
            string key = GetKey(date, estateId, agentId);
            lock (myLock)
            {
                Add(key, ListImpressions, DescriptionImpressions, estateId, agentId, date);
                if (m_hash.Keys.Count > FlushLimit)
                    Flush();
            }
        }
        public void FlushHold()
        {
            lock (myLock)
            {
                var oSaveThread = new SaveThreadClass(m_hash);
                oSaveThread.SaveThreadProc();
                m_hash = new Dictionary<string, StatRow>();
            }
        }
        public void Flush()
        {
            lock (myLock)
            {
                var oSaveThread = new SaveThreadClass(m_hash);
                System.Threading.ThreadStart otter = new System.Threading.ThreadStart(oSaveThread.SaveThreadProc);
                System.Threading.Thread oThread = new System.Threading.Thread(otter);
                m_hash = new Dictionary<string, StatRow>();
                oThread.Start();
            }
        }
        ~StatData()
        {
            Flush();
        }

        private class SaveThreadClass
        {
            Dictionary<string, StatRow> m_Container;
            public SaveThreadClass(Dictionary<string, StatRow> oContainer)
            {
                m_Container = oContainer;
            }

            public void SaveThreadProc()
            {
                using (var dbi = new Dbi())
                {
                    if (m_Container != null)
                        foreach (var statistics in m_Container.Values)
                            Save(statistics, dbi);
                }
            }
            private void Save(StatRow item, Dbi dbi)
            {
                SqlTransaction trans = dbi.GetTransaction();
                try
                {
                    var ret = dbi.Execute(trans, string.Format("update statobjekt set n_antalbeskriv=n_antalbeskriv+{0},N_LISTTRAFF=N_LISTTRAFF+{1} where l_objektnr={2} and n_maklarid={3} and dat_datum='{4}'", item.DescriptionImpressions, item.ListImpressions, item.EstateId, item.AgentId, item.Date.ToShortDateString() + " 00:00"));

                    if (ret <= 0)
                    {
                        dbi.Execute(trans, string.Format("insert into statobjekt(L_OBJEKTNR,N_MAKLARID,DAT_DATUM,N_VECKA,N_ANTALBESKRIV,N_LISTTRAFF,L_SAJTID) values({0},{1},'{2}',{3},{4},{5},0)", item.EstateId, item.AgentId, item.Date.ToShortDateString() + " 00:00", item.Week, item.DescriptionImpressions, item.ListImpressions));
                    }
                    trans.Commit();
                }
                catch(Exception ex)
                {
                    AppLog.Error(typeof(StatData), "{0}:{1}:{2}:{3}:{4}:{5}",item.Date.ToShortDateString(),item.DescriptionImpressions, item.ListImpressions, item.EstateId, item.AgentId, ex.ToString());
                    try { trans.Rollback(); }
                    catch { }
                }
            }
        }
    }

    public class StatRow
    {
        public StatRow(DateTime date, int estateId, int agentId)
        {
            EstateId = estateId;
            AgentId = agentId;
            Date = date;
        }
        public int EstateId;
        public int AgentId;
        public DateTime Date;
        public int DescriptionImpressions = 0;
        public int ListImpressions = 0;
        public int Week
        {
            get { return Bovision.Date.Weeknumber_Entire4DayWeekRule(Date); }
        }
    }
}
