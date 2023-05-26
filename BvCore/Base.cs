using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision.Controls
{
    public interface ICrudControl
    {
        event EventHandler<CrudEvent> CrudHappend;
        void UpdateControl(object model);
        object UpdateModel();
    }
}
