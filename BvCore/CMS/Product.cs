using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public class ProductType
    {
        public string Name { get; private set; }
        private ProductType(string name)
        {
            this.Name = name;
        }
        public static ProductType Avtal = new ProductType("Avtal");
        public static ProductType Micro = new ProductType("Micro");
        public static ProductType Komplement = new ProductType("Komplement");
        public static ProductType Mixed = new ProductType("Mixed");
        public static ProductType Manuell = new ProductType("Manuell");
        public static ProductType Unknown = new ProductType("");

        public static IEnumerable<ProductType> Types { get { return new[] { Avtal, Micro, Komplement, Manuell, Mixed }; } }
        public static ProductType ByName(string name)
        {
            var ret = Types.FirstOrDefault(t => String.Compare(t.Name, name, true) == 0);
            return ret != null ? ret : Unknown;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class PriceType
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        private PriceType(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
        public static PriceType Linear = new PriceType(1,"Linjär");
        public static PriceType Tiered = new PriceType(2,"Stafflad");
        public static PriceType Mixed = new PriceType(3, "Mixed");
        public static PriceType Unknown = new PriceType(0,"");

        public static IEnumerable<PriceType> Types { get { return new[] { Linear, Tiered }; } }
        public static PriceType ByName(string name)
        {
            var ret = Types.FirstOrDefault(t => String.Compare(t.Name, name, true) == 0);
            return ret != null ? ret : Unknown;
        }
        public static PriceType ById(int Id)
        {
            var ret = Types.FirstOrDefault(t => t.Id == Id);
            return ret != null ? ret : Unknown;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class PriceTier
    {
        public int Start { get; set; }
        public double Price { get; set; }
    }
    public class PriceTierIntervall
    {
        public int Start { get; set; }
        public int End { get; set; }
        public double Price { get; set; }
    }
    public class PriceTierSet
    {
        private List<PriceTier> tiers = new List<PriceTier>();
        public Currency Currency = Currency.SEK;
        public List<PriceTier> Items
        {
            get { return tiers; }
            set { tiers = value.OrderBy(i => i.Start).ToList(); }
        }
        public void SetTiers( IEnumerable<PriceTier> tiers )
        {
            this.tiers = tiers.OrderBy(i => i.Start).ToList();
        }
        public PriceTierIntervall GetPriceTier(double Count)
        {
            // 1-7      350
            // 8-15     500     countToTier
            // 16-30    700     2:a record

            var pti = new PriceTierIntervall();

            var sortedTier = tiers
                .OrderBy(i => i.Start);

            var inTier = sortedTier
                .Where(i => Convert.ToDouble(i.Start).CompareTo(Count) <= 0);

            if (inTier == null || inTier.Count() == 0 )
            {
                pti.Start = 0;
                pti.Price = 0.0;
            }
            else
            {
                pti.Start = inTier.LastOrDefault().Start;
                pti.Price = inTier.LastOrDefault().Price;
            }

            var afterTier = sortedTier
                .Where(i => Convert.ToDouble(i.Start).CompareTo(Count) > 0);

            if (afterTier == null || afterTier.Count() == 0)
            {
                pti.End = 9999;
            }
            else
            {
                pti.End = afterTier.FirstOrDefault().Start - 1;
            }

            return pti; 
        }
    }

    [DataDynamic]
    public class ProductPrice : BaseDataReadWriteId<ProductPrice>, ILocalID
    {
        [DataDynamic(PrimaryKey=true)]
        public int Id { get; set; }
        [DataDynamic]
        public int ProductId { get; set; }
        [DataDynamic]
        public double Amount { get; set; }
        [DataDynamic]
        public string CurrencyCode { get; set; }

        public ProductPrice()
        { }
        public ProductPrice(int ProductId)
        {
            this.ProductId = ProductId;
        }

        public Price Price
        {
            get { return new Price(Amount, Currency.From(CurrencyCode)); }
            set { Amount = value.Amount; CurrencyCode = value.Currency.Code; }
        }
        public Currency Currency
        {
            get { return Currency.From(CurrencyCode); }
            set { CurrencyCode = value.Code; }
        }
        public Product Product
        {
            get { return Product.ById(ProductId); }
            set { ProductId = value.Id; }
        }

        public static ProductPrice ById(int Id)
        {
            return ProductPrice.Get(Id);
        }
        public static List<ProductPrice> ByProduct(int ProductId)
        {
            return ProductPrice.Find(Expr.Eq("ProductId", ProductId));
        }
        public static ProductPrice ByCurrency(int ProductId, Currency c)
        {
            return ProductPrice.Get(Expr.Eq("ProductId", ProductId),Expr.Eq("CurrencyCode",c.Code));
        }
    }
    [DataDynamic]
    public class Product : BaseDataReadWriteId<Product>, ILocalID, IIdentityName
    {
        [DataDynamic(PrimaryKey = true)]
        public int Id { get; set; }
        [DataDynamic]
        public string Name { get; set; }
        [DataDynamic]
        public string Code { get; set; }
        [DataDynamic]
        public string Description { get; set; }
        //[DataDynamic]
        //public double Price { get; set; }
        [DataDynamic(Size = 10)]
        public string Unit { get; set; }
        [DataDynamic]
        public int InvoiceCyclesInMoths { get; set; }
        [DataDynamic]
        private int VATId = VAT.Standard.Id;
        [System.Xml.Serialization.XmlIgnore]
        [AjaxPro.AjaxNonSerializable]
        public VAT VAT { get { return VAT.ById(VATId); } set { VATId = value.Id; } }
        [DataDynamic]
        private int priceType = PriceType.Linear.Id;
        [System.Xml.Serialization.XmlIgnore]
        [AjaxPro.AjaxNonSerializable]
        public PriceType PriceType { get { return PriceType.ById(priceType); } set { priceType = value.Id; } }
        [DataDynamic]
        public string Notes { get; set; }
        [DataDynamic(FieldName="Type")]
        public string TypeName { get; set; }
        public ProductType Type { get { return ProductType.ByName(TypeName); } set { TypeName = value.Name; } }


        [DataDynamic(Size=2000)]
        public string Rules {
            get { return JsonConvert.SerializeObject(PriceSets); }
            set { priceSets = JsonConvert.DeserializeObject<List<PriceTierSet>>(value); }
        }
        private List<PriceTierSet> priceSets = null;
        public List<PriceTierSet> PriceSets {
            get { return priceSets == null ? priceSets = new List<PriceTierSet>() : priceSets; }
            set { priceSets = value; }
        }

        [DataDynamic]
        public DateTime Deleted = Date.Default;

        public static event EventHandler<CrudEvent> CrudHappend;

        private static List<Product> all;

        public static Product ByCode(string code)
        {
            return All.FirstOrDefault(p => p.Code == code);
        }
        public static Product ById(int Id)
        {
            return All.FirstOrDefault(p => p.Id == Id);
        }
        public static IEnumerable<Product> All
        {
            get
            {
                if (all == null)
                {
                    all = Product.Find(new OrderBy("Code", Direction.Ascend));
                    if (CrudHappend != null)
                        CrudHappend(all, CrudEvent.Refresh);
                }
                return all;
            }
        }
        public static IEnumerable<Product> Active { get { return All.Where(p => p.Deleted < Date.Treshold); } }
        
        public new void Save()
        {
            base.Save();
            all = Product.Find(new OrderBy("Code", Direction.Ascend));
            if (CrudHappend != null)
                CrudHappend(this, Id == 0 ? CrudEvent.Create : CrudEvent.Update);
        }
        public new void ForceInsert()
        {
            throw new NotImplementedException();
        }
        public new void Delete()
        {
            base.Delete();
            ProductPrice.ByProduct(Id).ForEach(p => p.Delete());
            all = Product.Find(new OrderBy("Code", Direction.Ascend));
            if (CrudHappend != null)
                CrudHappend(this, CrudEvent.Delete);
        }
        public override string ToString()
        {
            return string.Format("{0} - {1}", Code, Name);
        }
        public string GetIntervall(double Count, Currency currency)
        {
            if (PriceType == PriceType.Linear || Count == 0)
                return "";
            
            var set = PriceSets.FirstOrDefault(p => p.Currency == currency);
            var pti = set.GetPriceTier(Count);

            // Sista posten i stafflingen
            if (pti.Start < pti.End && pti.End == 9999)
            {
                return String.Format(">{0} st", pti.Start);
            }
            // "Normala" posten  
            else if (pti.Start < pti.End)
            {
                return String.Format("{0}-{1} st", pti.Start, pti.End);
            }
            // Ifall det är 1 steg mellan posterna
            else if ( pti.Start == pti.End )
            {
                return String.Format("{0} st", pti.Start);
            }
            // Felfall
            else
            {
                return String.Format("{0}-{1} st (Intervallfel!)", pti.Start, pti.End);
            }
        }

        public bool HasCurrency(Currency currency)
        {
            if (PriceType == PriceType.Linear && ProductPrice.ByCurrency(Id, currency) == null)
                return false;
            var set = PriceSets.FirstOrDefault(p => p.Currency == currency);
            return (set != null);
        }
        public Price GetPrice(Currency currency)
        {
            var pprice = ProductPrice.ByCurrency(Id, currency);
            if (pprice == null)
                return new Price(0, Currency.SEK);
            return pprice.Price;
        }
        public Price GetPrice(double Count, Currency currency)
        {
            double price = 0.0;
            if (PriceType == PriceType.Linear)
            {
                var p = ProductPrice.ByCurrency(Id, currency).Price;
                return new Price(p.Amount * Count, p.Currency);
            }
            else if (PriceType == PriceType.Tiered)
            {
                var set = PriceSets.FirstOrDefault(p => p.Currency == currency);
                price = set.GetPriceTier(Count).Price;
                return new Price(price, currency);
            }
            return new Price(price,currency);
        }

    }

    //Alter Table [orderitem] Drop Column Active
    //alter table [orderitem]
    //add Active AS CAST(
    //    CASE WHEN deleted <= '1901-01-01' and
    //              (contractends <= '1901-01-01' or contractends > getdate()) and
    //              (contractstarts <= '1901-01-01' or contractstarts < getdate())
    //    THEN 1 else 0 END AS bit)
}
