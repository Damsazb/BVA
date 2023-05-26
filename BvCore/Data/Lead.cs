using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic("Intresseanm")]
    public class LeadItem
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public int AgentId { get; set; }
        [DataDynamic("ObjectId")]
        public int EstateId { get; set; }
        [DataDynamic]
        public string FullName { get; set; }
        [DataDynamic("PID")]
        public string PersonalIdentity { get; set; }
        [DataDynamic]
        public string Phone { get; set; }
        [DataDynamic]
        public string Email { get; set; }
        [DataDynamic]
        public string Address { get; set; }
        [DataDynamic("ZipCode")]
        public string PostalCode { get; set; }
        [DataDynamic]
        public string City { get; set; }
        [DataDynamic]
        public string Errand { get; set; }
        [DataDynamic]
        public string Message { get; set; }
        [DataDynamic]
        public string Notes = "";
        [DataDynamic]
        public DateTime Created = new DateTime(1900, 1, 1);
        [DataDynamic]
        public DateTime LastAccess = new DateTime(1900, 1, 1);
        [DataDynamic]
        public DateTime Transfered = new DateTime(1900, 1, 1);
        [DataDynamic]
        public DateTime Sent = new DateTime(1900, 1, 1);
        [DataDynamic]
        public string AgentEmail { get; set; }
    }
    public class Lead
    {
        public static List<LeadItem> GetByAgent(int AgentId)
        {
            using( var ctx = new Data<LeadItem>())
            {
                return ctx.Find(lead => lead.AgentId == AgentId, new []{ new OrderBy("Created", Direction.Descend) }, 50);
            }
        }
        public static List<LeadItem> GetByEstate(int EstateId)
        {
            using (var ctx = new Data<LeadItem>())
            {
                return ctx.Find(lead => lead.EstateId == EstateId, new OrderBy("Created", Direction.Descend));
            }
        }
        public static bool Save(LeadItem item)
        {
            try
            {
                using (var ctx = new Data<LeadItem>())
                {
                    ctx.Save(item);
                    return true;
                }
            }
            catch { return false; }
        }
    }
}
