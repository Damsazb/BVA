using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bovision.IO;
using DataCore;
namespace Bovision
{
    public static class BannerEngine
    {
        private static BannerStorage Storage;
        private static EstateAreaType country = EstateAreaType.From(typeof(Country));
        private static EstateAreaType county = EstateAreaType.From(typeof(County));
        private static EstateAreaType municipality = EstateAreaType.From(typeof(Municipality));
        private static EstateAreaType citypart = EstateAreaType.From(typeof(CityPart));
        private static EstateAreaType city = EstateAreaType.From(typeof(City));
        //private static EstateAreaType urban = EstateAreaType.From(typeof(UrbanDistrict));
        static BannerEngine()
        {
            Storage = new BannerStorage();
            Updateable.Add(Storage, 10, Updateable.UpdateOption.RunAndLock);
        }
        private class BannerStorage : IUpdatable
        {
            public List<Banner> Banners;
            public void Update()
            {
                Banners = Banner.Find(Expr.LtEq("Deleted", Date.Treshold), Expr.Eq("Active", 1));
            }
        }
        public static int BannerCount { get { return Storage.Banners.Count; } }
        public static void Update()
        {
            Storage.Update();
        }
        public static BannerResult GetResult(string adplace, string path, string query)
        {
            BannerResult result = new BannerResult();
            QueryParams qp = new QueryParams(query);

            var munics = Municipalities(qp);
            var countries = Countries(qp);


            foreach (var b in Storage.Banners.Where(b => b.IsShown && b.AdPlace == adplace))
            {
                QueryParams bq = new QueryParams(b.Query);
                var bannermunics = Municipalities(bq);
                var bannercountries = Countries(bq);
                int points = b.StartingPoints;
                points = (b.AdPlace == adplace) ? ++points : -1;
                if (points > 0 && bannermunics.Count > 0)
                    points = (munics.Overlaps(bannermunics)) ? ++points : -1;
                if (points > 0 && bannercountries.Count > 0)
                    points = (countries.Overlaps(bannercountries)) ? ++points : -1;
                if (points > 0 && !string.IsNullOrEmpty(b.Match))
                    points = (string.Compare(b.Match, path, true) == 0) ? ++points : -1;
                result.AddResult(b, points);
            }
            return result;
        }
        private static HashSet<int> Countries(QueryParams qp)
        {
            var c1 = qp.EstateAreaList.Where(ea => ea.Type == country).Select(ea => ea.Id);
            return new HashSet<int>(c1);
        }
        private static HashSet<int> Municipalities(QueryParams qp)
        {
            var munics = new HashSet<int>();
            var m1 = qp.EstateAreaList.Where(ea => ea.Type == city).Select(ea => City.ById(ea.Id).MunicipalityId);
            var m2 = qp.EstateAreaList.Where(ea => ea.Type == citypart).Select(ea => CityPart.ById(ea.Id).Municipality.Id);
            var m3 = qp.EstateAreaList.Where(ea => ea.Type == municipality).Select(ea => ea.Id);
            //var m4 = qp.EstateAreaList.Where(ea => ea.Type == urban).Select(ea => ea.MunicipalityId);
            var ccs = qp.EstateAreaList.Where(ea => ea.Type == county).Select(ea => County.ById(ea.Id));
            foreach (var cc in ccs)
                munics.UnionWith(cc.Municipalities.Select(m => m.Id));
            munics.UnionWith(m1);
            munics.UnionWith(m2);
            munics.UnionWith(m3);
            return munics;
        }
    }
    public class BannerResult
    {
        private List<BannerResultItem> ResultList = new List<BannerResultItem>();
        public void AddResult(Banner banner, int score)
        {
            ResultList.Add(new BannerResultItem(banner, score));
        }
        public List<BannerResultItem> GetResult()
        {
            if (ResultList.Count == 0)
                return ResultList;
            if (!ResultList.Exists(r => r.score >= 0))
                return new List<BannerResultItem>();
            return ResultList.OrderByDescending(r => r.score).ThenBy(r => r.random).ToList();
        }
        public List<BannerResultItem> GetTopResult()
        {
            var result = GetResult();
            if (result.Count == 0)
                return result;
            var top = result[0];
            return result.TakeWhile(r => r.score == top.score).ToList();
        }
    }
    public class BannerResultItem
    {
        private static Random rnd = new Random();
        public Banner banner { get; private set; }
        public int score { get; private set; }
        public int random { get; private set; }
        public BannerResultItem(Banner banner, int score)
        {
            this.banner = banner;
            this.score = score;
            this.random = rnd.Next();
        }
    }


