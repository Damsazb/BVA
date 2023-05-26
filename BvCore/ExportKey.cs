using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic]
    public class ExportKey : BaseDataReadWriteId<ExportKey>, ILocalID
    {
        [DataDynamic(PrimaryKey=true)]
        public int Id { get; set; }
        [DataDynamic]
        public string System { get; set; }
        [DataDynamic]
        public string Query { get; set; }
        [DataDynamic]
        public string Owner { get; set; }
        [DataDynamic]
        public string OtherInfo { get; set; }
        [DataDynamic]
        public string Key { get; set; }
        [DataDynamic]
        public DateTime Created { get; set; }
        [DataDynamic]
        public DateTime Deleted;

        public static ExportKey CreateKey(string system, string owner, string info, string query)
        {
            var crypt = new GCrypt(GCrypt.Crypto.TripleDES);
            var key = new ExportKey
            {
                Deleted = Date.Default,
                Created = Date.Now,
                Owner = owner,
                Query = query,
                OtherInfo = info,
                System = system
            };
            key.Save();
            key.Update(Expr.Eq("Key", crypt.Encode(key.Id.ToString(), GCrypt.Encoding.UrlToken)));
            return key;
        }
        public static ExportKey CreateKey(Customer c)
        {
            return CreateKey("Export", c.Id.ToString(), "", "ai=" + c.Id);
        }
        public static List<ExportKey> GetKeys()
        {
            return ExportKey.Find(Expr.LtEq("Deleted", Date.Treshold));
        }
        public static List<ExportKey> GetKeys(Customer c)
        {
            return ExportKey.Find(Expr.LtEq("Deleted",Date.Treshold), Expr.Eq("Owner", c.Id.ToString()));
        }
        public static List<ExportKey> GetKeys(string system)
        {
            return ExportKey.Find(Expr.LtEq("Deleted", Date.Treshold), Expr.Eq("system",system));
        }
        public static ExportKey GetKey(string skey)
        {
            return ExportKey.Get(Expr.LtEq("Deleted", Date.Treshold), Expr.Eq("Key", skey));
        }
        public static ExportKey GetKey(int id)
        {
            return ExportKey.Get(Expr.LtEq("Deleted", Date.Treshold), Expr.Eq("Id",id));
        }
        public static void RemoveKey(string skey)
        {
            var key = GetKey(skey);
            if (key != null)
                key.Deleted = Date.Now;
            key.Save();
        }
        public static void RemoveKey(int Id)
        {
            var key = Get(Id);
            if (key != null && key.Deleted <= Date.Treshold)
                key.Deleted = Date.Now;
            key.Save();
        }
        private static LRUCache<string, ExportKey> keyCache = new LRUCache<string, ExportKey>(100, TimeSpan.FromMinutes(10),
            delegate(string key, out ExportKey v)
            {
                v = Bovision.ExportKey.GetKey(key);
                return v != null;
            });
        public static bool TryGetKey(string key, out ExportKey xkey)
        {
            lock (keyCache)
            {
                return (keyCache.TryGetValue(key, out xkey));
            }
        }
        public static bool ValidateKey(string key, string system)
        {
            ExportKey xkey;
            if (TryGetKey(key, out xkey) && xkey.System == system)
                return true;
            return false;
        }
    }
}
