using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic(FieldName="SysUser")]
    public class User : BaseDataReadWriteId<User>, ILocalID, IIdentityName
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic(Size = 120)]
        public string Name { get; set; }
        [DataDynamic]
        public string Email { get; set; }
        [DataDynamic]
        public string Phone { get; set; }
        [DataDynamic(Size=40)]
        public string Nick { get; set; }
        [DataDynamic]
        public int SecurityLevel = 0;
        public bool IsAdmin { get { return SecurityLevel == 1; } }
        [DataDynamic]
        public string Password { get; set; }
        [DataDynamic]
        public DateTime Created = Date.Default;
        [DataDynamic]
        public DateTime Deleted = Date.Default;
        [DataDynamic]
        public DateTime LastAccess = Date.Default;


        private static System.Collections.Concurrent.ConcurrentDictionary<int, User> users = new System.Collections.Concurrent.ConcurrentDictionary<int, User>();
        public static User ById(int Id)
        {
            User user;
            if (!users.TryGetValue(Id, out user))
                user = users[Id] = User.Get(Id);
            return user;
        }
        public override string ToString()
        {
            return String.Format("[{0}]/{1}",Id,Name);
        }

        public static readonly User Unknown = new User() { Id = -1, Name = "Unknown", Email = "N/A", Nick = "N/A", SecurityLevel = -1 };
    }
}
