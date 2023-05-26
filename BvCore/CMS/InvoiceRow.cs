using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bovision
{
    [DataDynamic]
    public class InvoiceRow : BaseDataReadWriteId<InvoiceRow>, ILocalID
    {
        [DataDynamic(PrimaryKey=true)]
        public int Id { get; set; }

        [DataDynamic]
        public int InvoiceId = 0;
        //public Invoice Invoice { get { return InvoiceId > 0 ? Invoice.Get(InvoiceId) : null; } set { InvoiceId = value.Id; } }

        [DataDynamic]
        public string Code = "";
        [DataDynamic]
        public string Name = "";
        [DataDynamic]
        public string Description = "";
        [DataDynamic]
        public double Quantity;
        [DataDynamic]
        public string Unit = "";
        [DataDynamic]
        public double VATPct;
        [DataDynamic]
        public double Price;
        [DataDynamic]
        public double Discount;
        public double Amount { get { return (Price * Quantity) - Discount; } }

        [AjaxPro.AjaxNonSerializable]
        public bool IsEmpty { get {
            return String.IsNullOrWhiteSpace(Code) && String.IsNullOrWhiteSpace(Name)
                && Price == 0.0 && Amount == 0.0;
        } }
    }
}
