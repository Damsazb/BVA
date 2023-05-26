using System;
using System.Collections.Generic;
using System.Linq;
using AjaxPro;

namespace Bovision
{
    public interface IAddress
    {
        string Address1 { get; set; }
        string Address2 { get; set; }
        string Address3 { get; set; }
        string CountryCode { get; set; }
        string PostalCode { get; set; }
        string PostalArea { get; set; }
    }

    public class Address : IAddress
    {
        public Address()
        {
        }

        public Address(IAddress addr)
        {
            Copy(addr, this);
        }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public string PostalArea { get; set; }

        public static void Copy(IAddress src, IAddress dest)
        {
            dest.Address1 = src.Address1;
            dest.Address2 = src.Address2;
            dest.Address3 = src.Address3;
            dest.CountryCode = src.CountryCode;
            dest.PostalCode = src.PostalCode;
            dest.PostalArea = src.PostalArea;
        }

        public static bool IsValid(IAddress address)
        {
            bool pc = !string.IsNullOrEmpty(address.PostalCode) &&
                      address.PostalCode.All(c => char.IsDigit(c) || char.IsWhiteSpace(c));
            return pc && !string.IsNullOrWhiteSpace(address.PostalArea);
        }

        public static bool IsEmpty(IAddress address)
        {
            return string.IsNullOrWhiteSpace(address.Address1) &&
                   string.IsNullOrWhiteSpace(address.Address2) &&
                   string.IsNullOrWhiteSpace(address.Address3) &&
                   string.IsNullOrWhiteSpace(address.PostalCode) &&
                   string.IsNullOrWhiteSpace(address.PostalArea);
        }
    }

    [DataDynamic]
    public class Group : BaseDataReadWriteId<Group>, IIdentityName, ILocalID
    {
        [DataDynamic] public DateTime Created = Date.Now;

        [DataDynamic] public string Password { get; set; }

        [DataDynamic(PrimaryKey = true)] public int Id { get; set; }

        [DataDynamic] public string Name { get; set; }
    }

    [DataDynamic]
    public class CustomerGroup : BaseDataReadWriteId<CustomerGroup>, ILocalID
    {
        [DataDynamic] public DateTime Created = Date.Now;

        [DataDynamic] public int CustomerId { get; set; }

        [DataDynamic] public int GroupId { get; set; }

        [DataDynamic(PrimaryKey = true)] public int Id { get; set; }
    }

    [DataDynamic]
    public class Customer : BaseDataReadWriteId<Customer>, IIdentityName, ILocalID, IAddress
    {
        public static LRUCache<int, Customer> cache = new LRUCache<int, Customer>(400, TimeSpan.FromSeconds(60));

        [DataDynamic] public string ContactEmail;

        [DataDynamic] public string ContactName;

        [DataDynamic] public string Currency = "SEK";

        [DataDynamic] public int Export;

        [DataDynamic(Size = 40)] public string Fax;

        [DataDynamic] public string InvoiceEmail;

        [DataDynamic] public string Logotype;

        [DataDynamic] public string MailingEmail;

        [DataDynamic] public string OrgNr;

        [DataDynamic] public string Password;

        [DataDynamic] public string PasswordServices;

        [DataDynamic(Size = 40)] public string Phone;

        [DataDynamic] public double SpecialDiscount;

        [DataDynamic] public int SpecialExpFee;

        [DataDynamic] public bool ToBlocket;

        [DataDynamic] public string VATno;

        [DataDynamic] public string Webadress;

        [AjaxPro.AjaxNonSerializable] public string DisplayName => Id.ToString() + " " + Name + ", " + PostalArea;

        [AjaxPro.AjaxNonSerializable]
        public IAddress Address
        {
            get => this;
            set => Bovision.Address.Copy(value, this);
        }

        [AjaxPro.AjaxNonSerializable]
        public IAddress DeliveryAddress
        {
            get =>
                new Address
                {
                    Address1 = DeliveryAddress1,
                    Address2 = DeliveryAddress2,
                    Address3 = DeliveryAddress3,
                    CountryCode = DeliveryCountryCode,
                    PostalCode = DeliveryPostalCode,
                    PostalArea = DeliveryPostalArea
                };
            set
            {
                DeliveryAddress1 = value.Address1;
                DeliveryAddress2 = value.Address2;
                DeliveryAddress3 = value.Address3;
                DeliveryCountryCode = value.CountryCode;
                DeliveryPostalCode = value.PostalCode;
                DeliveryPostalArea = value.PostalArea;
            }
        }

        [DataDynamic] public string DeliveryAddress1 { get; set; }

        [DataDynamic] public string DeliveryAddress2 { get; set; }

        [DataDynamic] public string DeliveryAddress3 { get; set; }

        [DataDynamic(Size = 2)] public string DeliveryCountryCode { get; set; }

        [DataDynamic(Size = 40)] public string DeliveryPostalCode { get; set; }

        [DataDynamic] public string DeliveryPostalArea { get; set; }

        [AjaxPro.AjaxNonSerializable] public List<CustomerContact> Contacts { get; set; }

        [AjaxPro.AjaxNonSerializable] public List<CustomerNote> Notes { get; set; }

        [AjaxPro.AjaxNonSerializable] public List<Order> Orders => Order.ByCustomer(this);

        [DataDynamic] public string Address1 { get; set; }

        [DataDynamic] public string Address2 { get; set; }

        [DataDynamic] public string Address3 { get; set; }

        [DataDynamic(Size = 2)] public string CountryCode { get; set; }

        [DataDynamic(Size = 40)] public string PostalCode { get; set; }

        [DataDynamic] public string PostalArea { get; set; }

