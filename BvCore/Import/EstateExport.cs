using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Globalization;
using Bovision.Import;
using Newtonsoft.Json;

namespace Bovision.Export
{
    public class XmlExportParams : AbstractParams<XmlExportParams>
    {
        public enum ExportAgentInfo { Inline, Unique, None }
        public enum ExportStatus { Active, All, Removed }
        [DataMember]
        [QKey("includeAgents")]
        public ExportAgentInfo IncludeAgents = ExportAgentInfo.None;
        [DataMember]
        [QKey("nImages")]
        public int NumberOfImages = 3;
        [QKey("key")]
        public string Key = "";
        [DataMember]
        [QKey("includeFrameUrl")]
        public bool IncludeExternalUrl = false;
        [QKey("xtnd")]
        public bool ExtendedData;
        [QKey("status")]
        public  ExportStatus Status = ExportStatus.Active;
        public XmlExportParams() { }
        public XmlExportParams(string qs)
        {
            Parse(qs, null);
        }
        public XmlExportParams(Params p)
        {
            Parse(p);
        }
        public string ImageTemplate = "http://files.bovision.se/Bilder/{0}/{2}.{1}.scale";
        
    }

    public class JsonExporter
    {
        private static CultureInfo icnfo = CultureInfo.InvariantCulture;
        private JsonTextWriter writer;
        //public string ImageTemplate = "{0}";
        // public static ConvertOptions standardOptions = new ConvertOptions { NumberOfImages = 3 };
        public XmlExportParams ExportParams = null;
        public JsonExporter(XmlExportParams parms, StringBuilder sb)
        {
            ExportParams = parms;
            StringWriter sw = new StringWriter(sb);
            writer = new JsonTextWriter(sw);

        }
        public JsonExporter(XmlExportParams parms, Stream s)
        {
            ExportParams = parms;
            StreamWriter sw = new StreamWriter(s, Encoding.UTF8);
            writer = new JsonTextWriter(sw);
        }
        public void Export(IEnumerable<XtndEstate> list)
        {
            Export(list, null);
        }
        private void Export(IEnumerable<XtndEstate> list, Pager<ImportEstate> pager)
        {
            writer.Formatting = Newtonsoft.Json.Formatting.Indented;

            writer.WriteStartObject();
            if (pager != null)
            {
                writer.WritePropertyName("Pager");
                writer.WriteStartObject();
                writer.WritePropertyName("TotalCount"); writer.WriteValue(pager.TotalCount);
                writer.WritePropertyName("ItemsPerPage");writer.WriteValue(pager.ItemsPerPage);
                writer.WritePropertyName("Page"); writer.WriteValue(pager.Pages);
                writer.WritePropertyName("CurrentPage"); writer.WriteValue(pager.CurrentPage);
                writer.WriteEndObject();
            }

            writer.WritePropertyName("Result");
            writer.WriteStartArray();
            foreach (var est in list)
                ExportEstate(est);
            writer.WriteEndArray();
            writer.Flush();
        }

        private void ExportAgent(ExportAgent agent)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Id"); writer.WriteValue(agent.Id);
            writer.WritePropertyName("Name"); writer.WriteValue(agent.Name);
            writer.WritePropertyName("Address"); writer.WriteValue(agent.Address);
            writer.WritePropertyName("ZipCode"); writer.WriteValue(agent.ZipCode);
            writer.WritePropertyName("City"); writer.WriteValue(agent.City);
            writer.WritePropertyName("HomePage"); writer.WriteValue(agent.HomePage);
            writer.WritePropertyName("Logo"); writer.WriteValue(agent.Logo);
            writer.WriteEndObject();
        }

        private bool isnext = false;

