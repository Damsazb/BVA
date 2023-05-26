using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    //ISO 4217
    public class Currency
    {
        public static Currency SEK = new Currency("SEK", "Svensk Krona");
        public static Currency EUR = new Currency("EUR", "Euro");
        public static Currency USD = new Currency("USD", "US-Dollar");
        public static Currency GBP = new Currency("GBP", "Brittiskt pund");
        public static Currency DKK = new Currency("DKK", "Dansk krona");
        public static Currency NOK = new Currency("NOK", "Norsk krona");
        public static Currency NONE = new Currency("", "");

        public static Currency XPF = new Currency("XPF", "CFP-franc");
        public static Currency CAD = new Currency("CAD", "Kanadensisk dollar");
        public static Currency THB = new Currency("THB", "Thai baht");
        public static Currency CHF = new Currency("CHF", "Schweizisk franc");
        public static Currency CZK = new Currency("CZK", "Tjeckisk krona");
        public static Currency AED = new Currency("AED", "Emiratisk dirham");
        public static Currency AUD = new Currency("AUD", "Australisk dollar");
        public static Currency NZD = new Currency("NZD", "Nyzeeländsk dollar");
        public static Currency ZAR = new Currency("ZAR", "Rand");

        public static IEnumerable<Currency> Items;
        private static Dictionary<string, Currency> dict;

        public string Code { get; private set; }
        public string Name { get; private set; }

        protected Currency() { }
        protected Currency(string code, string name)
        {
            if (code == null || name == null)
                throw new ArgumentException();
            if (!code.IsUpper())
                code = code.ToUpper();
            Code = code;
            Name = name;
        }
        static Currency()
        {
            Items = new[] { SEK, EUR, USD, GBP, DKK, NOK };
            dict = Items.ToDictionary(k => k.Code, StringComparer.OrdinalIgnoreCase);
        }
        public static Currency From(string code)
        {
            code = code != null ? code : "";
            Currency c = null;
            if (dict.TryGetValue(code, out c))
                return c;
            return Currency.NONE;
        }
        public static bool operator ==(Currency a, Currency b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.Code == b.Code;
        }
        public static bool operator !=(Currency a, Currency b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Code.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var c = obj as Currency;
                if (c != null)
                    return Code.Equals(c.Code);
            }
            return false;
        }
        public override string ToString()
        {
            return Code;
        }
    }
}
