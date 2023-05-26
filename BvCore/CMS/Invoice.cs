using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bovision
{
    public class ExternalInvoiceId
    {
        public static ExternalInvoiceId Empty = new ExternalInvoiceId(ExternalInvoiceSystem.Internal, "");
        public ExternalInvoiceSystem System { get; private set; }
        public string Id { get; private set; }
        public ExternalInvoiceId(ExternalInvoiceSystem system, string id)
        {
            System = system == null ? ExternalInvoiceSystem.Unknown : system;
            Id = id == null ? "" : id;
        }
        public ExternalInvoiceId(ExternalInvoiceSystem system, int id)
        {
            System = system == null ? ExternalInvoiceSystem.Unknown : system;
            Id = id.ToString();
        }
        public ExternalInvoiceId(string system, string id)
        {
            System = ExternalInvoiceSystem.ByName(system);
            Id = id == null ? "" : id;
        }

        public static ExternalInvoiceId Create(ExternalInvoiceSystem system, string id)
        {
            return new ExternalInvoiceId(system, id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExternalInvoiceId);
        }
        public bool Equals(ExternalInvoiceId p)
        {
            if ((object)p == null)
                return false;
            return System.Name == p.System.Name && Id == p.Id;
        }
        public static bool operator ==(ExternalInvoiceId a, ExternalInvoiceId b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.System.Name == b.System.Name && a.Id == b.Id;
        }
        public static bool operator !=(ExternalInvoiceId a, ExternalInvoiceId b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return System.Name.GetHashCode() ^ Id.GetHashCode();
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Id))
                return "";
            return System.Name + ":" + Id;
        }
    }
    public class ExternalInvoiceSystem
    {
        public string Name { get; private set; }
        public static ExternalInvoiceSystem Speedfeed = new ExternalInvoiceSystem() { Name = "Speedfeed" };
        public static ExternalInvoiceSystem Billogram = new ExternalInvoiceSystem() { Name = "Billogram" };
        public static ExternalInvoiceSystem Internal = new ExternalInvoiceSystem() { Name = "Internal" };
        public static ExternalInvoiceSystem Unknown = new ExternalInvoiceSystem() { Name = "Unknown" };
        public static IEnumerable<ExternalInvoiceSystem> Systems { get { return systems.Values; } }
        private static Dictionary<string, ExternalInvoiceSystem> systems = new Dictionary<string,ExternalInvoiceSystem> {
                                                                    { Speedfeed.Name, Speedfeed },
                                                                    { Billogram.Name, Billogram },
                                                                    { Internal.Name, Internal },
                                                                    { Unknown.Name, Unknown },
        };
        private ExternalInvoiceSystem() { }
        public static ExternalInvoiceSystem ByName(string name)
        {
            ExternalInvoiceSystem v;
            if (systems.TryGetValue(name, out v))
                return v;
            return Unknown;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as ExternalInvoiceSystem);
        }
        public bool Equals(ExternalInvoiceSystem p)
        {
            if ((object)p == null)
                return false;
            return Name == p.Name;
        }
        public static bool operator ==(ExternalInvoiceSystem a, ExternalInvoiceSystem b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Name == b.Name;
        }
        public static bool operator !=(ExternalInvoiceSystem a, ExternalInvoiceSystem b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class InvoiceType
    {
        public string Name { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        private InvoiceType(string name, int min, int max)
        {
            this.Name = name;
            this.Min = min;
            this.Max = max;
        }
        public static InvoiceType Avtal = new InvoiceType("Avtal",100000,199999);
        public static InvoiceType Micro = new InvoiceType("Micro", 200000, 299999);
        public static InvoiceType Manuell = new InvoiceType("Manuell", 300000, 399999);
        public static InvoiceType Kredit = new InvoiceType("Kredit", 400000, 499999);
        public static InvoiceType Mixed = new InvoiceType("Mixed", 500000, 599999);
        public static InvoiceType Unknown = new InvoiceType("", 0, 0);

        public static IEnumerable<InvoiceType> Types { get { return new []{ Avtal, Micro, Manuell, Kredit, Mixed }; } }
        public static InvoiceType ByName(string name)
        {
            var ret = Types.FirstOrDefault( t => String.Compare(t.Name, name, true) == 0);
            return ret != null ? ret : Unknown;
        }
        public static InvoiceType ByNumber(int nr)
        {
            var ret = Types.FirstOrDefault( t => (nr >= t.Min && nr < t.Max));
            return ret != null ? ret : Unknown;
        }
    }
    [DataDynamic]
    public class Invoice : BaseDataReadWriteId<Invoice>, ILocalID, IIdentityName, IAddress
    {
        public static string OrgVAT = "";
        public static string OrgDomicile = "";
        public Invoice()
        {
            this.OurVAT = OrgVAT;
            this.Domicile = OrgDomicile;
        }
        public Invoice(Order order, InvoiceType Type) : this()
        {
            var customer = order.Customer;
            this.type = Type;
            this.Currency = order.Currency;
            this.Created = DateTime.Now;
            this.Name = customer.Name;
            if( Bovision.Address.IsEmpty(customer.DeliveryAddress) )
                this.Address = customer.Address;
            else
                this.Address = customer.DeliveryAddress;

            this.CustomerId = customer.Id;
            this.CustomerVAT = customer.VATno;
            this.DueDate = DateTime.Now.AddDays(30);
            this.InvoiceDate = Date.Now;
            this.Reference = order.Reference;
            this.OrderNo = order.OrderNr;
        
            this.TermsPayment = DueDate.Subtract(this.InvoiceDate).Days + " dagar";

            Product p = order.Product;

            var row = this.AddRow(p, order.InvoiceCyclesInMonths);

            string desc = "Avser ";
            var date = Date.Now.AddMonths(1);
            for (int i = 0; i < order.InvoiceCyclesInMonths; i++)
            {
                desc += Date.Month(date.Month) + " ";
                date = date.AddMonths(1);
            }
            this.AddRow(desc);
        }
        public Invoice(Customer Customer, InvoiceType Type, Currency currency, DateTime date) : this()
        {
            this.type = Type;
            this.Currency = currency;
            this.Created = DateTime.Now;
            this.Name = Customer.Name;
            if (Bovision.Address.IsValid(Customer.DeliveryAddress))
                this.Address = Customer.DeliveryAddress;
            else
                this.Address = Customer.Address;
            this.CustomerId = Customer.Id;
            this.CustomerVAT = Customer.VATno;
            this.InvoiceDate = date;
            this.DueDate = date.AddDays(30);
            if (Customer.Contacts != null && Customer.Contacts.Count > 0)
                this.Reference = Customer.Contacts.First().Name;

            this.TermsPayment = DueDate.Subtract(this.InvoiceDate).Days + " dagar";
        }
        public void SetDates(DateTime invoice_date, int payment_days)
        {
            InvoiceDate = invoice_date;
            DueDate = InvoiceDate.AddDays(payment_days);
            TermsPayment = DueDate.Subtract(InvoiceDate).Days + " dagar";
        }
        public void SetDates(DateTime invoice_date, DateTime due_date)
        {
            SetDates(invoice_date, DueDate.Subtract(InvoiceDate).Days);
        }
        public InvoiceRow AddRow(InvoiceRow row)
        {
            _rows.Add(row);
            return row;
        }
        public InvoiceRow AddRow(string info)
        {
            return AddRow("",info,"", Price.None,0,"", Price.None, Bovision.VAT.NoVAT);
        }
        public InvoiceRow AddRow(Product p, double quantity)
        {
            InvoiceRow row = new InvoiceRow();
            row.InvoiceId = this.InvoiceId;
            row.Quantity = quantity;
            row.Unit = p.Unit;
            row.Code = p.Code;
            row.Name = p.Name;
            row.Price = p.GetPrice(this.Currency).Amount;
            row.VATPct = p.VAT.Amount;
            return AddRow(row);
        }
        public InvoiceRow AddRow(string code, string title, string description, Price price, double quantity, string unit, Price discount, VAT vat)
        {
            InvoiceRow row = new InvoiceRow();
            row.InvoiceId = this.InvoiceId;
            row.Code = code;
            row.Name = title;
            // Max 200 tecken!
            row.Description = description.Length > 200 ? description.Substring ( 0, 200 ) : description;
            row.Price = price.Amount;
            row.Quantity = quantity;
            row.Unit = unit;
            row.VATPct = vat.Amount;
            row.Discount = discount.Amount;
            return AddRow(row);
        }
        public void CalculateRows(double expFee)
        {
            this.ExpFee = expFee;
            var lrows = GetRows();

            var rows_netto = lrows.Sum(r => r.Amount);
            double netto = rows_netto + Freight + ExpFee;
            double moms = Math.Abs(lrows.Sum(r => (r.Amount * r.VATPct)));
            if (rows_netto != 0.0 && moms == 0.0)
            {
                //this Customer should not pay VAT
            }
            else
            {
                moms += Bovision.VAT.Standard.Amount * Math.Abs(Freight);
                moms += Bovision.VAT.Standard.Amount * Math.Abs(ExpFee);
            }

            if (netto < 0.0)
                moms = -moms;

            double evenout = Math.Round(netto + moms, MidpointRounding.AwayFromZero) - (netto + moms);
            double brutto = netto + moms + evenout;

            this.Sum = rows_netto;
            this.VAT = moms;
            this.ExpFee = expFee;
            this.EvenOut = evenout;
            this.TotalSum = brutto;
        }

        [DataDynamic(PrimaryKey=true)]
        public int Id { get; set; }

        public bool IsCredit { get { return this.Type == InvoiceType.Kredit; } }

        [DataDynamic]
        public int InvoiceId = 0;

        private InvoiceType type = InvoiceType.Manuell;
        public InvoiceType Type {
            get { return InvoiceId == 0 ? type : InvoiceType.ByNumber(InvoiceId); }
            set { type = value; }
        }

        [DataDynamic]
        public int CustomerId = 0;
        public Customer Customer { get { return Customer.ById(CustomerId); } set { CustomerId = value.Id; } }

        [DataDynamic]
        public string Name { get; set; }

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

        [AjaxPro.AjaxNonSerializable]
        public IAddress Address {
            get { return this; }
            set {
                this.Address1 = value.Address1;
                this.Address2 = value.Address2;
                this.Address3 = value.Address3;
                this.CountryCode = value.CountryCode;
                this.PostalCode = value.PostalCode;
                this.PostalArea = value.PostalArea;
            }
        }
        [AjaxPro.AjaxNonSerializable]
        public IAddress DeliveryAddress
        {
            get
            {
                return new Address
                {
                    Address1 = this.DeliveryAddress1,
                    Address2 = this.DeliveryAddress2,
                    Address3 = this.DeliveryAddress3,
                    CountryCode = this.DeliveryCountryCode,
                    PostalCode = this.DeliveryPostalCode,
                    PostalArea = this.DeliveryPostalArea
                };
            }
            set
            {
                this.DeliveryAddress1 = value.Address1;
                this.DeliveryAddress2 = value.Address2;
                this.DeliveryAddress3 = value.Address3;
                this.DeliveryCountryCode = value.CountryCode;
                this.DeliveryPostalCode = value.PostalCode;
                this.DeliveryPostalArea = value.PostalArea;
            }
        }
        [DataDynamic]
        public string DeliveryName { get; set; }
        [DataDynamic]
        public string DeliveryAddress1 { get; set; }
        [DataDynamic]
        public string DeliveryAddress2 { get; set; }
        [DataDynamic]
        public string DeliveryAddress3 { get; set; }
        [DataDynamic(Size = 2)]
        public string DeliveryCountryCode { get; set; }
        [DataDynamic(Size = 40)]
        public string DeliveryPostalCode { get; set; }
        [DataDynamic]
        public string DeliveryPostalArea { get; set; }


        [DataDynamic]
        public string CustomerVAT;
        [DataDynamic]
        public string OurVAT;

        [DataDynamic]
        public string Reference;
        [DataDynamic]
        public string DeliveryTerms;
        [DataDynamic]
        public string DeliveryMode;
        [DataDynamic]
        public string TermsPayment;
        [DataDynamic]
        public DateTime DueDate = Date.Default;
        [DataDynamic]
        public string OurReference;
        [DataDynamic]
        public string OrderNo;
        [DataDynamic]
        public string Domicile;
        [DataDynamic]
        public double Freight;
        [DataDynamic]
        public double ExpFee;
        [DataDynamic]
        public double VAT;
        [DataDynamic]
        public double EvenOut;
        [DataDynamic]
        public double Sum;
        [DataDynamic]
        public double TotalSum;
        [DataDynamic]
        private string currency;
        public Currency Currency
        {
            get { return String.IsNullOrEmpty(currency) ? Currency.SEK : Currency.From(currency); }
            set { currency = value.Code; }
        }
        [DataDynamic]
        public string Notes = "";

        [DataDynamic]
        public DateTime InvoiceDate = Date.Default;
        [DataDynamic]
        public DateTime Created = Date.Default;
        [DataDynamic]
        public DateTime Sent = Date.Default;
        [DataDynamic]
        public DateTime Reminder = Date.Default;
        [DataDynamic]
        public DateTime Claim = Date.Default;
        [DataDynamic]
        public DateTime Payed = Date.Default;

        [DataDynamic]
        public DateTime Credit = Date.Default;
        [DataDynamic]
        public DateTime Deleted = Date.Default;
        [DataDynamic]
        public int PertainsToCredit;

        [DataDynamic]
        protected string externalSystem = ExternalInvoiceSystem.Unknown.Name;
        [DataDynamic]
        protected string externalId = "";

        public ExternalInvoiceSystem ExternalSystem
        {
            get { return ExternalInvoiceSystem.ByName(externalSystem); }
            set { externalSystem = (value != null) ? value.Name : ExternalInvoiceSystem.Unknown.Name; }
        }

        public ExternalInvoiceId ExternalId {
            get { return new ExternalInvoiceId(externalSystem, string.IsNullOrEmpty(externalId) ? InvoiceId.ToString() : externalId); }
            set
            {
                if (value != null)
                {
                    externalSystem = value.System.Name;
                    externalId = value.Id;
                }
            }
        }

        private List<InvoiceRow> GetRows()
        {
            var list = new List<InvoiceRow>(GetCommitedRows());
            list.AddRange(GetUnCommitedRows());
            return list;
        }
        private List<InvoiceRow> GetCommitedRows()
        {
            return InvoiceRow.Find(Expr.Eq("InvoiceId", InvoiceId));
        }
        private List<InvoiceRow> _rows = new List<InvoiceRow>();
        
        private List<InvoiceRow> GetUnCommitedRows()
        {
            return _rows;
        }
        [AjaxPro.AjaxNonSerializable]
        public List<InvoiceRow> Rows {
            get { return InvoiceId == 0 ? _rows : InvoiceRow.Find(Expr.Eq("InvoiceId", InvoiceId)); }
            set
            {
                if (InvoiceId == 0)
                    _rows = value;
                else
                {
                    var oldrows = Rows;
                    foreach (var row in value)
                    {
                        row.Id = 0;
                        row.InvoiceId = this.InvoiceId;
                        row.Save();
                    }
                    oldrows.ForEach(r => r.Delete());
                }
            }
        }

        public new void Delete()
        {
            this.Deleted = DateTime.Now;
            Save();
        }
        public void PhysicallyDelete()
        {
            foreach (var row in GetRows())
                row.Delete();
            base.Delete();
        }
        public new void Save()
        {
            if (InvoiceId == 0)
            {
                using (var dbi = new Dbi())
                {
                    this.InvoiceId = (int)dbi.Scalar("select isnull(MAX(InvoiceId),?) from invoice where InvoiceId<? and InvoiceId>=?", type.Min, type.Max, type.Min) + 1;
                }
            }
            foreach (var r in _rows)
            {
                r.InvoiceId = InvoiceId;
                r.Save();
            }
            base.Save();
        }

        public static Invoice ById(int Id)
        {
            return Invoice.Get(Expr.Eq("InvoiceId", Id));
        }
        public static List<Invoice> ByCustomer(Customer c)
        {
            return Invoice.Find(Expr.Eq("CustomerId", c.Id), Expr.LtEq("Deleted", Date.Default));
        }

        public Invoice MakeCopy()
        {
            Invoice inv = this.MemberwiseClone() as Invoice;
            inv.Id = 0;
            inv.InvoiceId = 0;
            List<InvoiceRow> list = new List<InvoiceRow>();
            foreach (var row in this.Rows)
            {
                row.Id = 0;
                row.InvoiceId = 0;
                list.Add(row);
            }
            inv.Rows = list;
            return inv;
        }

        public Invoice MakeCredit()
        {
            Invoice inv = this.MemberwiseClone() as Invoice;
            inv.Type = InvoiceType.Kredit;
            inv.Id = 0;
            inv.InvoiceId = 0;
            inv.Sent = Date.Default;
            inv.DueDate = DateTime.Now;
            inv.Sum = -inv.Sum;
            inv.VAT = -inv.VAT;
            inv.Freight = -inv.Freight;
            inv.ExpFee = -inv.ExpFee;
            inv.TotalSum = -inv.TotalSum;
            inv.TermsPayment = "";
            inv.PertainsToCredit = this.InvoiceId;
            //this.Rows = MakeCreditRows();
            inv.ExternalId = new ExternalInvoiceId(ExternalInvoiceSystem.Unknown, "");
            return inv;
        }
        public IEnumerable<InvoiceRow> MakeCreditRows()
        {
            List<InvoiceRow> list = new List<InvoiceRow>();
            foreach (var row in this.Rows)
            {
                row.Id = 0;
                row.InvoiceId = 0;
                row.Price = -row.Price;
                list.Add(row);
            }
            return list;
        }

        public static List<Invoice> SentInvoices(DateTime from)
        {
            return Invoice.Find(Expr.GtEq("InvoiceDate",from),Expr.Gt("Sent", Date.Treshold), Expr.LtEq("Deleted",Date.Treshold));
        }
        public static List<Invoice> SentInvoices()
        {
            return Invoice.Find(Expr.Gt("Sent", Date.Treshold), Expr.LtEq("Deleted", Date.Treshold));
        }
        public static List<Invoice> QueuedInvoices()
        {
            return Invoice.Find(Expr.LtEq("Sent", Date.Treshold), Expr.LtEq("Deleted", Date.Treshold));
        }



        public static bool operator ==(Invoice a, Invoice b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            if (a.Id == 0 && b.Id == 0)
                return a.Equals(b);
            return a.Id == b.Id;
        }
        public static bool operator !=(Invoice a, Invoice b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var o = obj as Invoice;
                
                if (o != null)
                {
                    if (Id == 0 && o.Id == 0)
                        return System.Object.ReferenceEquals(this, o);
                    return Id.Equals(o.Id);
                }
            }
            return false;
        }
    }
}
