using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic]
    public class VAT : BaseDataReadWriteId<VAT>, ILocalID, IIdentityName
    {
        [DataDynamic(PrimaryKey=true)]
        public int Id { get; set; }
        [DataDynamic(Size=20)]
        public string Name { get; set; }
        [DataDynamic]
        public double Amount;

        public override string ToString()
        {
            return String.Format("{0} {1:0.00}", Name, Amount * 100.0);
        }

        static List<VAT> list;
        static VAT()
        {
            list = VAT.Find();
        }
        public static IEnumerable<VAT> AllVAT { get { return list; } }
        public static VAT NoVAT { get { return list[0]; } }
        public static VAT Standard { get { return list[1]; } }
        public static VAT ById(int Id)
        {
            return list.Find(v => v.Id == Id);
        }
        public static VAT ByAmount(double pct)
        {
            return list.Find(v => v.Amount == pct);
        }
        public new void Save() {
            base.Save();
            list = VAT.Find();
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            VAT v = obj as VAT;
            if ((object)v == null)
                return false;
            return Amount == v.Amount;
        }
        public override int GetHashCode()
        {
            return Amount.GetHashCode();
        }
        public bool Equals(VAT v)
        {
            if ((object)v == null)
                return false;
            return Amount == v.Amount;
        }
        public static bool operator ==(VAT a, VAT b)
        {
            // If both are null, or both are same instance, return true.
            if (object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            // Return true if the fields match:
            return a.Amount == b.Amount;
        }

        public static bool operator !=(VAT a, VAT b)
        {
            return !(a == b);
        }
    }
}
