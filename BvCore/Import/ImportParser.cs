using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Bovision.Net;

namespace Bovision.Import
{
    public class ImportSettings
    {
        public bool TryCorrectIssues = true;
        public bool ImportAllImages = false;
        public List<int> AgentDuplicateCheck = new List<int>();
    }

    public enum ParserResultStatus { NotValid, Parsed };
    public class ParserResult
    {
        public ParserResultStatus Status = ParserResultStatus.Parsed;
        public List<string> Messages = new List<string>();
        public XtndEstate Estate = null;
        public string ClientId = "N/A";
        public ParserResult() { Estate = new XtndEstate(); Status = ParserResultStatus.Parsed; }
        public ParserResult(string ClientId, XtndEstate est, ParserResultStatus Status)
        {
            this.Estate = est;
            this.Status = Status;
        }
        public void SetError(string msg)
        {
            Status = ParserResultStatus.NotValid;
            Messages.Add(msg);
        }
    }
    public class ExportParser
    {
        public IEnumerable<ImportEstate> Estates { get { return Result.Where(r => r.Status == ParserResultStatus.Parsed).Select(r => r.Estate); } }
        protected Dictionary<int, ExportAgent> Agents = new Dictionary<int, ExportAgent>();
        public List<ParserResult> Result = new List<ParserResult>();
        protected ParserResult res;
        protected ExportAgent Agent = null;
        public ExportParser() { }
        public ExportParser(Stream s)
        {
            var r = XmlTextReader.Create(s, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
            while(r.Read())
                Parse(r);
            r.Close();
            if(Agents.Count > 0)
            {
                ExportAgent a;
                foreach (var est in Estates)
                    if (Agents.TryGetValue(est.AgentId, out a))
                        est.Agent = a;
            }
        }
        
        protected enum ParserNS { Root, Estate, Agent, Images, Meta }
        protected Stack<ParserNS> ns = new Stack<ParserNS>();
        protected ParserNS CurrentNS = ParserNS.Root;
        private string elm = null;
        protected virtual void Parse(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.Element)
            {
                elm = r.Name;
                if (elm == "Estate")
                {
                    ns.Push(CurrentNS);
                    CurrentNS = ParserNS.Estate;
                    res = new ParserResult();
                }
                else if (elm == "Images")
                {
                    CurrentNS = ParserNS.Images;
                }
                else if (elm == "Location")
                {
                    res.Estate.Datum = r.GetAttribute("datum");
                }
                else if (elm == "Agent")
                {
                    ns.Push(CurrentNS);
                    CurrentNS = ParserNS.Agent;
                    if (Agent != null)
                        Agents[Agent.Id] = Agent;
                    Agent = new ExportAgent();
                }
                else if (elm == "Meta")
                {
                    ns.Push(CurrentNS);
                    CurrentNS = ParserNS.Meta;
                }
            }
            else if( r.NodeType == XmlNodeType.EndElement)
            {
                if (r.Name == "Estate")
                {
                    Result.Add(res);
                    CurrentNS = ns.Pop();
                }
                else if (r.Name == "Agent")
                    CurrentNS = ns.Pop();
                else if (r.Name == "Images")
                    CurrentNS = ParserNS.Estate;
                else if (r.Name == "Meta")
                    CurrentNS = ns.Pop();
            }
            else if (r.NodeType == XmlNodeType.Text || r.NodeType == XmlNodeType.CDATA)
            {
                if (CurrentNS == ParserNS.Agent)
                {
                    switch (elm)
                    {
                        case "Id":      Agent.Id = iValue(res, "Id", r.Value, true); break;
                        case "Name":    Agent.Name = r.Value; break;
                        case "Address": Agent.Address = r.Value; break;
                        case "ZipCode": Agent.ZipCode = r.Value; break;
                        case "City":    Agent.City = r.Value; break;
                        case "HomePage": Agent.HomePage = r.Value; break;
                        case "Logo":    Agent.Logo = r.Value; break;
                        case "Phone":   Agent.Phone = r.Value; break;
                        case "Email":   Agent.Email = r.Value; break;
                    }
                }
                else if (CurrentNS == ParserNS.Estate)
                {
                    switch(elm)
                    {
                        case "Id": res.Estate.Id = iValue(res, elm, r.Value, false); break;
                        case "ClientId":
                            res.ClientId = r.Value;
                            res.Estate.ClientId = sValue(res, elm, r.Value, true);
                            break;
                        case "ProjectId":
                            res.Estate.ProjectId = sValue(res, elm, r.Value, true); break;
                        case "AgentId":
                            res.Estate.AgentId = iValue(res, elm, r.Value, false); break;
                        case "EstateType":
                            if (!EstateType.ValidObjectType(r.Value))
                                res.SetError("EstateType is not valid:" + r.Value);
                            res.Estate.ObjectType = r.Value;
                            break;
                        case "EstateContract":
                            if (!EstateType.ValidContractType(r.Value))
                                res.SetError("EstateContract is not valid:" + r.Value);
                            res.Estate.ContractType = r.Value;
                            break;
                        case "AreaName":
                            res.Estate.AreaName = sValue(res, elm, r.Value, true); break;
                        case "Address":
                            res.Estate.Address = sValue(res, elm, r.Value, true); break;
                        case "ZipCode":
                            res.Estate.ZipCode = iValue(res, elm, r.Value, false); break;
                        case "City":
                            res.Estate.City = sValue(res, elm, r.Value, true); break;
                        case "MunicipalityId":
                            res.Estate.MunicipalityId = iValue(res, elm, r.Value, true); break;
                        case "CountryId":
                            res.Estate.CountryId = sValue(res, elm, r.Value, true); break;
                    }
                    if (elm == "Latitude")
                        res.Estate.Latitude = dValue(res, elm, r.Value, false);
                    else if (elm == "Longitude")
                        res.Estate.Longitude = dValue(res, elm, r.Value, false);
                    else if (elm == "UsableArea")
                        res.Estate.UsableArea = dValue(res, elm, r.Value, false);
                    else if (elm == "SideArea")
                        res.Estate.SideArea = dValue(res, elm, r.Value, false);
                    else if (elm == "BuildYear")
                        res.Estate.BuildYear = iValue(res, elm, r.Value, false);
                    else if (elm == "Rooms")
                        res.Estate.Rooms = dValue(res, elm, r.Value, false);
                    else if (elm == "FloorsInBuilding")
                        res.Estate.FloorsInBuilding = iValue(res, elm, r.Value, false);
                    else if (elm == "Floor")
                        res.Estate.Floor = iValue(res, elm, r.Value, false);
                    else if (elm == "Currency")
                        res.Estate.Currency = sValue(res, elm, r.Value, true);
                    else if (elm == "Price")
                        res.Estate.Price = iValue(res, elm, r.Value, false);
                    else if (elm == "Rent")
                        res.Estate.Rent = iValue(res, elm, r.Value, false);
                    else if (elm == "Description")
                        res.Estate.Description = sValue(res, elm, r.Value, false);
                    else if (elm == "Design")
                        res.Estate.Design = sValue(res, elm, r.Value, false);
                    else if (elm == "Surroundings")
                        res.Estate.Surroundings = sValue(res, elm, r.Value, false);
                    else if (elm == "Equipment")
                        res.Estate.Equipment = sValue(res, elm, r.Value, false);
                    else if (elm == "OutdoorPlace")
                        res.Estate.OutdoorPlace = sValue(res, elm, r.Value, false);
                    else if (elm == "OtherInfo")
                        res.Estate.OtherInfo = sValue(res, elm, r.Value, false);
                    else if (elm == "Parking")
                        res.Estate.Parking = sValue(res, elm, r.Value, false);
                    else if (elm == "OtherBuildings")
                        res.Estate.OtherBuildings = sValue(res, elm, r.Value, false);
                    else if (elm == "Display")
                    {
                        if (!string.IsNullOrEmpty(r.Value))
                            if (!DateTime.TryParse(r.Value, out res.Estate.DisplayTime))
                                res.SetError(elm + " must be a valid DateTime value in ISO 8601 format");
                    }
                    else if (elm == "ContactName")
                        res.Estate.ContactName = sValue(res, elm, r.Value, false);
                    else if (elm == "ContactEmail")
                        res.Estate.ContactEmail = sValue(res, elm, r.Value, false);
                    else if (elm == "ContactPhone")
                        res.Estate.ContactPhone = sValue(res, elm, r.Value, false);
                    else if (elm == "DescriptionUrl")
                        res.Estate.DescriptionUrl = sValue(res, elm, r.Value, false);
                    else if (elm == "Image")
                    {
                        if (!(Uri.IsWellFormedUriString(r.Value, UriKind.Absolute) ||
                            Util.ValidateBase64(r.Value)))
                            res.SetError(elm + " must be a valid Uri or in a valid Base64 format.");
                        res.Estate.Images.Add(sValue(res, elm, r.Value, true));
                    }
                    else if (elm == "Status")
                        res.Estate.Status = EstateStatus.GetStatus(r.Value).Value;
                }
                else if (CurrentNS == ParserNS.Images)
                {
                    if (elm == "Image")
                    {
                        if (!(Uri.IsWellFormedUriString(r.Value, UriKind.Absolute) ||
                            Util.ValidateBase64(r.Value)))
                            res.SetError(elm + " must be a valid Uri or in a valid Base64 format.");
                        res.Estate.Images.Add(sValue(res, elm, r.Value, true));
                    }
                }
            }
        }
        protected int iValue(ParserResult res, string name, string v, bool req)
        {
            if (v == "" && !req)
                return 0;
            int i;
            if (!Int32.TryParse(v, out i))
                res.SetError(String.Format("'{0}' must be an integer type.", name));
            return i;
        }
        protected int uiValue(ParserResult res, string name, string v, bool req)
        {
            if (v == "" && !req)
                return 0;
            int i;
            if (!Int32.TryParse(v, out i))
                res.SetError(String.Format("'{0}' must be an integer type.", name));
            if (i < 0)
                res.SetError(String.Format("'{0}' may not be negative.", name));
            return i;
        }
        protected double dValue(ParserResult res, string name, string v, bool req)
        {
            if (v == "" && !req)
                return 0.0;
            double i;
            if (!Double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
                res.SetError(String.Format("'{0}' must be a double type.", name));
            return i;
        }
        protected static Regex rx_htmlstrip = new Regex("<(.|\n)*?>", RegexOptions.Compiled);
        protected string sValue(ParserResult res, string name, string v, bool req)
        {
            v = rx_htmlstrip.Replace(v, String.Empty);
            v = v.Trim();
            if (req && v.Length == 0)
                res.SetError(String.Format("'{0}' must have a value.", name));
            return v;
        }
    }
    [Serializable]
    public class ImportParser : ExportParser
    {
        public ImportSettings Settings { get; set; }
        public bool Valid { get; set; }
        public string Error = String.Empty;
        public Meta MetaData = new Meta();
        public StringBuilder sb = new StringBuilder();
        
