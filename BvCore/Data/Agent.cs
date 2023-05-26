using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bovision
{
    public interface IPackage : IComparable<IPackage>
    {
        string Package { get; }
    }
    public class BVPackage : IPackage
    {
        public string Package { get; private set; }
        public BVPackage(string s)
        {
            Package = s;
        }
        public int CompareTo(IPackage oOther)
        {
            if (oOther == null)
                return 1;
            return this.Package.CompareTo(oOther.Package);
        }
        public static bool operator ==(BVPackage a, BVPackage b)
        {
            return a.Package.Equals(b.Package);
        }
        public static bool operator !=(BVPackage a, BVPackage b)
        {
            return !a.Package.Equals(b.Package);
        }
        public override string ToString()
        {
            return Package;
        }
        public override int GetHashCode()
        {
            return Package.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return (obj is BVPackage && (obj as BVPackage) == this);
        }
        public bool ExtendedList { get { return Package == "Plus" || Package == "PlusPlus"; } }
    }
    
    [DataDynamic("V_Agents")]
    public class Agent : _Agent<Agent>
    {
        public static Agent ById(int Id)
        {
            return Agent.Get(Id);
        }

        [DataDynamic("DescriptionTemplate")]
        protected string DescriptionTemplate = "";

        public string ResolveDescriptionUrl(FullEstate est)
        {
            if (!String.IsNullOrEmpty(est.DescriptionUrl))
                return est.DescriptionUrl;
            if (!String.IsNullOrEmpty(DescriptionTemplate))
            {
                return DescriptionTemplate.Replace("[BVNR]", est.Id.ToString()).Replace("[OBJEKTID]", est.ClientId);
            }
            return "";// Util.GetHostAndPort() + "/Description/DescriptionBody.aspx?guid=" + est.Id;
        }

        private List<FullEstate> _Estates = null;
        public List<FullEstate> GetEstates() { return (_Estates != null) ? _Estates : _Estates = FullEstate.Find(Expr.Eq("AgentId", this.Id)); }
        public bool CanLoginOnBV()
        {
            return (Bovision || HouseProducer) && BVPackage.Package.ToLower() != "bas";
        }

        public bool IsPayingOnBV()
        {
            return (Bovision || HouseProducer) && BovisionPublic && BVPackage.Package.ToLower() != "bas";
        }

        [XmlIgnore]
        private BVPackage bvpackage = null;
        [XmlIgnore]
        public IPackage BVPackage
        {
            get
            {
                bvpackage = bvpackage ?? new BVPackage(sBVPackage);
                return bvpackage;
            }
        }
    }
    public abstract class _Agent<T> : BaseDataReadWriteId<T>, ILocalID where T : _Agent<T>, new()
    {
        [DataDynamic("N_MAKLARID", PrimaryKey = true, DataType = typeof(Int16))]
        public int Id { get; set; }
        [DataDynamic("S_MAKLARE")]
        public string Name = "";
        [DataDynamic("S_NAMN")]
        public string Contact = "";
        [DataDynamic("S_ADRESS")]
        public string Address = "";
        [DataDynamic("L_POSTNR")]
        public int ZipCode = 0;
        [DataDynamic("S_POSTADRESS")]
        public string City = "";
        [DataDynamic("S_TELE")]
        public string Phone = "";
        [DataDynamic("S_FAX")]
        public string Fax = "";
        [DataDynamic("S_EMAIL")]
        public string Email = "";
        [DataDynamic("S_EMAILUTSKICK")]
        public string EmailNewsletter = "";
        [XmlIgnore]
        [DataDynamic("S_WWWADRESS")]
        protected string homePage = "";
        public string HomePage { get { return Util.EnsureHttp(homePage); } }
        [XmlIgnore]
        [DataDynamic("b_nyproducent", DataType = typeof(byte))]
        public bool HouseProducer = false;
        [XmlIgnore]
        [DataDynamic("B_BOVISION", DataType = typeof(Int16))]
        public bool Bovision = false;
        [XmlIgnore]
        [DataDynamic("B_objektvision", DataType = typeof(Int16))]
        public bool Objektvision = false;
        [DataDynamic("S_LOGGA")]
        protected string logo = "";
        public string Logo { get { return (logo == "") ? "" : String.Format("http://files.bovision.se/Bilder{0}.scale", logo); } }
        [XmlIgnore]
        [DataDynamic("S_LOSEN")]
        public string Password = "";
        [XmlIgnore]
        [DataDynamic("S_LOSENTJANSTER")]
        public string PasswordServices = "";
        [XmlIgnore]
        [DataDynamic("B_PUBLIK", DataType = typeof(Int16))]
        public bool BovisionPublic = true;
        [XmlIgnore]
        [DataDynamic("B_OVPUBLIK", DataType = typeof(Int16))]
        public bool ObjektvisionPublic = true;
        [XmlIgnore]
        [DataDynamic("OV_PAKET")]
        protected string sOVPackage = "";
        [XmlIgnore]
        [DataDynamic("BV_PAKET")]
        public string sBVPackage = "Bas";
        [XmlIgnore]
        [DataDynamic("Changed")]
        public DateTime Changed;
    }
}
