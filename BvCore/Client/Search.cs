using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bovision.Client
{
    public class Search
    {
        private static XmlReaderSettings settings = new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true, CloseInput = true };
        public static string MasterKey = "QJ3GlGzVclU1";
        //public static string Url = "http://localhost:23445/api/Export/";
        public static string Url = "http://services.bovision.se/api/Export/";
        public Query Query;
        public Search(System.IO.Stream stream)
        {
            ParseStream(stream);
        }
        public Search(string Key, Query query, bool usePaging = false)
        {
            this.Query = query;
            if (query.Page == 0)
                query.Page = 1;
            using (var wc = new System.Net.WebClient())
            {
                var sb = new StringBuilder();
                sb.Append(Url).Append(Key).Append("?includeAgents=Unique&");
                query.ToString(sb, false);
                var stream = wc.OpenRead(sb.ToString());
                ParseStream(stream);
            }
        }
        public PagerHelper GetPagerHelper()
        {
            return new PagerHelper(this.Pages, this.CurrentPage);
        }
        private void ParseStream(System.IO.Stream stream)
        {
            var r = XmlReader.Create(stream, settings);
            Parse(r);
            if (agents.Count > 0)
                foreach (var est in estates)
                    agents.TryGetValue(est.AgentId, out est.Agent);
        }
        protected enum ParserNS { Root, Estate, Agent }
        protected Dictionary<int, Agent> agents = new Dictionary<int, Agent>();
        protected List<Estate> estates = new List<Estate>();

        public IReadOnlyList<Estate> Estates { get { return estates; } }
        public IReadOnlyDictionary<int, Agent> Agents { get { return agents; } }
        public int Count { get { return estates.Count; } }
        public int TotalCount { get; private set; }
        public int ItemsPerPage { get; private set; }
        public int Pages { get; private set; }
        public int CurrentPage { get; private set; }

        protected void Parse(XmlReader r)
        {
            ParserNS CurrentNS = ParserNS.Root;
            ParserNS ParentNS = ParserNS.Root;
            Estate est = null;
            Agent agent = null;
            string elm = null;
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Element)
                {
                    elm = r.Name;
                    if (elm == "Estate")
                    {
                        CurrentNS = ParserNS.Estate;
                        estates.Add(est = new Estate());
                    }
                    else if (elm == "Location")
                    {
                        string datum = r.GetAttribute("datum");
                        if (datum == "wgs84" || datum == "rt90" || datum == "sweref99tm")
                            est.Datum = r.GetAttribute("datum");
                    }
                    else if (elm == "Agent")
                    {
                        ParentNS = CurrentNS;
                        CurrentNS = ParserNS.Agent;
                        agent = new Agent();
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    if (r.Name == "Estate")
                        CurrentNS = ParserNS.Root;
                    else if (r.Name == "Agent")
                        CurrentNS = ParentNS;
                }
                else if (r.NodeType == XmlNodeType.Text || r.NodeType == XmlNodeType.CDATA)
                {
                    if (CurrentNS == ParserNS.Root)
                    {
                        switch (elm)
                        {
                            case "TotalCount": TotalCount = Util.atoi(r.Value); break;
                            case "ItemsPerPage": ItemsPerPage = Util.atoi(r.Value); break;
                            case "Pages": Pages = Util.atoi(r.Value); break;
                            case "CurrentPage": CurrentPage = Util.atoi(r.Value); break;
                        }

                    }
                    if (CurrentNS == ParserNS.Agent)
                    {
                        switch(elm)
                        {
                            case "Id": agent.Id = Util.atoi(r.Value); agents[agent.Id] = agent; break;
                            case "Name" : agent.Name = r.Value; break;
                            case "Address" : agent.Address = r.Value; break;
                            case "ZipCode" : agent.ZipCode = r.Value; break;
                            case "City" : agent.City = r.Value; break;
                            case "HomePage" : agent.HomePage = r.Value; break;
                            case "Logo" : agent.Logo = r.Value; break;
                            case "Phone" : agent.Phone = r.Value; break;
                            case "Email" : agent.Email = r.Value; break;
                        }
                    }
                    else if (CurrentNS == ParserNS.Estate)
                    {
                        switch(elm)
                        {
                            case "Id": est.Id = Util.atoi(r.Value); break;
                            case "ClientId" : est.ClientId = r.Value; break;
                            case "ProjectId" : est.ProjectId = r.Value; break;
                            case "AgentId" : est.AgentId = Util.atoi(r.Value); break;
                            case "EstateType" : est.EstateType = r.Value; break;
                            case "EstateContract" : est.EstateContract = r.Value; break;
                            case "AreaName" : est.AreaName = r.Value; break;
                            case "Address" : est.Address = r.Value; break;
                            case "ZipCode" : est.ZipCode = r.Value; break;
                            case "City" : est.City = r.Value; break;
                            case "MunicipalityId" : est.MunicipalityId = Util.atoi(r.Value); break;
                            case "CountryId" : est.CountryId = r.Value; break;
                            case "Latitude" : est.Latitude = Util.atod(r.Value); break;
                            case "Longitude" : est.Longitude = Util.atod(r.Value); break;
                            case "UsableArea" : est.UsableArea = Util.atod(r.Value); break;
                            case "SideArea": est.SideArea = Util.atod(r.Value); break;
                            case "BuildYear": est.BuildYear = Util.atoi(r.Value); break;
                            case "Rooms" :  est.Rooms = Util.atod(r.Value); break;
                            case "LotArea" : est.LotArea = Util.atod(r.Value); break;
                            case "Currency" : est.Currency = r.Value; break;
                            case "Price" : est.Price = Util.atoi(r.Value); break;
                            case "Rent" : est.Rent = Util.atoi(r.Value); break;
                            case "Description" : est.Description = r.Value; break;
                            case "Floor": est.Floor = Util.atoi(r.Value); break;
                            case "FloorsInBuilding": est.FloorsInBuilding = Util.atoi(r.Value); break;
                            case "HasElevator": est.HasElevator = Util.Bool(r.Value); break;
                            case "Created" : est.Created = DateTime.Parse(r.Value); break;
                            case "Changed" : est.Changed = DateTime.Parse(r.Value); break;
                            case "Display" : if (!string.IsNullOrEmpty(r.Value)) DateTime.TryParse(r.Value, out est.DisplayTime); break;
                            case "ContactName" : est.ContactName = r.Value; break;
                            case "ContactEmail" : est.ContactEmail = r.Value; break;
                            case "ContactPhone" : est.ContactPhone = r.Value; break;
                            case "DescriptionUrl" : est.DescriptionUrl = r.Value; break;
                            case "Image":
                                if(est.Images.Count < 3)
                                    est.Images.Add(r.Value);
                                break;
                            case "Design": est.Design = r.Value; break;
                            case "Surroundings": est.Surroundings = r.Value; break;
                            case "Equipment": est.Equipment = r.Value; break;
                            case "OutdoorPlace": est.OutdoorPlace = r.Value; break;
                            case "OtherInfo": est.OtherInfo = r.Value; break;
                            case "Parking": est.Parking = r.Value; break;
                            case "OtherBuildings": est.OtherBuildings = r.Value; break;
                        }
                    }
                }
            }
        }
    }
    public class Estate
    {
        public int Id = 0;
        public string ClientId = "";
        public string ProjectId = null;
        public string EstateType;
        public string EstateContract;
        public bool SwapDemand = false;
        public int AgentId = 0;
        public string AreaName;
        public string Address = "";
        public string ZipCode = "";
        public string City = "";
        public int MunicipalityId = 0;
        public string CountryId = "";
        public string Datum = "wgs84";
        public double Latitude = 0.0;
        public double Longitude = 0.0;
        public double UsableArea = 0.0;
        public double SideArea = 0.0;
        public double LotArea = 0.0;
        public int BuildYear = 0;
        public double Rooms = 0.0;
        public string Currency = "";
        public int Price = 0;
        public int Rent = 0;
        public int Floor = 0;
        public int FloorsInBuilding = 0;
        public bool HasElevator = false;
        public string Description = "";
        public string Design = "";
        public string Surroundings = "";
        public string Equipment = "";
        public string OutdoorPlace = "";
        public string OtherInfo = "";
        public string Parking = "";
        public string OtherBuildings = "";

        public string ContactName = "";
        public string ContactEmail = "";
        public string ContactPhone = "";
        public string DescriptionUrl = "";
        public string FrameUrl = "";
        public List<string> Images = new List<string>();
        public DateTime DisplayTime = DateTime.MinValue;
        public string DisplayText = "";
        public DateTime Created = DateTime.MinValue;
        public DateTime Changed = DateTime.MinValue;
        public Agent Agent = null;


        public Municipality Municipality { get { return Municipality.ById(MunicipalityId); } }
        public EstateType Type { get { return Bovision.EstateType.GetEstateType(this.EstateType, this.EstateContract); } }

    }
    public class Agent
    {
        public int Id;
        public string Name;
        public string Address;
        public string ZipCode;
        public string City;
        public string HomePage;
        public string Logo;
        public string Phone;
        public string Email;
    }
}
