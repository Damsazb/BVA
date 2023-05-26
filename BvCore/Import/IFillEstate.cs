using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bovision.Import;

namespace Bovision
{
    public class XmlConvInfo
    {
        public Credentials Credentials;
        public Uri ExternalXml;
    }
    public interface IFillEstate
    {
        IEnumerable<XtndEstate> GetEstates();
    }
    public abstract class FillEstate<T> : FillXtndEstate<Tag>, IFillEstate
    {
        public override bool SwapDemand() { return false; }
        public override string ProjectId() { return null; }
        public override string Design() { return string.Empty; }
        public override string Surroundings() { return string.Empty; }
        public override string Equipment() { return string.Empty; }
        public override string OutdoorPlace() { return string.Empty; }
        public override string OtherInfo() { return string.Empty; }
        public override string Parking() { return string.Empty; }
        public override string OtherBuildings() { return string.Empty; }
        public override DateTime DisplayTime() { return DateTime.MinValue; }
        public override string DisplayText() { return string.Empty; }
        public override string Status() { return EstateStatus.Public.Value; }
    }
    public abstract class FillXtndEstate<T> : IFillEstate
    {
        public abstract IEnumerable<XtndEstate> GetEstates();
        public abstract string ClientId();
        public abstract string ProjectId();
        public abstract EstateType.ObjectType Type();
        public abstract EstateType.ContractType Contract();
        public abstract bool SwapDemand();
        public abstract string AreaName();
        public abstract string Address();
        public abstract int ZipCode();
        public abstract string City();
        public abstract int MunicipalityId();
        public abstract string CountryId();
        public abstract CPoint LatLong();
        public abstract double UsableArea();
        public abstract double SideArea();
        public abstract double LotArea();
        public abstract int BuildYear();
        public abstract double Rooms();
        public abstract string Currency();
        public abstract int Price();
        public abstract int Rent();
        public abstract int FloorsInBuilding();
        public abstract int Floor();
        public abstract bool HasElevator();
        public abstract string Description();
        public abstract string ContactName();
        public abstract string ContactEmail();
        public abstract string ContactPhone();
        public abstract string DescriptionUrl();
        public abstract List<string> Images();
        public abstract string Design();
        public abstract string Surroundings();
        public abstract string Equipment();
        public abstract string OutdoorPlace();
        public abstract string OtherInfo();
        public abstract string Parking();
        public abstract string OtherBuildings();
        public abstract DateTime DisplayTime();
        public abstract string DisplayText();
        public abstract string Status();
        protected XtndEstate Convert()
        {
            var est = new XtndEstate();
            est.ClientId = ClientId();
            est.ProjectId = ProjectId();
            est.Type = Type();
            est.Contract = Contract();
            est.SwapDemand = SwapDemand();
            est.AreaName = AreaName();
            est.Address = Address();
            est.ZipCode = ZipCode();
            est.City = City();
            est.MunicipalityId = MunicipalityId();
            est.CountryId = CountryId();
            est.LatLong = LatLong();
            est.UsableArea = UsableArea();
            est.SideArea = SideArea();
            est.LotArea = LotArea();
            est.BuildYear = BuildYear();
            est.Rooms = Rooms();
            est.Currency = Currency();
            est.Price = Price();
            est.Rent = Rent();
            est.FloorsInBuilding = FloorsInBuilding();
            est.Floor = Floor();
            est.HasElevator = HasElevator();
            est.Description = Description();
            est.ContactName = ContactName();
            est.ContactEmail = ContactEmail();
            est.ContactPhone = ContactPhone();
            est.DescriptionUrl = DescriptionUrl();
            est.Images = Images();
            est.Design = Design();
            est.Surroundings = Surroundings();
            est.Equipment = Equipment();
            est.OutdoorPlace = OutdoorPlace();
            est.OtherInfo = OtherInfo();
            est.Parking = Parking();
            est.OtherBuildings = OtherBuildings();
            est.DisplayTime = DisplayTime();
            est.DisplayText = DisplayText();
            est.Status = Status();
            return est;
        }
    }
}
