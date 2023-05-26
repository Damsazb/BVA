using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Bovision
{



    public class ExchangeRates
    {
        public Currency Base { get; private set; }
        protected Dictionary<Currency,ExchangeRate> Rates = new Dictionary<Currency, ExchangeRate>();
        public static string Test()
        {
            var exch = ExchangeRates.FromECB();
            var list = new List<Price>() {
                new Price(100.0, Currency.USD),
                new Price(100.0, Currency.NOK),
                new Price(100.0, Currency.DKK),
                new Price(100.0, Currency.GBP),
            };
            var conv = list.Select( p => exch.Convert(p, Currency.SEK)).ToList();
            var res = new List<Price>();
            for(int i = 0; i < list.Count; i++)
            {
                res.Add(exch.Convert(conv[i], list[i].Currency));
            }
            return res.Concat(conv).Select(p => p.ToString()).Join("\r\n");
        }
        public static ExchangeRates FromECB()
        {
            var exrts = new ExchangeRates() { Base = Currency.EUR };
            exrts.AddRate(new ExchangeRate(Currency.EUR, 1.0));
            using (var r = XmlReader.Create("http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml", new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }))
            {
                while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.Element && r.Name == "Cube")
                    {
                        var currency = r.GetAttribute("currency");
                        var rate = r.GetAttribute("rate");
                        if (string.IsNullOrEmpty(currency) || string.IsNullOrEmpty(rate))
                            continue;
                        var c = Currency.From(currency);
                        if (c != Currency.NONE)
                            exrts.AddRate(new ExchangeRate(Currency.From(currency), Util.Numeric(rate)));
                    }
                }
            }
            return exrts;
        }
        public static ExchangeRates FromFloatRates()
        {
            var exrts = new ExchangeRates() { Base = Currency.EUR };
            exrts.AddRate(new ExchangeRate(Currency.EUR, 1.0));
            using (var r = XmlReader.Create("http://www.floatrates.com/daily/EUR.xml", new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }))
            {
                Currency currency = Currency.NONE;
                while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.Element)
                    {
                        if(r.Name == "targetCurrency")
                            currency = Currency.From(r.Value);
                        if(r.Name == "exchangeRate" && currency != Currency.NONE)
                            exrts.AddRate(new ExchangeRate(currency, Util.Numeric(r.Value)));
                    }
                }
            }
            return exrts;
        }
        protected void AddRate(ExchangeRate rate)
        {
            Rates[rate.Currency] = rate;
        }
        public Price Convert(Price p, Currency toCurrency)
        {
            ExchangeRate from, to;
            if(Rates.TryGetValue(p.Currency, out from) && Rates.TryGetValue(toCurrency, out to))
            {
                double v = to.Rate / from.Rate;
                return new Price(p.Amount * v, toCurrency);
            }
            return null;
        }
    }
    [DataDynamic]
    public class ExchangeRate
    {
        [DataDynamic]
        private string currency { get { return Currency.Code; } set { Currency = Currency.From(value); } }
        public Currency Currency { get; private set; }
        [DataDynamic]
        public double Rate { get; private set; }
        public ExchangeRate(Currency currency, double rate)
        {
            this.Currency = currency;
            this.Rate = rate;
        }
    }
}
