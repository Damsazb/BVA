using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision.Client
{
    public class Query : AbstractParams<Query>
    {
        public Query(string parameters)
        {
            Parse(parameters, null);
        }
        public Query(Params parameters)
        {
        }
        [QKey("ai")]
        public List<int> AgentIds = new List<int>();
        [QKey("gi")]
        public List<int> GroupIds = new List<int>();
        [QKey("ea")]
        public List<EstateAreaId> EstateAreas = new List<EstateAreaId>();
        [QKey("t")]
        public List<SearchType> EstateTypes = new List<SearchType>();

        [QKey("price")]
        public Range<int> Price = new Range<int>();
        [QKey("area")]
        public Range<double> LivingArea = new Range<double>();
        [QKey("rooms")]
        public Range<double> Rooms = new Range<double>();
        [QKey("rent")]
        public Range<int> Rent = new Range<int>();
        [QKey("lot")]
        public Range<double> Lot = new Range<double>();
        [QKey("year")]
        public Range<int> BuildYear = new Range<int>();
        [QKey("floors")]
        public Range<int> FloorsInBuilding = new Range<int>();
        [QKey("floor")]
        public Range<int> Floor = new Range<int>();
        [QKey("elevator")]
        public int Elevator = 0;
        [QKey("active")]
        public int Active = 1;
        [QKey("so")]
        public List<SortProperty> Sorting = new List<SortProperty>();

        public void AddSort(string name, SortDirection dir = SortDirection.Ascend, DefaultValue defv = DefaultValue.Smallest)
        {
            Sorting.Add(new SortProperty(name, dir, defv));
        }
        [QKey("p")]
        public int Page = 0;
        [QKey("ipp")]
        public int ItemsPerPage = 0;

        protected override bool Parse(Type t, Member m, string v)
        {
            if (t == typeof(List<EstateAreaId>))
            {
                List<EstateAreaId> list = (List<EstateAreaId>)m.Get(this);
                foreach (string s in v.Split(comma, StringSplitOptions.RemoveEmptyEntries))
                {
                    var id = EstateAreaId.From(s);
                    if (id != null)
                        list.Add(id);
                }
                return true;
            }
            else if (t == typeof(List<SearchType>))
            {
                var list = (List<SearchType>)m.Get(this);
                foreach (string s in v.Split(comma, StringSplitOptions.RemoveEmptyEntries))
                {
                    SearchType type;
                    if (Bovision.SearchTypes.TryParse(s, out type))
                        list.Add(type);
                }
                return true;
            }
            else if (t == typeof(List<SortProperty>))
            {
                var list = (List<SortProperty>)m.Get(this);
                list.AddRange(SortProperty.Parse(v));
                return true;
            }
            return false;
        }
    }
}