        private void ExportEstate(XtndEstate ie)
        {
            //if (isnext)
            //    writer.WriteLine(",");
            //isnext = true;

            if (ExportParams.IncludeAgents == XmlExportParams.ExportAgentInfo.Inline)
            {
                if (ie.Agent != null)
                    ExportAgent(ie.Agent);
            }

            writer.WriteStartObject();
            writer.WritePropertyName("Id"); writer.WriteValue(ie.Id);
            writer.WritePropertyName("ClientId"); writer.WriteValue(ie.ClientId);
            writer.WritePropertyName("ProjectId"); writer.WriteValue(ie.ProjectId);
            writer.WritePropertyName("AgentId"); writer.WriteValue(ie.AgentId);
            writer.WritePropertyName("EstateType"); writer.WriteValue(ie.ObjectType);
            writer.WritePropertyName("EstateContract"); writer.WriteValue(ie.ContractType);
            writer.WritePropertyName("SwapDemand"); writer.WriteValue(ie.SwapDemand);

            writer.WritePropertyName("AreaName"); writer.WriteValue(ie.AreaName);
            writer.WritePropertyName("Address"); writer.WriteValue(ie.Address);
            writer.WritePropertyName("ZipCode"); writer.WriteValue(ie.ZipCode);
            writer.WritePropertyName("City"); writer.WriteValue(ie.City);

            writer.WritePropertyName("MunicipalityId"); writer.WriteValue(ie.MunicipalityId);
            Municipality m = Municipality.ById(ie.MunicipalityId);
            writer.WritePropertyName("Municipality"); writer.WriteValue(m.Name);

            writer.WritePropertyName("CountryId"); writer.WriteValue(ie.CountryId);
            Country cy = Country.ByISO2Code(ie.CountryId);
            writer.WritePropertyName("Country"); writer.WriteValue(cy.Name);

            writer.WritePropertyName("Location");
            writer.WriteStartObject();
            writer.WritePropertyName("lat"); writer.WriteValue(String.Format(icnfo, "{0:0.####}", ie.Latitude));
            writer.WritePropertyName("lng"); writer.WriteValue(String.Format(icnfo, "{1:0.####}", ie.Longitude));
            writer.WriteEndObject();

            writer.WritePropertyName("UsableArea"); writer.WriteValue(ie.UsableArea.ToString(icnfo));
            writer.WritePropertyName("SideArea"); writer.WriteValue(ie.SideArea.ToString(icnfo));
            writer.WritePropertyName("LotArea"); writer.WriteValue(ie.LotArea.ToString(icnfo));
            writer.WritePropertyName("BuildYear"); writer.WriteValue(ie.BuildYear.ToString(icnfo));
            writer.WritePropertyName("Rooms"); writer.WriteValue(ie.Rooms.ToString(icnfo));
            writer.WritePropertyName("Currency"); writer.WriteValue(ie.Currency);
            writer.WritePropertyName("Price"); writer.WriteValue(ie.Price);
            writer.WritePropertyName("Rent"); writer.WriteValue(ie.Rent);
            writer.WritePropertyName("FloorsInBuilding"); writer.WriteValue(ie.FloorsInBuilding);
            writer.WritePropertyName("Floor"); writer.WriteValue(ie.Floor);
            writer.WritePropertyName("HasElevator"); writer.WriteValue(ie.HasElevator);

            writer.WritePropertyName("Description"); writer.WriteValue(ie.Description);

            if(ExportParams.ExtendedData)
            {
                writer.WritePropertyName("Design"); writer.WriteValue(ie.Design);
                writer.WritePropertyName("Surroundings"); writer.WriteValue(ie.Surroundings);
                writer.WritePropertyName("Equipment"); writer.WriteValue(ie.Equipment);
                writer.WritePropertyName("OutdoorPlace"); writer.WriteValue(ie.OutdoorPlace);
                writer.WritePropertyName("OtherInfo"); writer.WriteValue(ie.OtherInfo);
                writer.WritePropertyName("Parking"); writer.WriteValue(ie.Parking);
                writer.WritePropertyName("OtherBuildings"); writer.WriteValue(ie.OtherBuildings);
            }


            if (ie.DisplayTime > DateTime.Now.Date)
                writer.WritePropertyName("Display"); writer.WriteValue(ie.DisplayTime.ToString("s"));

            writer.WritePropertyName("ContactName"); writer.WriteValue(ie.ContactName);
            writer.WritePropertyName("ContactEmail"); writer.WriteValue(ie.ContactEmail);
            writer.WritePropertyName("ContactPhone"); writer.WriteValue(ie.ContactPhone);

            writer.WritePropertyName("DescriptionUrl");
            var _url = ie.DescriptionUrl.StartsWith("http://") 
                ? ie.DescriptionUrl 
                : "http://bovision.se" + ie.DescriptionUrl;
            writer.WriteValue(_url);

            if (ie.Images == null || ie.Images.Count == 0)
            {
                writer.WritePropertyName("Images");
                writer.WriteStartArray(); writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName("Images");
                writer.WriteStartArray();

                ie.Images
                    .ForEach(
                        img => 
                        writer.WriteValue(String.Format(ExportParams.ImageTemplate, ie.AgentId, img, ie.Id)));
                writer.WriteEndArray();
            }

            writer.WritePropertyName("Created"); writer.WriteValue(ie.Created.ToString("s"));
            writer.WritePropertyName("Changed"); writer.WriteValue(ie.Changed.ToString("s"));

            if (ExportParams.Status != XmlExportParams.ExportStatus.Active)
            {
                writer.WritePropertyName("Status"); writer.WriteValue(ie.Status);
            }

            writer.WriteEndObject();
        }
    }
    public class XmlExporter
    {
        private XmlTextWriter writer;
        //public string ImageTemplate = "{0}";
        // public static ConvertOptions standardOptions = new ConvertOptions { NumberOfImages = 3 };
        public XmlExportParams ExportParams = null;
        public XmlExporter(XmlExportParams parms, StringBuilder sb)
        {
            ExportParams = parms;
            StringWriter sw = new StringWriter(sb);
            writer = new XmlTextWriter(sw);
        }
        public XmlExporter(XmlExportParams parms, Stream s)
        {
            ExportParams = parms;
            writer = new XmlTextWriter(s, System.Text.Encoding.UTF8);
        }
        public void Export(IEnumerable<ImportEstate> list)
        {
            Export(list.ToList(), null);
        }
        public void Export(IEnumerable<ParserResult> result)
        {
            Export(result.Select(r => r.Estate as ImportEstate).ToList(), null);
        }
        public void Export(Search s)
        {
            Export(s.Result, s.Pager);
        }
        private void Export(List<ImportEstate> list, Pager<ImportEstate> pager)
        {
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("Export");
            writer.WriteAttributeString("version", "1.0");
            writer.WriteStartElement("Meta");
            if (pager != null)
            {
                writer.WriteStartElement("Pager");
                writer.WriteElementString("TotalCount", pager.TotalCount.ToString());
                writer.WriteElementString("ItemsPerPage", pager.ItemsPerPage.ToString());
                writer.WriteElementString("Pages", pager.Pages.ToString());
                writer.WriteElementString("CurrentPage", pager.CurrentPage.ToString());
                writer.WriteEndElement();
            }
            else
                writer.WriteElementString("TotalCount", list.Count.ToString());

            writer.WriteEndElement();

            if (ExportParams.IncludeAgents == XmlExportParams.ExportAgentInfo.Unique)
            {
                writer.WriteStartElement("Agents");
                var aids = list.Select(est => est.AgentId).ToHashSet();
                foreach (var aid in aids)
                    ExportAgent(writer, Data.AgentById(aid));
                writer.WriteEndElement();
            }

            writer.WriteStartElement("Result");
            foreach (var est in list)
                ExportEstate(est);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }
        private static CultureInfo icnfo = CultureInfo.InvariantCulture;
        private void ExportAgent(XmlTextWriter writer, ExportAgent agent)
        {
            writer.WriteStartElement("Agent");
            writer.WriteElementString("Id", agent.Id.ToString());
            writer.WriteElementString("Name", agent.Name);
            writer.WriteElementString("Address", agent.Address);
            writer.WriteElementString("ZipCode", agent.ZipCode.ToString());
            writer.WriteElementString("City", agent.City);
            writer.WriteElementString("HomePage", agent.HomePage);
            writer.WriteElementString("Logo", agent.Logo);
            writer.WriteElementString("Email", agent.Email);
            writer.WriteElementString("Phone", agent.Phone);
            writer.WriteEndElement();
        }
        private void ExportEstate(ImportEstate ie)
        {
            writer.WriteStartElement("Estate");

            if (ExportParams.IncludeAgents == XmlExportParams.ExportAgentInfo.Inline)
            {
                if (ie.Agent != null)
                    ExportAgent(writer, ie.Agent);
            }
            writer.WriteElementString("Id", ie.Id.ToString());
            writer.WriteElementString("ClientId", ie.ClientId);
            if (!string.IsNullOrEmpty(ie.ProjectId))
                writer.WriteElementString("ProjectId", ie.ProjectId);
            writer.WriteElementString("AgentId", ie.AgentId.ToString());
            writer.WriteElementString("EstateType", ie.ObjectType);
            writer.WriteElementString("EstateContract", ie.ContractType);
            writer.WriteElementString("SwapDemand", ie.SwapDemand ? "true" : "false");

            writer.WriteElementString("AreaName", ie.AreaName);
            writer.WriteElementString("Address", ie.Address);
            writer.WriteElementString("ZipCode", ie.ZipCode.ToString());
            writer.WriteElementString("City", ie.City);
            writer.WriteStartElement("MunicipalityId");
            if (Municipality.ById(ie.MunicipalityId) != null)
                writer.WriteAttributeString("name", Municipality.ById(ie.MunicipalityId).Name);
            writer.WriteString(ie.MunicipalityId.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("CountryId");
            if (Country.ByISO2Code(ie.CountryId) != null)
                writer.WriteAttributeString("name", Country.ByISO2Code(ie.CountryId).Name);
            writer.WriteString(ie.CountryId);
            writer.WriteEndElement();

            writer.WriteStartElement("Location");
            writer.WriteAttributeString("datum", ie.Datum);
            writer.WriteElementString("Latitude", ie.Latitude.ToString(icnfo));
            writer.WriteElementString("Longitude", ie.Longitude.ToString(icnfo));
            writer.WriteEndElement();

            writer.WriteElementString("UsableArea", ie.UsableArea.ToString(icnfo));
            writer.WriteElementString("SideArea", ie.SideArea.ToString(icnfo));
            writer.WriteElementString("LotArea", ie.LotArea.ToString(icnfo));
            writer.WriteElementString("BuildYear", ie.BuildYear.ToString());
            writer.WriteElementString("Rooms", ie.Rooms.ToString(icnfo));
            writer.WriteElementString("Currency", ie.Currency);
            writer.WriteElementString("Price", ie.Price.ToString());
            writer.WriteElementString("Rent", ie.Rent.ToString());
            writer.WriteElementString("FloorsInBuilding", ie.FloorsInBuilding.ToString());
            writer.WriteElementString("Floor", ie.Floor.ToString());
            writer.WriteElementString("HasElevator", ie.HasElevator ? "true" : "false");


            writer.WriteStartElement("Description");
            writer.WriteCData(ie.Description);
            writer.WriteEndElement();

            var xe = ie as XtndEstate;
            if (xe != null && ExportParams.ExtendedData)
            {
                if(!string.IsNullOrEmpty(xe.Design))
                {
                    writer.WriteStartElement("Design");
                    writer.WriteCData(xe.Design);
                    writer.WriteEndElement();
                }
                if (!string.IsNullOrEmpty(xe.Surroundings))
                {
                    writer.WriteStartElement("Surroundings");
                    writer.WriteCData(xe.Surroundings);
                    writer.WriteEndElement();
                }
                if (!string.IsNullOrEmpty(xe.Equipment))
                {
                    writer.WriteStartElement("Equipment");
                    writer.WriteCData(xe.Equipment);
                    writer.WriteEndElement();
                }
                if (!string.IsNullOrEmpty(xe.OutdoorPlace))
                {
                    writer.WriteStartElement("OutdoorPlace");
                    writer.WriteCData(xe.OutdoorPlace);
                    writer.WriteEndElement();
                }
                if (!string.IsNullOrEmpty(xe.Parking))
                {
                    writer.WriteStartElement("Parking");
                    writer.WriteCData(xe.Parking);
                    writer.WriteEndElement();
                }
                if (!string.IsNullOrEmpty(xe.OtherBuildings))
                {
                    writer.WriteStartElement("OtherBuildings");
                    writer.WriteCData(xe.OtherBuildings);
                    writer.WriteEndElement();
                }
                if (!string.IsNullOrEmpty(xe.OtherInfo))
                {
                    writer.WriteStartElement("OtherInfo");
                    writer.WriteCData(xe.OtherInfo);
                    writer.WriteEndElement();
                }
            }

            if (ie.DisplayTime > DateTime.Now.Date)
                writer.WriteElementString("Display", ie.DisplayTime.ToString("s"));

            writer.WriteElementString("ContactName", ie.ContactName);
            writer.WriteElementString("ContactEmail", ie.ContactEmail);
            writer.WriteElementString("ContactPhone", ie.ContactPhone);

            if(ie.Id == 0)
                writer.WriteElementString("DescriptionUrl", ie.DescriptionUrl);
            else
                writer.WriteElementString("DescriptionUrl", "http://bovision.se/Beskrivning/" + ie.Id);
            if (ExportParams.IncludeExternalUrl)
            {
                var url = ie.DescriptionUrl.StartsWith("http") ? ie.DescriptionUrl : "http://bovision.se/Beskrivning/" + ie.Id;
                writer.WriteElementString("ExternalUrl", url);
            }
            writer.WriteStartElement("Images");

            ie.Images.ForEach(img => writer.WriteElementString("Image", String.Format(ExportParams.ImageTemplate, ie.AgentId, img, ie.Id)));
            writer.WriteEndElement();
            writer.WriteElementString("Created", ie.Created.ToString("s"));
            writer.WriteElementString("Changed", ie.Changed.ToString("s"));
            if(ExportParams.Status != XmlExportParams.ExportStatus.Active || ie.Status != EstateStatus.Public.Value)
            {
                writer.WriteElementString("Status", ie.Status);
            }
            writer.WriteEndElement();
        }
    }
}
