using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Bovision.Import
{
    public class ImportWork : IDisposable
    {
        public ImportJob Job { get; private set; }
        private string basepath;
        private Dictionary<string, XtndEstate> current_estates = new Dictionary<string, XtndEstate>();
        private Dictionary<string, XtndEstate> duplicate_estates = new Dictionary<string, XtndEstate>();
        private Dictionary<string, XtndEstate> duplicate_urls = new Dictionary<string, XtndEstate>();
        private List<int> deleteIds = new List<int>();
        private PersistantDictionary<State> states;
        private StreamWriter log;
        private static object lck = new object();

        public int Count { get { return Job.Parser.Result.Count; } }
        public int Processed { get; private set; }
        public int Removed { get; private set; }
        public int RemovedCount { get { return deleteIds.Count; } }

        public ImportWork(string localfile, string basepath, ImportSettings settings, params Credentials[] credentials)
        {
            if (settings == null)
                settings = new ImportSettings();
            var parser = new ImportParser(File.OpenRead(Path.Combine(basepath, localfile)), settings);
            var job = new ImportJob(parser, new List<Credentials>(credentials));
            Setup(job, basepath);
        }
        public ImportWork(Uri remotepath, string basepath, ImportSettings settings, params Credentials []credentials)
        {
            if (settings == null)
                settings = new ImportSettings();
            using(var wc = new System.Net.WebClient())
                wc.DownloadFile(remotepath, Path.Combine(basepath, remotepath.Host + ".xml"));
            var parser = new ImportParser(File.OpenRead(Path.Combine(basepath, remotepath.Host + ".xml")), settings);
            var job = new ImportJob(parser, new List<Credentials>(credentials));
            Setup(job, basepath);
        }
        public ImportWork(ImportJob job, string basepath)
        {
            Setup(job, basepath);
        }
        private void Setup(ImportJob job, string basepath)
        {
            this.Job = job;
            this.basepath = basepath;
            if (Job.CheckCredentials() != Credentials.AuthenticationState.Authenticated)
                throw new Exception();
            var path = Path.Combine(basepath, Job.Credentials.AgentId.ToString());
            Directory.CreateDirectory(path);
            states = new PersistantDictionary<State>(Path.Combine(path, Job.JobId.ToString() + ".map"), 128, false);
            log = File.AppendText(Path.Combine(path, Job.JobId.ToString() + ".log"));

            lock (lck)
            {
                using (var ctx = new Data<XtndEstate>())
                {
                    var updated = new HashSet<int>();
                    var estates = ctx.Find(est => est.AgentId == Job.Credentials.AgentId).Where(est => !String.IsNullOrEmpty(est.ClientId));
                    foreach (var est in estates)
                    {
                        XtndEstate xest;
                        if (current_estates.TryGetValue(est.ClientId, out xest))
                        {
                            if(est.Changed < xest.Changed)
                                deleteIds.Add(est.Id);
                            else
                            {
                                deleteIds.Add(xest.Id);
                                current_estates[est.ClientId] = est;
                            }
                        }
                        else
                            current_estates[est.ClientId] = est;
                    }
                    foreach (var aid in Job.Parser.Settings.AgentDuplicateCheck.Where(id => id != Job.Credentials.AgentId))
                        foreach (var est in ctx.Find(est => est.AgentId == aid).Where(est => !String.IsNullOrEmpty(est.ClientId)))
                            duplicate_urls[est.DescriptionUrl] = duplicate_estates[est.ClientId] = est;
                }

                var cids = Job.Parser.Estates.Select(est => est.ClientId).ToHashSet();
                deleteIds.AddRange(current_estates.Values.Where(est => !cids.Contains(est.ClientId)).Select(est => est.Id).ToList());

                current_estates.ForEach(kv => states.Set(kv.Key, State.Loaded));
                job.Parser.Result.ForEach(r => states.Set(r.ClientId, (r.Status == ParserResultStatus.Parsed) ? State.Parsed : State.NotValid));
            }
        }
        public void Import()
        {
            for (Processed = 0; Processed < Job.Parser.Result.Count; Processed++)
            {
                var res = Job.Parser.Result[Processed];
                XtndEstate o_est;
                string msg;
                current_estates.TryGetValue(res.Estate.ClientId, out o_est);
                var state = Import(res.Estate, o_est, out msg);
                states.Set(res.Estate.ClientId, state);
                if (!string.IsNullOrEmpty(msg))
                {
                    log.WriteLine(msg);
                    log.Flush();
                }
            }
            BovisionFacade.Facade fc = new BovisionFacade.Facade(Job.Credentials.AgentId);
            for (Removed = 0; Removed < deleteIds.Count; Removed++)
            {
                if (!fc.Delete(deleteIds[Removed], Job.Credentials.AgentId))
                    log.WriteLine(string.Format("Couldn't delete {0}", deleteIds[Removed]));
            }
            log.Close();
        }
        private State Import(XtndEstate est, XtndEstate o_est, out string log)
        {
            State state = states.Get(est.ClientId);
            if (state != State.Parsed && state != State.Error)
            {
                log = "Skipped (already processed): " + est.ClientId;
                return state;
            }
            XtndEstate d_est;
            if (!string.IsNullOrEmpty(est.DescriptionUrl) && Job.Parser.Settings.ImportAllImages == false)
                est.Images = est.Images.Take(1).ToList();
            est.AgentId = Job.Credentials.AgentId;
            List<string> changes = null;
            if (o_est != null && !Diff(o_est, est, out changes))
            {
                log = "Skipped:" + est.ClientId;
                return State.Skipped;
            }
            if (duplicate_estates.TryGetValue(est.ClientId, out d_est) || duplicate_urls.TryGetValue(est.DescriptionUrl, out d_est))
            {
                string reason = d_est.ClientId == est.ClientId ? "ClientId" : "URL";
                log = String.Format("Skipped: {0}, Duplicate from Agent:[{1}]/Id:[{2}] ({3})", est.ClientId, d_est.AgentId, d_est.Id, reason);
                return State.Skipped;
            }
            try {
                var obj = ConvertEstate.Convert2Estate(est);
                if (o_est != null)
                    obj.ServerID = o_est.Id;
                est.Id = AppCommon.T.Save(obj, "XmlImport");
            }
            catch(Exception ex) {
                log = String.Format("Error:{0} - {1}", est.ClientId, ex.ToString());
                return State.Error;
            }
            if (o_est != null)
            {
                log = String.Format("Updated({0}):{1}", changes.Join(","), est.ClientId);
                return State.Updated;
            }
            log = String.Format("New({0}):{1}",est.Id, est.ClientId);
            return State.New;
        }
        public static bool Diff(XtndEstate old, XtndEstate current, out List<string> changes)
        {
            changes = new List<string>();

            if(current.Contract != EstateType.ContractType.Sale)
                if (old.Rent != current.Rent)
                    changes.Add(string.Format("Rent [{0}:{1}]", old.Rent, current.Rent));
            
            if (old.Rooms != current.Rooms)
                changes.Add(string.Format("Rooms [{0}:{1}]", old.Rooms, current.Rooms));
            if (old.BuildYear != current.BuildYear)
                changes.Add(string.Format("BuildYear [{0}:{1}]", old.BuildYear, current.BuildYear));
            if (old.UsableArea != current.UsableArea)
                changes.Add(string.Format("UsableArea [{0}:{1}]", old.UsableArea, current.UsableArea));
            if (old.SideArea != current.SideArea)
                changes.Add(string.Format("SideArea [{0}:{1}]", old.SideArea, current.SideArea));
            if (old.LotArea != current.LotArea)
                changes.Add(string.Format("LotArea [{0}:{1}]", old.LotArea, current.LotArea));
            if (!(old.MunicipalityId == 9999 && current.MunicipalityId == 0))
                if (old.MunicipalityId != current.MunicipalityId)
                    changes.Add("MunicipalityId");
            if (old.Price != current.Price)
                changes.Add(string.Format("Price [{0}:{1}]", old.Price, current.Price));
            if (String.Compare(old.CountryId, 0, current.CountryId, 0, 2, true) != 0)
                changes.Add(string.Format("CountryId [{0}:{1}]", old.CountryId, current.CountryId));
            if (String.Compare(old.Currency, current.Currency, true) != 0)
                changes.Add(string.Format("Currency [{0}:{1}]", old.Currency, current.Currency));
            if (String.Compare(old.DescriptionUrl, current.DescriptionUrl, true) != 0)
                changes.Add(string.Format("DescriptionUrl [{0}:{1}]", old.DescriptionUrl, current.DescriptionUrl));
            if (String.Compare(old.ContactEmail, current.ContactEmail, true) != 0)
                changes.Add(string.Format("ContactEmail [{0}:{1}]", old.ContactEmail, current.ContactEmail));
            if (String.Compare(old.City, current.City, true) != 0)
                changes.Add(string.Format("City [{0}:{1}]", old.City, current.City));
            if (String.Compare(old.Address, current.Address, true) != 0)
                changes.Add(string.Format("Address [{0}:{1}]", old.Address, current.Address));

            if(DiffText(old.Description, current.Description))
                changes.Add(string.Format("Description len:[{0}:{1}]", Util.Length(old.Description), Util.Length(current.Description)));
            if (DiffText(old.Design, current.Design))
                changes.Add("Design");
            if (DiffText(old.Equipment, current.Equipment))
                changes.Add("Equipment");
            if (DiffText(old.Surroundings, current.Surroundings))
                changes.Add("Surroundings");
            if (DiffText(old.Parking, current.Parking))
                changes.Add("Parking");
            if (DiffText(old.OutdoorPlace, current.OutdoorPlace))
                changes.Add("OutdoorPlace");
            if (DiffText(old.OtherInfo, current.OtherInfo))
                changes.Add("OtherInfo");
            if (DiffText(old.OtherBuildings, current.OtherBuildings))
                changes.Add("OtherBuildings");

            if (String.Compare(old.Status, current.Status, true) != 0)
                changes.Add("Status(" + current.Status + ")");

            return changes.Count > 0;
        }
        private static bool DiffText(string a, string b)
        {
            var sa = CleanText(a);
            var sb = CleanText(b);
            return !sa.Equals(sb);
        }
        private static StringBuilder CleanText(string s)
        {
            if (s == null)
                return new StringBuilder();
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
                if (Char.IsLetterOrDigit(s[i]))
                    sb.Append(s[i]);
            return sb;
        }

        public void Dispose()
        {
            states.Flush();
            states.Dispose();
            log.Dispose();
        }
    }
}
