using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision.Import
{
    public class Meta : Credentials
    {
        public string Query = "";
        public string Version = "";
    }
    public class Credentials
    {
        public enum AuthenticationState { Failed, Authenticated, NotValid, Error, Empty }
        private AuthenticationState state = AuthenticationState.Empty;
        [System.Xml.Serialization.XmlIgnore]
        public AuthenticationState State { get { return state; } private set { state = value; } }
        public static Credentials SuperUser = new Credentials(-1, "") { State = AuthenticationState.Authenticated };
        private int agentid = 0;
        public int AgentId { get { return agentid; } set { agentid = value; State = AuthenticationState.NotValid; } }
        private string password = "";
        public string Password { get { return password; } set { password = value; State = AuthenticationState.NotValid; } }
        public bool IsEmpty { get { return AgentId == 0 && String.IsNullOrEmpty(Password); } }
        public override bool Equals(object obj)
        {
            Credentials p = obj as Credentials;
            if ((object)p == null || p.Password == null)
                return false;
            return p.AgentId == AgentId && p.Password == Password;
        }
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Password))
                return 0;
            return Password.GetHashCode() ^ AgentId;
        }
        public static bool operator ==(Credentials a, Credentials b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.Equals(b);
        }
        public static bool operator !=(Credentials a, Credentials b)
        {
            return !(a == b);
        }
        public Credentials() { State = AuthenticationState.Empty;  }
        public Credentials(int AgentId, string Password)
        {
            this.AgentId = AgentId;
            this.Password = Password;
            State = AuthenticationState.NotValid;
        }
        public AuthenticationState Authenticate()
        {
            if (State == AuthenticationState.NotValid || State == AuthenticationState.Error)
            {
                if (IsEmpty)
                    State = AuthenticationState.NotValid;
                else
                {
                    try
                    {
                        var a = Customer.ById(AgentId);
                        if (a != null && a.Password == Password)
                            State = AuthenticationState.Authenticated;
                        else
                            State = AuthenticationState.Failed;
                    }
                    catch { State = AuthenticationState.Error; }
                }
            }
            return State;
        }
        public static Credentials Authenticate(int AgentId)
        {
            var c = Customer.ById(AgentId);
            return new Credentials(AgentId, c.Password) { state = AuthenticationState.Authenticated };
        }
        public static Credentials.AuthenticationState Authenticate(IEnumerable<Credentials> creds, out Credentials c)
        {
            c = null;
            Credentials.AuthenticationState state = AuthenticationState.NotValid;
            var tmp = creds.Where(cr => cr != null && !cr.IsEmpty);
            var first = tmp.FirstOrDefault();
            if (first != null && tmp.All(cr => cr.Authenticate() == AuthenticationState.Authenticated && cr.AgentId == first.AgentId))
            {
                state = AuthenticationState.Authenticated;
                c = first;
            }
            return state;
        }
        public static Credentials FromToken(SafeToken tk)
        {
            if(tk != null && tk.State == SafeToken.TokenState.Valid && tk.System == "Customer")
            {
                return new Credentials(tk.Id, "") { state = AuthenticationState.Authenticated };
            }
            return new Credentials();
        }
        public static Credentials FromAuthorizationString(string s)
        {
            var c = new Credentials();
            if(!string.IsNullOrEmpty(s))
            {
                var val = Util.DecodeFrom64(s);
                var parts = val.Split(':');
                if(parts.Length == 2)
                {
                    var id = Util.atoi(parts[0]);
                    if(id > 0)
                    {
                        c.AgentId = id;
                        c.Password = parts[1];
                        c.State = AuthenticationState.NotValid;
                    }
                }
            }
            return c;
        }
    }
}
