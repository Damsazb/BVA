using Microsoft.AspNetCore.Mvc;


namespace BVA.Data
    {
    public class UserRole
        {
        public UserRole(string id, string userName, List<string> roles)
            {
            UserId = id;
            UserName = userName;
            Roles = roles;
            }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        }
    }
