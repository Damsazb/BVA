using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using DataCore;

namespace Bovision
{
    [Serializable]
    [DataDynamic("v_bvovlistimg")]
    public class BvFile : BaseDataReadWrite<BvFile>
    {
        [XmlIgnore]
        [DataDynamic]
        public int EstateId = 0;
        [DataDynamic]
        public string FileName = "";
    }
	[Serializable]
	[DataDynamic("v_ListDataBostad_Active")]
	public class FullEstate : _Estate<FullEstate>
	{
	}

	[Serializable]
	[DataDynamic("v_ListDataBostad_Active")]
	public abstract class _Estate<T> : BaseDataReadWriteId<T>, ILocalID where T : _Estate<T>,new()
	{
        public _Estate() { }
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public string ClientId = "";
        [DataDynamic(typeof(short))]
        public int AgentId = 0;
        [XmlIgnore]
        [DataDynamic]
        public int TypeId = 0;

        private string areaname = "";
        [DataDynamic]
        public string AreaName
        {
            get { return areaname; }
            set
            {
                value = value.Trim('.', '\"', '-', '_', '*').TrimStart(',', ' ');
                if (value.Length < 2)
                    value = "";
                areaname = value.Length < 2 ? "" : value;
            }
        }

        [DataDynamic()]
        protected bool ShowAddress = true;
        [DataDynamic(FieldName = "Address")]
        protected string _Address = "";
        public string Address { get { return (ShowAddress) ? _Address : ""; } set { _Address = value; } }

        [DataDynamic]
        public int ZipCode = 0;
        [DataDynamic]
        public string City = "";
        [DataDynamic(typeof(short))]
        public int BuildYear = 0;
        [DataDynamic]
        public double Rooms = 0;
        [DataDynamic()]
        public double LivingArea = 0.0;
        [DataDynamic()]
        public double SideArea = 0.0;
        [DataDynamic]
        public int LotArea = 0;
        [DataDynamic(DataType = typeof(short))]
        public int MunicipalityCode = 0;
        [DataDynamic]
        public string CountryCode = "";
        [DataDynamic]
        public int Price = 0;
        [DataDynamic]
        public int Rent = 0;
        [XmlIgnore]
        [DataDynamic]
        public int X = 0;
        [XmlIgnore]
        [DataDynamic]
        public int Y = 0;

        public CPoint Coord = null;
        [DataDynamic]
        public DateTime Changed = DateTime.MinValue;
        [DataDynamic]
        public DateTime Registered = DateTime.MinValue;

        private string _description = "";
        [DataDynamic]
        public string Description { get { return _description; } set { _description = rx_htmlstrip.Replace(value, String.Empty).Replace("\v", "\r\n"); } }

        private string _xdescription = "";
        [DataDynamic]
        public string ExtendedDescription { get { return _xdescription; } set { _xdescription = rx_htmlstrip.Replace(value, String.Empty).Replace("\v", "\r\n"); } }
        [DataDynamic]
        public string Currency = "";
        [DataDynamic()]
        public bool SwapRequired = false;
        [DataDynamic()]
        public bool ToCompanies = false;
        [DataDynamic()]
        public bool Objektvision = false;
        [DataDynamic()]
        public bool Bovision = false;
        [DataDynamic(typeof(string))]
        [XmlIgnore]
        public string TypeString = "";

        [XmlIgnore]
        [DataDynamic(typeof(string))]
        protected string contactName = "";
        public string ContactName { get { ProcessContact(); return contactName; } set { contactName = value; } }

        [XmlIgnore]
        [DataDynamic(typeof(string))]
        protected string contactEmail = "";
        public string ContactEmail { get { ProcessContact(); return contactEmail; } set { contactEmail = value; } }

        [XmlIgnore]
        [DataDynamic(typeof(string))]
        protected string contactPhone = "";
        public string ContactPhone { get { ProcessContact(); return contactPhone; } set { contactPhone = value; } }
        [XmlIgnore]
        [DataDynamic(typeof(string))]
        protected string contactCell = "";
        public string ContactCell { get { ProcessContact(); return contactCell; } }

        [XmlIgnore]
        [DataDynamic(typeof(string))]
        protected string contactFax = "";
        public string ContactFax { get { ProcessContact(); return contactFax; } }

        //[XmlIgnore]
        //[DataDynamic()]
        //DateTime Deleted = new DateTime(1900, 1, 1);
        [XmlIgnore]
        [DataDynamic(FieldName = "status", DataType = typeof(short))]
        protected int StatusFlag = 0;

