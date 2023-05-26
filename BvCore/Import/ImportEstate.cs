using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Bovision
{
    [DataDynamic("v_EstateRemoved")]
    public class DeletedEstate : ImportEstate
    {

    }
    [DataDynamic("v_Estate")]
    public class ImportEstate
    {
        [DataDynamic]
        public int Id = 0;
        [DataDynamic]
        public string ClientId = "";
        [DataDynamic]
        public string ProjectId = null;
        [DataDynamic]
        public string ObjectType { get { return Type.ToString(); } set { Type = Bovision.EstateType.GetObjectType(value); } }
        [DataDynamic]
        public string ContractType { get { return Contract.ToString(); } set { Contract = Bovision.EstateType.GetContractType(value); } }

        public EstateType.ObjectType Type = Bovision.EstateType.ObjectType.Unknown;
        public EstateType.ContractType Contract = Bovision.EstateType.ContractType.Unknown;

        [DataDynamic]
        public bool SwapDemand = false;
        [DataDynamic(typeof(short))]
        public int AgentId = 0;
        [DataDynamic]
        public string AreaName = "";
        [DataDynamic]
        public string Address = "";
        [DataDynamic]
        public int ZipCode = 0;
        [DataDynamic]
        public string City = "";
        [DataDynamic(typeof(short))]
        public int MunicipalityId = 0;
        [DataDynamic]
        public string CountryId = "";
        public string Datum = "wgs84";
        [DataDynamic]
        public double Latitude = 0.0;
        [DataDynamic]
        public double Longitude = 0.0;

        private CPoint coordinate = null;
        public CPoint LatLong {
            get { if (coordinate == null) return coordinate = new CPoint(Latitude, Longitude); return coordinate; }
            set { coordinate = value; Latitude = value.X.DecimalDegree; Longitude = value.Y.DecimalDegree; }
        }

        [DataDynamic]
        public double UsableArea = 0.0;
        [DataDynamic]
        public double SideArea = 0.0;
        [DataDynamic(typeof(int))]
        public double LotArea = 0.0;
        [DataDynamic(typeof(short))]
        public int BuildYear = 0;
        [DataDynamic]
        public double Rooms = 0.0;
        [DataDynamic]
        public string Currency = "SEK";
        [DataDynamic]
        public int Price = 0;
        [DataDynamic]
        public int Rent = 0;
        [DataDynamic(typeof(short))]
        public int FloorsInBuilding = 0;
        [DataDynamic(typeof(short))]
        public int Floor = 0;
        [DataDynamic]
        public bool HasElevator = false;
        [DataDynamic]
        public string Description = "";

        [DataDynamic]
        public string ContactName = "";
        [DataDynamic]
        public string ContactEmail = "";
        [DataDynamic]
        public string ContactPhone = "";
        [DataDynamic("DescriptionUrl")]
        public string DescriptionUrl = "";

        public List<string> Images = new List<string>();
        
        public DateTime DisplayTime = DateTime.MinValue;
        public string DisplayText = "";
        [DataDynamic]
        public DateTime Created = DateTime.MinValue;
        [DataDynamic]
        public DateTime Changed = DateTime.MinValue;
        public ExportAgent Agent;


        [DataDynamic]
        public string Status = Bovision.EstateStatus.Public.Value;
        public bool Active { get { return Status == Bovision.EstateStatus.Public.Value; } }
    }
    [DataDynamic("v_Estate")]
    public class XtndEstate : ImportEstate
    {
        [DataDynamic]
        public string Design = "";
        [DataDynamic]
        public string Surroundings = "";
        [DataDynamic]
        public string Equipment = "";
        [DataDynamic]
        public string OutdoorPlace = "";
        [DataDynamic]
        public string OtherInfo = "";
        [DataDynamic]
        public string Parking = "";
        [DataDynamic]
        public string OtherBuildings = "";
    }
    public class EstateStatus
    {
        public static readonly EstateStatus NotCustomer = new EstateStatus() { Value = "NotCustomer" };
        public static readonly EstateStatus Public = new EstateStatus() { Value = "Public" };
        public static readonly EstateStatus Private = new EstateStatus() { Value = "Private" };
        public static readonly EstateStatus Removed = new EstateStatus() { Value = "Removed" };
        public static readonly EstateStatus DateFuture = new EstateStatus() { Value = "DateFuture" };
        public static readonly EstateStatus DatePassed = new EstateStatus() { Value = "DatePassed" };
        public static readonly EstateStatus Unknown = new EstateStatus() { Value = "Unknown" };
        private static Dictionary<string, EstateStatus> statuses = new Dictionary<string, EstateStatus>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"NotCustomer", NotCustomer },
            {"Public", Public },
            {"Private", Private },
            {"Removed", Removed },
            {"DateFuture", DateFuture },
            {"DatePassed", DatePassed },
            {"Unknown", Unknown }
        };
        public string Value { get; set; }
        public static EstateStatus GetStatus(string status)
        {
            EstateStatus s;
            if (!statuses.TryGetValue(status, out s))
                s = Unknown;
            return s;
        }
        public static bool operator ==(EstateStatus a, EstateStatus b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.Value == b.Value;
        }
        public static bool operator !=(EstateStatus a, EstateStatus b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var c = obj as Currency;
                if (c != null)
                    return Value.Equals(c.Code);
            }
            return false;
        }
        public override string ToString()
        {
            return Value;
        }
    }
    [DataDynamic("v_Agent")]
    public class ExportAgent
    {
        [DataDynamic]
        public int Id;
        [DataDynamic]
        public string Name;
        [DataDynamic]
        public string Address;
        [DataDynamic]
        public string ZipCode;
        [DataDynamic]
        public string City;
        [DataDynamic]
        public string HomePage;
        [DataDynamic]
        public string Logo;
        [DataDynamic]
        public string Phone;
        [DataDynamic]
        public string Email;
        [DataDynamic]
        public DateTime Changed;
    }
}
