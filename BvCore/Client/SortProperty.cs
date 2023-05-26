using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision.Client
{
    public enum DefaultValue { Smallest, Largest }
    public enum SortDirection { Ascend = 1, Descend = -1 }
    public struct SortProperty
    {
        public string Name;
        public SortDirection Direction;
        public DefaultValue Default;

        private static char[] delims = new char[] { ' ', ':' };
        public static List<SortProperty> Parse(string s)
        {
            string[] parts = s.Split(',');
            var props = new List<SortProperty>();
            for (int i = 0; i < parts.Length; i++)
            {
                props.Add(new SortProperty(parts[i]));
            }
            return props;
        }
        public SortProperty(string Name)
        {
            string[] ps = Name.Split(delims);
            this.Name = ps[0];
            if (ps.Length > 1)
            {
                string dir = ps[1];
                Direction = (dir.StartsWith("de", StringComparison.InvariantCultureIgnoreCase) || dir == "-1") ? SortDirection.Descend : SortDirection.Ascend;
            }
            else
                Direction = SortDirection.Ascend;
            if (ps.Length > 2)
            {
                string v = ps[2];
                Default = (string.Compare(v, "dl", true) == 0 ? DefaultValue.Largest : DefaultValue.Smallest);
            }
            else
                Default = DefaultValue.Smallest;
        }
        public SortProperty(string Name, SortDirection Direction)
        {
            this.Name = Name;
            this.Direction = Direction;
            this.Default = DefaultValue.Smallest;
        }
        public SortProperty(string Name, SortDirection Direction, DefaultValue Default)
        {
            this.Name = Name;
            this.Direction = Direction;
            this.Default = Default;
        }
        public override string ToString()
        {
            return ToString(null).ToString();
        }
        public StringBuilder ToString(StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder();
            sb.Append(Name);
            if (Direction == SortDirection.Descend || Default == DefaultValue.Largest)
                sb.Append(":").Append(Direction == SortDirection.Ascend ? "1" : "-1");
            if (Default == DefaultValue.Largest)
                sb.Append(":dl");
            return sb;
        }
    }
}
