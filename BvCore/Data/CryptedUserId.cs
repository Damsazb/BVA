using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bovision
{
    public class CryptedUserId
    {
        private BvCrypt crypt = new BvCrypt();
        public string CryptedId = "";
        public int UserId = 0;
        public CryptedUserId()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["bvuser"] != null)
            {
                this.CryptedId = HttpContext.Current.Request.Cookies["bvuser"].Value;
                if (this.CryptedId.Length > 2)
                    Int32.TryParse(crypt.Decode(CryptedId).Substring(2), out UserId);
            }
        }
        public CryptedUserId(string CryptedId)
        {
            try
            {
                this.CryptedId = CryptedId;
                Int32.TryParse(crypt.Decode(CryptedId).Substring(2), out UserId);
            }
            catch
            {
                UserId = 0;
            }
        }
        public CryptedUserId(int UserId)
        {
            this.UserId = UserId;
            CryptedId = crypt.Encode("bv" + UserId);
        }
        public void ClearCookie()
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie("bvuser", "") { Path = "/" });
        }
        public void SetCookie(string key, DateTime expire)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(key, CryptedId) { Path = "/", Expires = expire });
        }
        public void SetCookie()
        {
            SetCookie("bvuser", DateTime.Now.AddYears(1));
        }
    }
}
