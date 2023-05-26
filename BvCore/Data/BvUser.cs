using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic("BvUser")]
    public class BvUser
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public string Name = "";
        [DataDynamic]
        public string LastName = "";
        [DataDynamic]
        public string Email = "";
        [System.Xml.Serialization.XmlIgnore]
        [AjaxPro.AjaxNonSerializable]
        [DataDynamic]
        public string Password = "";
        [DataDynamic]
        public string Phone = "";
        [DataDynamic]
        public int SiteID = 2;
        [DataDynamic]
        private DateTime PhoneValidate = new DateTime(1900, 1, 1);
        [DataDynamic]
        private DateTime PhoneBlocked = new DateTime(1900, 1, 1);
        [DataDynamic]
        public DateTime Created = new DateTime(1900, 1, 1);
        [System.Xml.Serialization.XmlIgnore]
        [DataDynamic]
        public DateTime LastAccess = new DateTime(1900, 1, 1);
        [System.Xml.Serialization.XmlIgnore]
        [DataDynamic(DataType = typeof(byte))]
        public bool IsAgent = false;
        [System.Xml.Serialization.XmlIgnore]
        [DataDynamic(DataType = typeof(byte))]
        public bool IsAdmin = false;
        [DataDynamic]
        [System.Xml.Serialization.XmlIgnore]
        public bool Newsletter = true;

        private List<UserItem> _UserItems = null;
        [System.Xml.Serialization.XmlIgnore]
        public List<UserItem> UserItems
        {
            get {
                if (_UserItems == null)
                    using (var ctx = new Data<UserItem>())
                        _UserItems = ctx.Find(ui => ui.BvUserId == Id);
                return _UserItems;
            }
            set { _UserItems = value; }
        }
        public BvUser() { }
        public BvUser(string Name, string LastName, string Email, string Password)
        {
            this.Name = Name;
            this.LastName = LastName;
            this.Email = Email;
            this.Password = Password;
        }
        public DateTime RegisterLastAccess()
        {
            DateTime t = LastAccess;
            RegisterAccess(Id);
            return t;
        }
        public static void RegisterAccess(int UserId)
        {
            using (var dbi = new Dbi())
            {
                dbi.Execute("update BvUser set lastaccess=? where Id=?", Date.Now, UserId);
            }
        }
        public static BvUser FromAgent(Customer c)
        {
            using(var ctx = new Data<BvUser>())
            {
                BvUser user = ctx.Get(u => u.LastName == c.Id.ToString() && u.SiteID == 2);
                if (user == null)
                    user = new BvUser(c.Name, c.Id.ToString(), "", "") { SiteID = 2, Created = Date.Now, IsAgent = true };
                user.Name = c.Name;
                user.Email = c.ContactEmail;
                user.Password = c.Password;
                ctx.Save(user);
                return user;
            }
        }
    }
    [AjaxPro.AjaxNoTypeUsage]
    [DataDynamic("BvUserItem")]
    public class UserItem
    {
        public UserItem() { }
        public UserItem(int userId, string name, string channel, string view, string queryparams)
        {
            this.BvUserId = userId;
            this.Name = name;
            this.Channel = channel;
            this.View = view;
            this.QueryString = queryparams;
        }

        [DataDynamic(PrimaryKey = true)]
        public int Id = 0;
        [DataDynamic]
        public int BvUserId = 0;
        [DataDynamic]
        public string Name = "";
        [DataDynamic("ChannelKey")]
        public string Channel = "";
        [DataDynamic("ViewKey")]
        public string View = "";
        [DataDynamic]
        public string QueryString = "";
        [DataDynamic]
        public DateTime Created = DateTime.Now;
    }

}