        [DataDynamic(PrimaryKey = true)] public int Id { get; set; }

        [DataDynamic] public string Name { get; set; }

        public IAddress GetAddress()
        {
            if (Bovision.Address.IsValid(DeliveryAddress))
                return DeliveryAddress;
            return Address;
        }

        public static Customer ById(int Id)
        {
            Customer c;
            if (!cache.TryGetValue(Id, out c))
                cache.Add(Id, c = Get(Id));
            return c;
        }

        public override void Save()
        {
            cache.Remove(Id);
            base.Save();
        }

        public override Customer Update(params DataDynamic.Eq[] elist)
        {
            cache.Remove(Id);
            return base.Update(elist);
        }

        public override void Delete()
        {
            cache.Remove(Id);
            base.Delete();
        }

        public List<Invoice> Invoices()
        {
            return Invoice.ByCustomer(this);
        }

        public void InjectToBVMaklare()
        {
            using (var dbi = new Dbi())
            {
                dbi.Execute(@"insert into MAKLARE (N_MAKLARID,B_PUBLIK,B_BOVISION,B_OBJEKTVISION,B_OVPUBLIK, S_ORGNR,
                                                               S_MAKLARE,S_ADRESS,L_POSTNR,S_POSTADRESS,
                                                               S_TELE,S_NAMN,S_EMAIL,S_WWWADRESS,S_LOSEN,S_LOSENTJANSTER,S_LOSEN2)
                                            values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
                    Id, 1, 1, 0, 0, OrgNr,
                    Name, Address1, Util.atoi(new string(PostalCode.Where(c => char.IsDigit(c)).ToArray())), PostalArea,
                    Phone, ContactName, ContactEmail, Webadress, Password, Password, Password);
                dbi.Execute("insert into MAKLARE2 (N_MAKLARID,N_BV_ANNONSPAKET) values(?,?)", Id, 3);
                if (!string.IsNullOrEmpty(MailingEmail))
                    try
                    {
                        dbi.Execute(@"update MAKLARE2 set s_emailutskick=? where N_MAKLARID=?", MailingEmail, Id);
                    }
                    catch
                    {
                    }

                new CustomerNote {CustomerId = Id, Text = "Skapad", CreatedBy = Global.CurrentUser.Name}.Save();
            }
        }

        public void TransferToBVMaklare()
        {
            using (var dbi = new Dbi())
            {
                // 2015-05-20/RC Om orders finns på kund så sätts B_PUBLIK och B_BOVISION till 1 om man väljer "Överför till BVMaklare"
                // Oväntat om man kryssar ur!
                //TODO: 2015-05-20
                int hasOrders = Orders.Count > 0 ? 1 : 0;
                var res = dbi.Execute(@"update MAKLARE set B_PUBLIK=?,B_BOVISION=?,S_ORGNR=?,
                                  S_MAKLARE=?,S_ADRESS=?,L_POSTNR=?,S_POSTADRESS=?,
                                  S_TELE=?,S_NAMN=?,S_EMAIL=?,S_WWWADRESS=?
                                  where N_MAKLARID=?",
                    hasOrders, hasOrders, OrgNr,
                    Name, Address1, int.Parse(new string(PostalCode.Where(c => char.IsDigit(c)).ToArray())), PostalArea,
                    Phone, ContactName, ContactEmail, Webadress, Id);

                if (!string.IsNullOrEmpty(MailingEmail))
                    try
                    {
                        dbi.Execute(@"update MAKLARE2 set s_emailutskick=? where N_MAKLARID=?", MailingEmail, Id);
                    }
                    catch
                    {
                    }

                if (!string.IsNullOrEmpty(Password))
                    dbi.Execute(@"update MAKLARE set S_LOSEN=?,S_LOSENTJANSTER=?,S_LOSEN2=? where N_MAKLARID=?",
                        Password, Password, Password, Id);
            }
        }

        public List<Group> GetGroups()
        {
            var list = CustomerGroup.Find(Expr.Eq("CustomerId", Id));
            if (list.Count == 0)
                return new List<Group>();
            return Group.Find(Expr.In("Id", list.Select(i => i.GroupId)));
        }

        public void AddGroup(Group g)
        {
            var list = CustomerGroup.Find(Expr.Eq("CustomerId", Id), Expr.Eq("GroupId", g.Id));
            if (list.Count == 0)
                new CustomerGroup {CustomerId = Id, GroupId = g.Id}.Save();
        }

        public void RemoveGroup(Group g)
        {
            var list = CustomerGroup.Find(Expr.Eq("CustomerId", Id), Expr.Eq("GroupId", g.Id));
            foreach (var i in list)
                i.Delete();
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Id, Name);
        }
    }

    [DataDynamic]
    public class CustomerContact : BaseDataReadWriteId<CustomerContact>, IIdentityName, ILocalID
    {
        [DataDynamic] public string Email;

        [DataDynamic] public string Notes;

        [DataDynamic] public string Password;

        [DataDynamic(Size = 3)] public string Phone { get; set; }

        [DataDynamic(PrimaryKey = true)] public int Id { get; set; }

        [DataDynamic] public string Name { get; set; }
    }

    [DataDynamic]
    public class CustomerNote : BaseDataReadWriteId<CustomerNote>, ILocalID
    {
        [DataDynamic] public DateTime Created = Date.Now;

        [DataDynamic] public string CreatedBy;

        [DataDynamic] public DateTime Deleted = Date.Default;

        [DataDynamic(Size = 512)] public string Text;

        [DataDynamic] public int CustomerId { get; set; }

        [DataDynamic(PrimaryKey = true)] public int Id { get; set; }
    }
}