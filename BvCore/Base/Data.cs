using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bovision.Import;
using Bovision.IO;

namespace Bovision
{
    public class Data : IUpdatable
    {
        public static IReadOnlyCollection<ImportEstate> Estates { get { return estates; } }
        public static IEnumerable<ExportAgent> Agents { get { return agents.Values; } }
        public static ImportEstate EstateById(int id) { ImportEstate e; estates.index.TryGetValue(id, out e); return e; }
        public static ExportAgent AgentById(int id) { ExportAgent a; agents.TryGetValue(id, out a); return a; }

        private static EList<ImportEstate> estates;
        private static Dictionary<int, ExportAgent> agents = new Dictionary<int, ExportAgent>();
        private static DateTime AgentLastChanged = new DateTime(1900, 1, 1);
        static Data()
        {
            var instance = new Data();
            EstateArea.EnsureAllIsLoaded();
        }
        private Data()
        {
            Updateable.Add(this, 90, Updateable.UpdateOption.RunAndLock);
        }
        void IUpdatable.Update()
        {
            //var t = Task.Run(() => UpdateAgents());
            UpdateAgents();
            UpdateEstates();
            //Task.WaitAll(t);
        }
        void UpdateEstates()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    AppLog.Debug(typeof(Data), "UpdateEstates:Start");
                    var ctx_est = new Data<ImportEstate>();
                    var ctx_attch = new Data<BvFile>();

                    var t1 = Task<EList<ImportEstate>>.Run(() => ctx_est.EFind(est => est.Id));
                    var t2 = Task<List<BvFile>>.Run(() => ctx_attch.Find(new OrderBy[]{ new OrderBy("ImgOrder", Direction.Ascend) }, 0));
                    Task.WaitAll(t1, t2);

                    estates = t1.Result;
                    ImportEstate estate;
                    foreach (var img in t2.Result)
                        if (estates.index.TryGetValue(img.EstateId, out estate))
                            estate.Images.Add(img.FileName);
                    ctx_est.Dispose();
                    ctx_attch.Dispose();
                    return;
                }
                catch (Exception ex)
                {
                    AppLog.Error(typeof(Data), "UpdateEstates:" + ex.ToString());
                }
            }
        }
        void UpdateAgents()
        {
            for (int i = 0; i < 3; i++)
            {
                AppLog.Debug(typeof(Data), "UpdateAgents:Start");
                try
                {
                    using (var ctx = new Data<ExportAgent>())
                    {
                        var alist = new Dictionary<int, ExportAgent>(agents);
                        foreach (var a in ctx.Find(a => a.Changed > AgentLastChanged))
                        {
                            alist[a.Id] = a;
                            if (a.Changed > AgentLastChanged)
                                AgentLastChanged = a.Changed;
                        }
                        agents = alist;
                    }
                    return;
                }
                catch (Exception ex)
                {
                    AppLog.Error(typeof(Data), "UpdateAgents:" + ex.ToString());
                }
            }
        }
    }
}