        public ImportParser() { }
        public ImportParser(string xmlContent, ImportSettings settings)
        {
            Setup(XmlTextReader.Create(new StringReader(xmlContent), new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }), settings);
        }
        public ImportParser(Stream s, ImportSettings settings)
        {
            Setup(XmlTextReader.Create(s, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }), settings);
        }
        private void Setup(XmlReader r, ImportSettings settings)
        {
            this.Settings = settings ?? new ImportSettings();
            try
            {
                Parse(r);
                Result.ForEach(item => item.Estate.AgentId = MetaData.AgentId);
                ValidateInput();
                Valid = true;
                return;
            }
            catch (Exception ex) { Error = ex.ToString(); }
            finally { r.Close(); }
            Valid = false;
        }
        
        private static HashSet<string> datums = new HashSet<string> { "wgs84","rt90","sweref99tm" };
        private void ValidateInput()
        {
            HashSet<string> ids = new HashSet<string>();
            foreach (var r in Result)
            {
                if (ids.Contains(r.ClientId))
                    r.SetError("ClientId is not Unique! Every item _must_ have an unique Id.");
                ids.Add(r.ClientId);

                if (r.Estate.Id != 0)
                    r.SetError("The field 'Id' must not be set when importing. It's the Server Id that we assign.");

                if (r.Estate.BuildYear > 2200)
                    r.SetError("BuildYear is too big");
                if( r.Estate.CountryId == "SE" && r.Estate.MunicipalityId > 0 && Municipality.ById(r.Estate.MunicipalityId) == null )
                    r.SetError("No such Municipality exists");

                if (!EstateType.ValidType(r.Estate.ObjectType, r.Estate.ContractType))
                    r.SetError("The given EstateType and EstateContract combination is not valid.(" + r.Estate.ObjectType + "/" + r.Estate.ContractType + ")");
                if (r.Estate.Latitude != 0.0 || r.Estate.Longitude != 0.0)
                {
                    if (r.Estate.Datum != "wgs84" && r.Estate.Datum != "rt90" && r.Estate.Datum != "sweref99tm")
                        r.SetError("datum not supported, must be one of 'wgs84', 'rt90' or 'sweref99tm'.");
                }
                //Lengths
                if (Util.Length(r.Estate.ClientId) == 0)
                    r.SetError("The field 'ClientId' must contain your unique Id.");
                if (Util.Length(r.Estate.ClientId) > 30)
                    r.SetError("The field 'ClientId' must not be more than 30 characters long.");
                if (Util.Length(r.Estate.ProjectId) > 255)
                    r.SetError("The field 'ProjectId' must not be more than 255 characters long.");
                if (Util.Length(r.Estate.AreaName) > 30)
                    r.SetError("The field 'AreaName' must not be more than 30 characters long.");

                if (Settings.TryCorrectIssues && Util.Length(r.Estate.Address) > 30)
                    r.Estate.Address = r.Estate.Address.Substring(0, 30);

                if (Util.Length(r.Estate.Address) > 30)
                    r.SetError("The field 'Address' must not be more than 30 characters long.");
                if (Util.Length(r.Estate.City) > 20)
                    r.SetError("The field 'City' must not be more than 20 characters long.");
                if (Util.Length(r.Estate.CountryId) != 2)
                    r.SetError("The field 'CountryId' must be an ISO 3166-1 code.");
                if (Util.Length(r.Estate.Currency) != 3)
                    r.SetError("The field 'Currency' must be an ISO 4217 Alphabetic Currency Code.");

                if (Util.Length(r.Estate.ContactName) > 30)
                    r.SetError("The field 'ContactName' must not be more than 30 characters long.");
                if (Util.Length(r.Estate.ContactEmail) > 60)
                    r.SetError("The field 'ContactEmail' must not be more than 60 characters long.");
                if (Util.Length(r.Estate.ContactPhone) > 30)
                    r.SetError("The field 'ContactPhone' must not be more than 30 characters long.");
                if (Util.Length(r.Estate.DescriptionUrl) > 255)
                    r.SetError("The field 'DescriptionUrl' must not be more than 255 characters long.");
                if (Util.Length(r.Estate.DescriptionUrl) > 0)
                {
                    Uri res = null;
                    if (!(Uri.TryCreate(r.Estate.DescriptionUrl, UriKind.Absolute, out res) && (res.Scheme == Uri.UriSchemeHttp || res.Scheme == Uri.UriSchemeHttps)))
                        r.SetError("The field 'DescriptionUrl' is not a valid URL");
                }
                if (!string.IsNullOrEmpty(r.Estate.ContactEmail) && !Mail.IsValidEmail(r.Estate.ContactEmail))
                    r.SetError("The field 'ContactEmail' doesn't contain a valid email adress");

                //Negatives
                if(r.Estate.FloorsInBuilding < 0)
                    r.SetError("The field 'FloorsInBuilding' must not be negative.");
                if (r.Estate.Rent < 0)
                    r.SetError("The field 'Rent' must not be negative.");
                if (r.Estate.Price < 0)
                    r.SetError("The field 'Price' must not be negative.");
                if (r.Estate.BuildYear < 0)
                    r.SetError("The field 'BuildYear' must not be negative.");
            }
        }

