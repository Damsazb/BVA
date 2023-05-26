using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public class SearchTypes
    {
        private static Dictionary<string, SearchType> storage = new Dictionary<string, SearchType>(StringComparer.OrdinalIgnoreCase);
        static SearchTypes()
        {
            Add(SearchType.All);
            Add(SearchType.House, SearchType.HouseSale, SearchType.HouseTenantOwnership, SearchType.HouseTenancyRights, SearchType.HouseSubTenancyRights, SearchType.HouseStudentTenancy, SearchType.HouseCompanyLet);
            Add(SearchType.Cottage, SearchType.CottageSale, SearchType.CottageTenantOwnership, SearchType.CottageTenancyRights, SearchType.CottageSubTenancyRights, SearchType.CottageStudentTenancy, SearchType.CottageCompanyLet);
            Add(SearchType.Flat, SearchType.FlatSale, SearchType.FlatTenantOwnership, SearchType.FlatTenancyRights, SearchType.FlatSubTenancyRights, SearchType.FlatStudentTenancy, SearchType.FlatCompanyLet);
            Add(SearchType.Farm, SearchType.FarmSale, SearchType.FarmTenantOwnership, SearchType.FarmTenancyRights, SearchType.FarmSubTenancyRights, SearchType.FarmCompanyLet);
            Add(SearchType.Lot, SearchType.LotSale);
            Add(SearchType.Parking, SearchType.ParkingTenancyRights);
            Add(SearchType.Sale, SearchType.TenancyRights, SearchType.TenantOwnership, SearchType.SubTenancyRights, SearchType.StudentTenancy, SearchType.CompanyLet);
            storage["Rent"] = SearchType.TenancyRights;
        }
        private static void Add(params SearchType []types)
        {
            foreach (var t in types)
            {
                storage[t.EstateType.ToString()] = t;
                if (t.EstateType.Type == Bovision.EstateType.ObjectType.Unknown && t.EstateType.Contract == Bovision.EstateType.ContractType.Unknown)
                    storage["All"] = t;
                else if (t.EstateType.Type == Bovision.EstateType.ObjectType.Unknown)
                {
                    var contract = t.EstateType.Contract.ToString();
                    storage["All:" + contract] = storage[contract] = t;
                }
                else if (t.EstateType.Contract == Bovision.EstateType.ContractType.Unknown)
                {
                    var type = t.EstateType.Type.ToString();
                    storage[type+":All"] = storage[type] = t;
                }
            }
        }
        public static bool TryParse(string s, out SearchType t)
        {
            return (storage.TryGetValue(s, out t));
        }
        public static IEnumerable<string> TypeNames()
        {
            return storage.Keys;
        }
    }
    public class SearchType
    {
        public static SearchType All = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Unknown, EstateType.ContractType.Unknown), "Alla typer");

        public static SearchType House = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.House, EstateType.ContractType.Unknown), "Hus");
        public static SearchType HouseSale = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.House, EstateType.ContractType.Sale), "Hus till salu");
        public static SearchType HouseTenantOwnership = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.House, EstateType.ContractType.TenantOwnership), "Bostadsrättshus");
        public static SearchType HouseTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.House, EstateType.ContractType.TenancyRights), "Hus uthyres");
        public static SearchType HouseSubTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.House, EstateType.ContractType.SubTenancyRights), "Hus uthyrning 2:a hand");
        public static SearchType HouseStudentTenancy = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.House, EstateType.ContractType.StudentTenancy), "Hus till student");
        public static SearchType HouseCompanyLet = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.House, EstateType.ContractType.CompanyLet), "Hus företagsuthyrning");

        public static SearchType Cottage = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Cottage, EstateType.ContractType.Unknown), "Fritidshus");
        public static SearchType CottageSale = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Cottage, EstateType.ContractType.Sale), "Fritidshus till salu");
        public static SearchType CottageTenantOwnership = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Cottage, EstateType.ContractType.TenantOwnership), "Fritidshus bostadsrätt");
        public static SearchType CottageTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Cottage, EstateType.ContractType.TenancyRights), "Fritidshus uthyres");
        public static SearchType CottageSubTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Cottage, EstateType.ContractType.SubTenancyRights), "Fritidshus uthyrning 2:a hand");
        public static SearchType CottageStudentTenancy = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Cottage, EstateType.ContractType.StudentTenancy), "Fritidshus till student");
        public static SearchType CottageCompanyLet = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Cottage, EstateType.ContractType.CompanyLet), "Fritidshus företagsuthyrning");

        public static SearchType Flat = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.Unknown), "Lägenhet");
        public static SearchType FlatSale = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.Sale), "Lägenhet till salu");
        public static SearchType FlatTenantOwnership = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.TenantOwnership), "Bostadsrättslägenhet");
        public static SearchType FlatTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.TenancyRights), "Lägenhet uthyres");
        public static SearchType FlatSubTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.SubTenancyRights), "Lägenhet uthyrning 2:a hand");
        public static SearchType FlatStudentTenancy = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.StudentTenancy), "Lägenhet till student");
        public static SearchType FlatCompanyLet = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.CompanyLet), "Lägenhet företagsuthyrning");

        public static SearchType Farm = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.Unknown), "Lantbruk");
        public static SearchType FarmSale = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.Sale), "Lantbruk till salu");
        public static SearchType FarmTenantOwnership = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.TenantOwnership), "Lantbruk Bostadsrätt");
        public static SearchType FarmTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.TenancyRights), "Lantbruk uthyres");
        public static SearchType FarmSubTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.SubTenancyRights), "Lantbruk uthyrning 2:a hand");
        public static SearchType FarmCompanyLet = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Flat, EstateType.ContractType.CompanyLet), "Lantbruk företagsuthyrning");

        public static SearchType Lot = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Lot, EstateType.ContractType.Unknown), "Tomt");
        public static SearchType LotSale = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Lot, EstateType.ContractType.Sale), "Tomt");

        public static SearchType Parking = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Parking, EstateType.ContractType.Unknown), "Parkering");
        public static SearchType ParkingTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Parking, EstateType.ContractType.TenancyRights), "Parkering uthyres");

        public static SearchType Sale = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Unknown, EstateType.ContractType.Sale), "Till salu");
        public static SearchType TenantOwnership = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Unknown, EstateType.ContractType.TenantOwnership), "Bostadsrätt");
        public static SearchType TenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Unknown, EstateType.ContractType.TenancyRights), "Uthyres");
        public static SearchType SubTenancyRights = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Unknown, EstateType.ContractType.SubTenancyRights), "Uthyres 2:a hand");
        public static SearchType StudentTenancy = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Unknown, EstateType.ContractType.StudentTenancy), "Uthyres studenter");
        public static SearchType CompanyLet = new SearchType(EstateType.GetEstateType(EstateType.ObjectType.Unknown, EstateType.ContractType.CompanyLet), "Till företag");

        public string Name { get; private set; }
        public string FullName { get; private set; }
        public EstateType EstateType { get; private set; }

        private SearchType(EstateType type, string description) : this(null, type, description) { }
        private SearchType(string name, EstateType type, string description)
        {
            var t = type.Type == Bovision.EstateType.ObjectType.Unknown ? null : type.Type.ToString();
            var c = type.Contract == Bovision.EstateType.ContractType.Unknown ? null : type.Contract.ToString();

            if (type.Type == Bovision.EstateType.ObjectType.Unknown && type.Contract == Bovision.EstateType.ContractType.Unknown)
                Name = "All";
            else if (type.Type == Bovision.EstateType.ObjectType.Unknown)
                Name = type.Contract.ToString();
            else if (type.Contract == Bovision.EstateType.ContractType.Unknown)
                Name = type.Type.ToString();
            else
                Name = type.Type.ToString();
            this.FullName = description;
            this.EstateType = type;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
