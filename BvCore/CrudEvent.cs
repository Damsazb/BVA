using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public enum CrudType { Create, Read, Update, Delete, Refresh };
    public class CrudEvent
    {
        public CrudType Type;
        public bool Handled = false;
        public static CrudEvent Create { get { return new CrudEvent(CrudType.Create); } }
        public static CrudEvent Read { get { return new CrudEvent(CrudType.Read); } }
        public static CrudEvent Update { get { return new CrudEvent(CrudType.Update); } }
        public static CrudEvent Delete { get { return new CrudEvent(CrudType.Delete); } }
        public static CrudEvent Refresh { get { return new CrudEvent(CrudType.Refresh); } }
        private CrudEvent(CrudType type)
        {
            this.Type = type;
        }
    }
}