        [DataDynamic()]
        Boolean Active = false;

        private List<BvFile> images = null;
        public List<BvFile> Images { get { return (images == null) ? images = new List<BvFile>() : images; } }


        //private Agent _Agent = null;
        //public Agent Agent { get { return (_Agent == null) ? _Agent = Agent.ById(this.AgentId) : _Agent; } }
        public Agent Agent { get { return Agent.ById(this.AgentId); } }

        //private Municipality _Municipality = null;
        //public Municipality Municipality { get { return (_Municipality != null) ? _Municipality : _Municipality = Municipality.ById(this.MunicipalityCode); } }
        public Municipality Municipality { get { return Municipality.ById(this.MunicipalityCode); } }

        //private County _County = null;
        //public County County { get { return (_County != null) ? _County : (_County = (MunicipalityCode > 0 && MunicipalityCode < 3099 ? County.Counties.ById(MunicipalityCode / 100) : null)); } }
        public County County { get { return (MunicipalityCode > 0 && MunicipalityCode < 3099) ? County.ById(MunicipalityCode / 100) : null; } }

        //private Country _Country = null;
        [XmlIgnore]
        public Country Country { get { return Country.ByISO2Code(CountryCode); } }
        //public Country Country { get { return (_Country != null) ? _Country : (_Country = (CountryCode == "") ? Country.ById("XX") : Country.ById(CountryCode)); } }

        protected string _descriptionUrl = "";
        [DataDynamic("DescriptionUrl")]
        public virtual string DescriptionUrl
        {
            get { return _descriptionUrl; }
            set { _descriptionUrl = value; }
        }
        public string DescriptionPage { get { return "http://bovision.se/Beskriv/" + Id; } }
        public string ImageScale
        {
            get { return (Images.Count == 0) ? "" : string.Format("{3}/Image.aspx?f=/{0}/{1}.{2}", AgentId, Id, HttpUtility.UrlEncode(Images[0].FileName), ServerSettings.ImageServer); }
        }
        public string ThumbNail
        {
            get { return (Images.Count == 0) ? "" : string.Format("{3}/Image.aspx?q=40&amp;hm=75&amp;wm=100&amp;f=/{0}/{1}.{2}", AgentId, Id, HttpUtility.UrlEncode(Images[0].FileName), ServerSettings.ImageServer); }
        }

        public int PricePerSQM
        {
            get
            {
                if (LivingArea > 0)
                    return (int)(Price / LivingArea);
                return 0;
            }
        }

        public string getCurrencyForDisplay()
        {
            return (string.IsNullOrEmpty(Currency) || Currency == "SEK") ? "kr" : Currency;
        }

        public Boolean IsNew
        {
            get { return DateTime.Now.AddDays(-1) < Registered; }
        }
        private static int npyear = DateTime.Now.AddDays(-180).Year;
        public bool IsNewProduction
        {
            get { return npyear >= BuildYear; }
        }

        public static FullEstate ById(int Id)
        {
            return FullEstate.Get(Id);
        }

        public string getDesciptionForBvList()
        {
            string s = (Description != "") ? Description : ExtendedDescription;
            if (s.Length > 150)
            {
                s = s.Substring(0, 150) + "....";
            }
            return s;
        }

        private static Regex rx_htmlstrip = new Regex("<(.|\n)*?>", RegexOptions.Compiled);


        private bool ContactProcessed = false;
        private void ProcessContact()
        {
            if (ContactProcessed == false)
            {
                bool global = !(contactName != "" && (contactEmail != "" || contactPhone != "" || contactCell != ""));
                contactName = (global) ? "" : contactName;
                contactPhone = (global && Agent != null) ? Agent.Phone : contactPhone;
                contactCell = (global) ? "" : contactCell;
                contactEmail = (global && Agent != null) ? Agent.Email : contactEmail;
                contactFax = (global && Agent != null) ? Agent.Fax : contactFax;
                ContactProcessed = true;
            }
        }
        private static CPoint nopoint = new CPoint(0, 0);
        public void Process()
        {
            if (!ShowAddress || Coord == null)
            {
                X = Y = 0;
                Coord = nopoint;
            }
        }
        [XmlIgnore]
        public bool IsUpComing
        {
            get { return StatusFlag == 1337; }
        }
        [XmlIgnore]
        public bool IsExternal { get { return Id < 0; } }
    }
}
