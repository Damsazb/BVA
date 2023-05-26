using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BovisionFacade;
using DomainModel;

namespace Bovision.Import
{
    public class ConvertEstate
    {
        private static EstateForDesctiption CreateEstateForDesctiption(Type t)
        {
            var estate = new DomainModel.EstateForDesctiption();
            estate.InternetDisplay = new List<DomainModel.InternetDisplay>();
            estate.InternetDisplay.Add((DomainModel.InternetDisplay)Activator.CreateInstance(t));
            AppCommon.DomainModelEstateHelper.PrepareEstate(estate, "");
            return estate;
        }
        public static EstateForDesctiption Convert2Estate(XtndEstate xe)
        {
            var est = Convert2Estate(xe as ImportEstate);
            var house = est.InternetDisplay[0] as House;
            if (house != null)
            {
                house.OtherBuildings = xe.OtherBuildings;
            }

            var housing = est.InternetDisplay[0] as Housing;
            if (housing != null)
            {
                housing.Design = xe.Design;
                housing.Equipment = xe.Equipment;
                housing.Surroundings = new Surroundings();
                housing.Surroundings.Nature = xe.Surroundings;
                housing.Surroundings.ParkingAndGarage = xe.Parking;
                housing.OutdoorPlace = xe.OutdoorPlace;
                housing.OtherInfo = xe.OtherInfo;
            }
            return est;
        }
        public static EstateForDesctiption Convert2Estate(ImportEstate ie)
        {
            //This is poorly tested and it probably duplicates some regonline code. Refactor and test!
            if (!EstateType.ValidType(ie.ObjectType, ie.ContractType))
                throw new Exception("Not a valid Type/Contract combination:" + ie.ObjectType + "," + ie.ContractType);
            EstateType type = EstateType.GetEstateType(ie.ObjectType, ie.ContractType);
            var cType = GetDomainEstateContract(type.Type, type.Contract);
            var est = CreateEstateForDesctiption(GetDomainEstateType(type.Type, type.Contract));

            var dmContract = Activator.CreateInstance(cType);
            var disp = est.InternetDisplay[0];

            disp.Status = (ie.Status == EstateStatus.Public.Value) ? DisplayMode.Public : DisplayMode.Private;
            if (ie.AgentId > 0)
                est.Agent = BovisionFacade.EstateLoader.LoadAgent(ie.AgentId);
            est.ServerID = ie.Id;
            est.ClientID = ie.ClientId;
            est.Address.Area = ie.AreaName;
            est.Address.StreetAddress = ie.Address;
            est.Address.PostalCode = ie.ZipCode.ToString();
            est.Address.City = ie.City;
            est.Address.MunicipalityCode = ie.MunicipalityId;
            est.Address.CountryCode = ie.CountryId.ToUpper();

            if (!(ie.Latitude == 0.0 && ie.Longitude == 0.0))
            {
                CPoint p = CPoint.FromUnknown(ie.Latitude, ie.Longitude);
                est.Address.Coordinate.System = CoordinateSystem.RT90;
                est.Address.Coordinate.X = System.Convert.ToDecimal(p.Rt90.X);
                est.Address.Coordinate.Y = System.Convert.ToDecimal(p.Rt90.Y);
            }

            est.DescriptionURL = est.InternetDisplay[0].RedirectURL = ie.DescriptionUrl;
            if (!String.IsNullOrEmpty(ie.Description) && ie.Description.Length >= 150)
                disp.ExtendedDescription = ie.Description;
            else
                disp.Description = ie.Description;
            est.Contact.Name = ie.ContactName;
            est.Contact.Email = ie.ContactEmail;
            est.Contact.Phone = ie.ContactPhone;
            
            var house = disp as House;
            if (house != null)
            {
                if (ie.Type == EstateType.ObjectType.Townhouse)
                    house.Type = HouseType.TerraceHouse;
                else
                    house.Type = HouseType.DetachedHouse;
            }

            var housing = disp as Housing;
            if (housing != null)
            {
                housing.ProjectId = ie.ProjectId;
                housing.BiArea = ie.SideArea;
                housing.BuildYear = ie.BuildYear;
                housing.LivingArea = ie.UsableArea;
                housing.Rooms = ie.Rooms;
                housing.PlotArea = System.Convert.ToInt32(ie.LotArea);

                Contract c = housing.Contract = (ContractNonCommercial)dmContract;
                var rent = c as ContractRent;
                if (rent != null)
                {
                    rent.FeePerMonth = new PriceWithInfo(ie.Rent, ie.Currency, "");

                    if (ie.Contract == EstateType.ContractType.CompanyLet)
                        rent.ContractType = RentType.ForCompanies;
                    else if (ie.Contract == EstateType.ContractType.SubTenancyRights)
                        rent.ContractType = RentType.Subtenancy;
                    else if (ie.Contract == EstateType.ContractType.TenancyRights)
                        rent.ContractType = RentType.TenancyRights;
                    else if (ie.Contract == EstateType.ContractType.StudentTenancy)
                        rent.ContractType = RentType.Student;
                }
                if (c is ContractTenantOwner)
                    (c as ContractTenantOwner).FeePerMonth = new PriceWithInfo(ie.Rent, ie.Currency, "");
                if (c is AbstractSaleContract)
                    (c as AbstractSaleContract).Price = new PriceWithInfo(ie.Price, ie.Currency, "");
            }
            var flat = disp as Flat;
            if (flat != null)
            {
                flat.FloorsInBuilding = (short)ie.FloorsInBuilding;
                flat.Floor = (short)ie.Floor;
                flat.HasElevator = ie.HasElevator;
            }
            var park = disp as Parking;
            if (park != null)
            {
                park.Contract = (ContractRent)dmContract;
                park.Contract.FeePerMonth = new PriceWithInfo(ie.Rent, ie.Currency, "");
                park.Contract.ContractType = RentType.TenancyRights;
            }
            var plot = disp as NonCommercialPlotForSale;
            if (plot != null)
            {
                plot.Contract = (ContractForPlotSale)dmContract;
                plot.PlotInfo.AreaInSquareMeters = System.Convert.ToInt32(ie.LotArea);
                plot.Contract.Price = new PriceWithInfo(ie.Price, ie.Currency, "");
            }
            est.Attachments = new List<AbstractAttachment>();
            int i = 1;
            foreach (var img in ie.Images)
            {
                var aimg = new AttachmentImage();
                aimg.ClientID = "importimage" + (i++).ToString();
                aimg.Category = ImageCategories.Enterior; // have to set one
                if (Uri.IsWellFormedUriString(img, UriKind.Absolute))
                {
                    var rc = new AttachmentRemoteContent();
                    rc.URL = img;
                    aimg.Content = rc;
                }
                else if (Util.ValidateBase64(img))
                {
                    var lc = new AttachmentBase64Content();
                    lc.Base64EncodedContent = img;
                    aimg.Content = lc;
                }
                else
                    throw new Exception("'Image'-tag doesn't have valid content");
                est.Attachments.Add(aimg);
            }
            return est;
        }
        private static Type GetDomainEstateType(EstateType.ObjectType ot, EstateType.ContractType ct)
            {
                switch (ot)
                {
                    case EstateType.ObjectType.Townhouse:
                    case EstateType.ObjectType.House:
                        return typeof(House);
                    case EstateType.ObjectType.Cottage:
                        return typeof(Cottage);
                    case EstateType.ObjectType.Flat:
                        return typeof(Flat);
                    case EstateType.ObjectType.Farm:
                        return typeof(Farm);
                    case EstateType.ObjectType.Lot:
                        return typeof(NonCommercialPlotForSale);
                    case EstateType.ObjectType.Parking:
                        return typeof(Parking);
                    default:
                        throw new Exception("Unknown Estate type:" + ot.ToString());
                }
            }
        private static Type GetDomainEstateContract(EstateType.ObjectType ot, EstateType.ContractType ct)
        {
            switch (ot)
            {
                case EstateType.ObjectType.Townhouse:
                case EstateType.ObjectType.House:
                    switch (ct)
                    {
                        case EstateType.ContractType.StudentTenancy:
                        case EstateType.ContractType.CompanyLet:
                        case EstateType.ContractType.TenancyRights:
                        case EstateType.ContractType.SubTenancyRights: return typeof(ContractRent);
                        case EstateType.ContractType.Sale: return typeof(ContractForSale);
                        case EstateType.ContractType.TenantOwnership: return typeof(ContractTenantOwner);
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case EstateType.ObjectType.Cottage:
                    switch (ct)
                    {
                        case EstateType.ContractType.StudentTenancy:
                        case EstateType.ContractType.CompanyLet:
                        case EstateType.ContractType.TenancyRights:
                        case EstateType.ContractType.SubTenancyRights: return typeof(ContractRent);
                        case EstateType.ContractType.Sale: return typeof(ContractForSale);
                        case EstateType.ContractType.TenantOwnership: return typeof(ContractTenantOwner);
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case EstateType.ObjectType.Flat:
                    switch (ct)
                    {
                        case EstateType.ContractType.StudentTenancy:
                        case EstateType.ContractType.CompanyLet:
                        case EstateType.ContractType.SubTenancyRights:
                        case EstateType.ContractType.TenancyRights:
                            return typeof(ContractRent);
                        case EstateType.ContractType.Sale:
                            return typeof(ContractFlatForSale);
                        case EstateType.ContractType.TenantOwnership:
                            return typeof(ContractTenantOwner);
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case EstateType.ObjectType.Farm:
                    switch (ct)
                    {
                        case EstateType.ContractType.StudentTenancy:
                        case EstateType.ContractType.CompanyLet:
                        case EstateType.ContractType.SubTenancyRights:
                        case EstateType.ContractType.TenancyRights: return typeof(ContractRent);
                        case EstateType.ContractType.Sale: return typeof(ContractForSale);
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case EstateType.ObjectType.Lot:
                    switch (ct)
                    {
                        case EstateType.ContractType.Sale: return typeof(ContractForPlotSale);
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                case EstateType.ObjectType.Parking:
                    switch (ct)
                    {
                        case EstateType.ContractType.StudentTenancy:
                        case EstateType.ContractType.CompanyLet:
                        case EstateType.ContractType.TenancyRights: return typeof(ContractRent);
                        default: throw new Exception("Not valid contract:" + ot.ToString() + "/" + ct.ToString());
                    }
                default:
                    throw new Exception("Unknown Estate type:" + ot.ToString());
            }
        }
    }
}
