using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public interface IPrice
    {
        double Amount { get; }
        Currency Currency { get; }
    }
    public class Price : IPrice
    {
        public static Price None = new Price(0.0, Currency.SEK);

        public double Amount { get; set; }
        public Currency Currency { get; set; }
        public Price(double amount, Currency currency)
        {
            this.Amount = amount;
            this.Currency = currency;
        }
        public Price(double amount, string currency)
        {
            this.Amount = amount;
            this.Currency = Currency.From(currency);
        }

        public static Price operator +(Price x, Price y) 
        {
            if (x.Currency != y.Currency)
                throw new Exception("You can not add two prices of different currency");
            return new Price(x.Amount + y.Amount, x.Currency);
        }
        public static Price operator -(Price x, Price y)
        {
            if (x.Currency != y.Currency)
                throw new Exception("You can not subtract two prices of different currency");
            return new Price(x.Amount - y.Amount, x.Currency);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Price p = obj as Price;
            if ((object)p == null)
                return false;
            return (Amount == p.Amount) && (Currency == p.Currency);
        }
        public override int GetHashCode()
        {
            return Amount.GetHashCode() ^ Currency.GetHashCode();
        }
        public bool Equals(Price p)
        {
            if ((object)p == null)
                return false;
            return (Amount == p.Amount) && (Currency == p.Currency);
        }
        public static bool operator ==(Price a, Price b)
        {
            // If both are null, or both are same instance, return true.
            if (object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            // Return true if the fields match:
            return (a.Amount == b.Amount) && (a.Currency == b.Currency);
        }

        public static bool operator !=(Price a, Price b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append(this.Currency.Code);
            return sb.ToString();
        }
    }
}
