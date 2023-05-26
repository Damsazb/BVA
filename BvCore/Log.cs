using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic]
    public class Log : BaseDataReadWriteId<Log>, ILocalID
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic(Size=40)]
        public string Who { get; set; }
        [DataDynamic(Size=1024)]
        public string Text { get; set; }
        [DataDynamic]
        public DateTime Created = DateTime.Now;

        public static void Message(string what, params object []p)
        {
            Message(Global.CurrentUser.Nick, String.Format(what,p));
        }
        public static void Message(string from, string what)
        {
            var l = new Log();
            l.Who = from;
            l.Text = what;
            l.Save();
        }
    }
}
