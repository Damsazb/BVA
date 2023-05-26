using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bovision.Import
{
    public enum ImportJobStatus { InProgress, Done }
    [Serializable]
    public class ImportJob
    {
        public Guid JobId = Guid.NewGuid();
        public DateTime Created = DateTime.Now;
        public ImportParser Parser { get; set; }
        public Credentials Credentials { get { Credentials c; Credentials.Authenticate(credentials, out c); return c; } set { credentials.Add(value); } }
        private List<Credentials> credentials = new List<Credentials>();
        public ImportJob() { }
        public ImportJob(ImportParser parser, List<Credentials> credentials)
        {
            this.Parser = parser;
            if(credentials != null)
                this.credentials.AddRange(credentials);
            this.credentials.Add(new Credentials(parser.MetaData.AgentId, parser.MetaData.Password));
        }
        public Credentials.AuthenticationState CheckCredentials()
        {
            Credentials c;
            var state = Credentials.Authenticate(credentials, out c);
            if (state == Import.Credentials.AuthenticationState.Authenticated)
                this.Credentials = c;
            return state;
        }
    }
    public enum State
    {
        NotValid = 0,
        Parsed = 1,
        Error = 2,
        Updated = 3,
        New = 4,
        Skipped = 5,
        Deleted = 6,
        Loaded = 7,
    }
    public class ImportItem
    {
        public string Id;
        public int AgentId;
    }
    public class ImportStatus
    {
        public DateTime CurrentTime = DateTime.Now;
        public string Status;
        public int PercentDone;
        public int TotalItems;
        public int ProcessedItems;
        public int UnProcessedItems;
        public ImportDetails ProcessDetails = new ImportDetails();
        public class ImportDetails
        {
            public int NotValidItems;
            public int ErrorItems;
            public int NewItems;
            public int SkippedItems;
            public int UpdatedItems;
            public int DeletedItems;
        }
    }
    public class ImportSummary
    {
        public string Id = null;
        public DateTime CurrentTime = DateTime.Now;
        public int TotalCount = 0;
        public int ValidCount = 0;
        public int NotValidCount = 0;
        public ImportSummary() { }
        public ImportSummary(ImportJob job)
        {
            Id = job.JobId.ToString();
            TotalCount = job.Parser.Result.Count;
            foreach (var res in job.Parser.Result)
            {
                if (res.Status == ParserResultStatus.Parsed)
                    ValidCount++;
                else if (res.Status == ParserResultStatus.NotValid)
                    NotValidCount++;
            }
            FailureDetails = job.Parser.Result.Where(r => r.Status == ParserResultStatus.NotValid).Select(res => new Item { ClientId = res.ClientId, Messages = String.Join("\r\n", res.Messages) }).ToList();
        }
        public List<Item> FailureDetails;
    }
    public class Item
    {
        public string ClientId = null;
        public string Messages;
    }
}