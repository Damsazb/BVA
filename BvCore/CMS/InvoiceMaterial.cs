using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    [DataDynamic]
    public class InvoiceMaterial : BaseDataReadWriteId<InvoiceMaterial>, ILocalID
    {
        public InvoiceMaterial() { }
        public InvoiceMaterial(Order order, DateTime period, double quantity, bool calculateDiscount)
        {
            var product = order.Product;
            this.CustomerId = order.CustomerId;
            this.InvoiceTo = order.InvoiceTo;
            this.Quantity = quantity;
            this.Unit = product.Unit ?? "st";
            this.ItemPrice = order.GetPrice(quantity, calculateDiscount);
            if (!calculateDiscount)
                this.Discount = order.GetDiscount(quantity);
            this.VATPct = order.VAT.Amount;
            this.Period = new DateTime(period.Year, period.Month, 1);
            var p = order.Product;
            this.Product = p;
            if (p.Type == ProductType.Micro || p.Type == ProductType.Mixed)
            {
                Quantity = 1;
                if (p.Code == "BVHYRAbon")
                {
                    Name = p.Name;
                }
                else
                {
                    Name = p.Name + " " + order.GetIntervall(quantity);
                }
            }
            this.Reference = order.Reference;
            if (!string.IsNullOrEmpty(order.OrderNr))
                this.Reference += ("/" + order.OrderNr);
        }

        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }

        [DataDynamic]
        public int CustomerId;
        [DataDynamic]
        public int InvoiceTo;
        [DataDynamic]
        public int InvoiceId;
        [DataDynamic]
        public DateTime Period = Date.Default;
        [DataDynamic]
        public DateTime Created = Date.Now;
        [DataDynamic]
        public DateTime Sent = Date.Default;

        [DataDynamic]
        protected int ProductId;
        [DataDynamic]
        public string ProductCode { get; protected set; }
        public Product Product { get { return Product.ById(ProductId); } set { ProductId = value.Id; Unit = value.Unit; ProductCode = value.Code; Name = value.Name; } }

        [DataDynamic]
        public string Name = "";
        [DataDynamic]
        public new string Details = "";
        [DataDynamic]
        public string Reference = "";
        [DataDynamic]
        public double Quantity;
        [DataDynamic]
        public string Unit = "";
        [DataDynamic]
        protected double VATPct;
        public VAT VAT { get { return VAT.ByAmount(VATPct); } set { VATPct = value.Amount; } }
        [DataDynamic]
        public double Amount;
        [DataDynamic]
        double DiscountAmount;
        public Price Discount { get { return new Price(DiscountAmount, Currency); }
            set {
                if (value.Currency != Currency)
                    throw new ArgumentException("Discount and Price must have same currency");
                DiscountAmount = value.Amount;
            }
        }

        [DataDynamic]
        protected string CurrencyCode;

        public Currency Currency { get { return Currency.From(CurrencyCode); } set { CurrencyCode = value == null ? Currency.NONE.Code : value.Code; } }

        public Price ItemPrice {
            get { return new Price(Amount, Currency.From(CurrencyCode)); }
            set { Amount = value.Amount; CurrencyCode = value.Currency.Code; }
        }
        public Price TotalPrice
        {
            get { return new Price((Amount * Quantity) - DiscountAmount, Currency.From(CurrencyCode)); }
        }

    }
}
