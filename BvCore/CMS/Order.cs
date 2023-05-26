using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic("OrderItem")]
    public class Order : BaseDataReadWriteId<Order>, ILocalID, IAddress
    {
        public static List<Order> ByCustomer(Customer c)
        {
            return ByCustomer(c.Id);
        }
        public static List<Order> ByCustomer(int CustomerId)
        {
            return Order.Find(Expr.Eq("CustomerId", CustomerId), Expr.LtEq("Deleted", Date.Default), Expr.Or(Expr.LtEq("ContractEnds", Date.Default), Expr.Gt("ContractEnds", Date.Now)), Expr.Or(Expr.LtEq("ContractStarts", Date.Default), Expr.Lt("ContractStarts", Date.Now)));
        }
        public static List<Order> HistoryByCustomer(int CustomerId)
        {
            return Order.Find(Expr.Eq("CustomerId", CustomerId));
        }
        public static List<Order> ByProduct(Product p)
        {
            return ByProduct(p.Id);
        }
        public static List<Order> ByProduct(int ProductId)
        {
            return ByProducts(new[] { ProductId });
        }
        public static List<Order> ByProducts(IEnumerable<int> ProductIds)
        {
            return Order.Find(Expr.In("ProductId", ProductIds), Expr.LtEq("Deleted", Date.Default), Expr.Or(Expr.LtEq("ContractEnds", Date.Default), Expr.Gt("ContractEnds", Date.Now)));
        }
        public Order() { }
        public Order(Product p, Customer c, string reference, string orderNr, int invoiceTo)
        {
            var date = Date.Now;

            Customer = c;
            Address = c.Address;
            ByWho = Global.CurrentUser.Name;
            ContractStarts = new DateTime(date.Year, date.Month, 1);
            ContractEnds = Date.Default;
            Currency = ProductPrice.ByProduct(p.Id).First().Currency;
            Deleted = Date.Default;
            InvoiceCyclesInMonths = 3;
            OrderDate = date;
            Reference = reference;
            OrderNr = orderNr;
            VAT = VAT.Standard;
            ProductId = p.Id;
            InvoiceTo = invoiceTo;
        }

        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public int ProductId = 0;
        [AjaxPro.AjaxNonSerializable]
        public Product Product { get { return Product.ById(ProductId); } set { ProductId = value.Id; } }
        [DataDynamic]
        public int CustomerId = 0;
        [AjaxPro.AjaxNonSerializable]
        public Customer Customer { get { return Customer.ById(CustomerId); } set { CustomerId = value.Id; } }

        [DataDynamic]
        public DateTime ContractStarts;
        [DataDynamic]
        public DateTime ContractEnds;
        [DataDynamic]
        public int InvoiceCyclesInMonths;
        [DataDynamic]
        public string ByWho = "";
        [DataDynamic]
        public string How = "";
        [DataDynamic]
        public string Reference = "";
        [DataDynamic]
        public string OrderNr = "";
        [DataDynamic]
        public DateTime OrderDate = Date.Default;
        [DataDynamic]
        public DateTime Deleted = Date.Default;

        [DataDynamic]
        public double Discount;

        [DataDynamic("Currency")]
        private string currency;
        public Currency Currency { get { return Currency.From(currency); } set { currency = value.Code; } }
        [DataDynamic]
        public int VATId;

        [AjaxPro.AjaxNonSerializable]
        public VAT VAT { get { return VAT.ById(VATId); } set { VATId = value.Id; } }

        [DataDynamic]
        public int InvoiceTo = 0;

        public IAddress Address { get { return this; } set { Bovision.Address.Copy(value, this); } }

        [DataDynamic]
        public string Address1 { get; set; }
        [DataDynamic]
        public string Address2 { get; set; }
        [DataDynamic]
        public string Address3 { get; set; }
        [DataDynamic(Size = 2)]
        public string CountryCode { get; set; }
        [DataDynamic(Size = 40)]
        public string PostalCode { get; set; }
        [DataDynamic]
        public string PostalArea { get; set; }
        [DataDynamic]
        public double SpecialPrice { get; set; }
        public bool IsActive()
        {
            return IsActive(Date.Now);
        }
        public bool IsActive(DateTime d)
        {
            bool start = ContractStarts <= Date.Treshold || ContractStarts <= d;
            bool end = ContractEnds <= Date.Treshold || ContractEnds > d;
            return (Deleted <= Date.Treshold && start && end);
        }
        public bool ActiveInPeriod(DateTime starts, DateTime ends)
        {
            if(Deleted > Date.Treshold)
                return false;
            var order = new Period(ContractStarts, ContractEnds <= Date.Treshold ? DateTime.MaxValue : ContractEnds);
            var period = new Period(starts, ends);
            return period.IntersectsWith(order);
        }
        public string GetIntervall(double Count)
        {
            return Product.GetIntervall(Count, Currency);
        }
        public Price GetPrice(double Count, bool CalculateDiscount)
        {
            if (SpecialPrice > 0.0 && Discount > 0.0)
                throw new Exception("Inte specialpris och rabatt samtidigt");
            if (SpecialPrice > 0.0)
                return new Price(SpecialPrice * Count, Currency);
            var price = Product.GetPrice(Count, Currency);
            if (Discount > 0.0 && CalculateDiscount)
                price = new Price(price.Amount - (price.Amount * (Discount / 100.0)), price.Currency);
            return price;
        }
        public Price GetDiscount(double Count)
        {
            var price = new Price(0, Currency);
            var oprice = Product.GetPrice(Count, Currency);
            if (Discount > 0.0)
                price = new Price(oprice.Amount * (Discount / 100.0), Currency);
            return price;
        }

        public override string ToString()
        {
            var p = Product;
            if (p != null)
                return string.Format("{0} - {1}", p.Code, p.Name);
            return "N/A";
        }
    }
}
