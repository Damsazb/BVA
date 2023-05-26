using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic]
    public class Tag : BaseDataReadWriteId<Tag>, ILocalID, IIdentityName
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public string Name { get; set; }

        private static List<Tag> tags;
        private static Dictionary<int,Tag> tdict;
        public static IEnumerable<Tag> Tags
        {
            get
            {
                if (tags == null)
                {
                    UpdateCache();
                    if (CrudHappend != null)
                        CrudHappend(tags, CrudEvent.Refresh);
                }
                return tags;
            }
        }
        public static event EventHandler<CrudEvent> CrudHappend;

        private static void UpdateCache()
        {
            tags = Tag.Find(new OrderBy("Name", Direction.Ascend));
            tdict = tags.ToDictionary(t => t.Id);
        }
        public static Tag ByName(string Name)
        {
            return Tags.FirstOrDefault(t => String.Compare(t.Name, Name, true) == 0);
        }
        public static Tag ById(int Id)
        {
            if( tags == null )
                UpdateCache();
            Tag tag;
            tdict.TryGetValue(Id, out tag);
            return tag;
        }
        public new void Save()
        {
            base.Save();
            UpdateCache();
            if (CrudHappend != null)
                CrudHappend(this, Id == 0 ? CrudEvent.Create : CrudEvent.Update);
        }
        public new void Delete()
        {
            base.Delete();
            UpdateCache();
            if (CrudHappend != null)
                CrudHappend(this, CrudEvent.Delete);
        }
    }
    [DataDynamic]
    public class CustomerTagSet : BaseDataReadWriteId<CustomerTagSet>, ILocalID
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public int TagId { get; set; }
        [DataDynamic]
        public int CustomerId { get; set; }

        public Tag Tag { get { return Bovision.Tag.ById(TagId); } }
        public string TagName { get { return Bovision.Tag.ById(TagId).Name; } }

        public static IEnumerable<Customer> GetCustomers(Tag tag)
        {
            var list = CustomerTagSet.Find( Expr.Eq( "TagId", tag.Id ) );
            return  Customer.Find( Expr.In("Id", list.Select( t => t.CustomerId)));
        }
        public static IEnumerable<Tag> GetTags(Customer c)
        {
            var tags = CustomerTagSet.Find(Expr.Eq("CustomerId", c.Id));
            if( tags.Count > 0 )
                return Bovision.Tag.Find(Expr.In("Id", tags.Select(t => t.TagId).ToArray()));
            return new List<Tag>();
        }
    }
}
