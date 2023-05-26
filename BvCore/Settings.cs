using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic("Settings")]
    public class Setting : BaseDataReadWriteId<Setting>, ILocalID, IIdentityName
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public string Name { get; set; }
        [DataDynamic]
        public string Value;
        //[DataDynamic]
        public bool CanEdit = false;
    }
}
