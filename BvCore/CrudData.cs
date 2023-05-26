using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public class CrudData<T> : BaseDataReadWriteId<T> where T : BaseDataReadWriteId<T>, ILocalID, new()
    {
        public new static List<T> Find(Expr[] elist, OrderBy[] order)
        {
            var list = new List<Expr>(elist);
            list.Add(Expr.LtEq("Deleted", Date.Treshold));
            return BaseDataReadWriteId<T>.Find(list.ToArray(),order);
        }
        public new void Save()
        {
            Changed = Date.Now;
            base.Save();
        }
        public new T Update(params DataDynamic.Eq[] elist)
        {
            Changed = Date.Now;
            return base.Update(elist);
        }
        public new void Delete()
        {
            Deleted = Date.Now;
            base.Save();
        }
        public DateTime Created = Date.Now;
        public DateTime Changed = Date.Now;
        public DateTime Deleted = Date.Default;
    }
}