    [DataDynamic]
    public class Banner : BaseDataReadWriteId<Banner>, IIdentityName, ILocalID
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public string Name { get; set; }

        [DataDynamic]
        public string AdPlace = "";

        [DataDynamic]
        public string Match = "";
        [DataDynamic]
        public string Query = "";

        [DataDynamic]
        public string LinkUrl = "";
        [DataDynamic]
        public string ImageUrl = "";
        [DataDynamic]
        public string ExternalCode = "";

        [DataDynamic]
        public DateTime Start = Date.Treshold;
        [DataDynamic]
        public DateTime Stop = Date.Treshold;
        [DataDynamic]
        public DateTime Created = Date.Treshold;
        [DataDynamic]
        public DateTime Deleted = Date.Treshold;
        [DataDynamic(typeof(int))]
        public bool Active = true;
        [DataDynamic]
        public int StartingPoints = 0;
        public bool IsShown
        {
            get
            {
                bool result = true;
                if (!Active)
                    result = false;
                if (Deleted > Date.Treshold)
                    result = false;
                if (Start > Date.Treshold && Start > DateTime.Now)
                    result = false;
                if (Stop > Date.Treshold && Stop < DateTime.Now)
                    result = false;
                return result;
            }
        }
        public string GetBannerCode()
        {
            if (!string.IsNullOrEmpty(ExternalCode))
                return ExternalCode;
            if (String.IsNullOrWhiteSpace(LinkUrl))
                return String.Format("<img src=\"{1}\">", ImageUrl);
            return String.Format("<a href=\"{0}\" target=\"_blank\"><img src=\"{1}\"></a>", LinkUrl, ImageUrl);
        }
        public static string []GetAdplaces()
        {
            using (var dbi = new Dbi())
                return dbi.Fetch("select distinct adplace from banner").Select(r => (string)r[0]).ToArray();
        }
    }
    //[DataDynamic("AdStore")]
    //public class AdStore : BaseDataReadWrite<AdStore>
    //{
    //    public static DateTime DateThreshold = new DateTime(2000, 1, 1);
    //    [DataDynamic(PrimaryKey = true)]
    //    public int Id = 0;
    //    [DataDynamic]
    //    public string AdPlace = "";
    //    [DataDynamic]
    //    public string Description = "";
    //    [DataDynamic]
    //    public string LinkUrl = "";
    //    [DataDynamic]
    //    public string ImageUrl = "";
    //    [DataDynamic]
    //    public string ExternalCode = "";
    //    [DataDynamic("ViewKey")]
    //    public string View = "";
    //    [DataDynamic("ChannelKey")]
    //    public string Channel = "";
    //    [DataDynamic]
    //    public string Path = "";
    //    [DataDynamic]
    //    public string Query = "";
    //    [DataDynamic]
    //    public DateTime Start = new DateTime(1900, 1, 1);
    //    [DataDynamic]
    //    public DateTime Stop = new DateTime(1900, 1, 1);
    //    [DataDynamic]
    //    public DateTime Created = new DateTime(1900, 1, 1);
    //    [DataDynamic]
    //    public DateTime Deleted = new DateTime(1900, 1, 1);
    //    [DataDynamic(typeof(int))]
    //    public bool Active = true;

    //    public bool IsShown
    //    {
    //        get
    //        {
    //            bool result = true;
    //            if (!Active)
    //                result = false;
    //            if (Deleted > DateThreshold)
    //                result = false;
    //            if (Start > DateThreshold && Start > DateTime.Now)
    //                result = false;
    //            if (Stop > DateThreshold && Stop < DateTime.Now)
    //                result = false;
    //            return result;
    //        }
    //    }
    //}
}