        protected override void Parse(XmlReader r)
        {
            var ns = ParserNS.Root;
            string elm = "";
            while (r.Read())
            {
                base.Parse(r);
                if (r.NodeType == XmlNodeType.Element)
                {
                    elm = r.Name;
                    if (elm == "Meta")
                        ns = ParserNS.Meta;
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    if (r.Name == "Meta")
                        ns = ParserNS.Root;
                }
                else if (r.NodeType == XmlNodeType.Text || r.NodeType == XmlNodeType.CDATA)
                {
                    if (ns == ParserNS.Meta)
                    {
                        if (elm == "Agent" || elm == "AgentId")
                            MetaData.AgentId = iValue(res, elm, r.Value, true);
                        else if (elm == "Password")
                            MetaData.Password = sValue(res, elm, r.Value, false);
                        else if (elm == "AgentDuplicateCheck")
                        {
                            var adc = iValue(res, elm, r.Value, false);
                            if (adc > 0)
                                Settings.AgentDuplicateCheck.Add(adc);
                        }
                        else if (elm == "ImportAllImages")
                            Settings.ImportAllImages = Util.Bool(r.Value);
                        else if (elm == "TryCorrectIssues")
                            Settings.TryCorrectIssues = Util.Bool(r.Value);
                    }
                }
            }
        }
    }
}
