//using Bovision;
//using DocumentFormat.OpenXml.EMMA;
//using DocumentFormat.OpenXml.Office2010.Excel;
//using DocumentFormat.OpenXml.Vml;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.VisualBasic;

//namespace BVA.Controllers
//    {
//    [Authorize(Roles = "Administrator,admin")]
//    public class CardController : Controller
//        {
        
//        public List<object> Controls3 { get; set; } = new List<object>();
//        public List<string> Controls { get;  set; }=new List<string>();
//        public List<string> Controls2 { get; set; } = new List<string>();

//        public IActionResult Index(string id)
//            {
//            return View();
//            }
//        public void LDelete(int annonsid, int customerid)
//            {
//            BovisionFacade.Facade fc = new BovisionFacade.Facade(customerid);
//            fc.Delete(annonsid, customerid);
//            Show(customerid);
//            }
//        public IActionResult NyAgent()
//            {
//           return View();
//            }
//        private string MunicipalityName(int code)
//            {
//            var m = Municipality.ById(code);
//            return m != null ? m.Name : "N/A";
//            }
//        public IActionResult Show(int id)
//            {
//            var c = Customer.ById(id);
//            var u = BvUser.FromAgent(c);
//            const string sql = @"select v.Id, v.ClientId, v.Address, v.MunicipalityCode, v.Rooms, v.LivingArea, v.Registered, v.Deleted, tmp.count from v_listdatabostad_unionall v
//	                    RIGHT JOIN (
//		                select L_OBJEKTNR as Id, SUM(N_ANTALBESKRIV) as count
//		                from statobjekt where L_OBJEKTNR > 0 and N_MAKLARID=? and dat_datum >= ?  group by L_OBJEKTNR) tmp
//	                    ON tmp.Id = v.Id";
//            var orders = c.Orders;
//            if (c.Id != 0)
//                {
//                int invoicetoCount = 0;
//                var cid = new HashSet<int>(orders.Where(o => o.InvoiceTo > 0).Select(o => o.InvoiceTo));
//                if (cid.Count > 0)
//                    {
//                    foreach (var id2 in cid)
//                        {
//                        var lnk = id2.ToString();

//                        Controls.Add(lnk);
//                        }
//                    }

//                using (var dbi = new Dbi())
//                    {
//                    foreach (var r in dbi.Fetch("select distinct customerid from orderitem where invoiceto = ?", c.Id))
//                        {
//                        var lnk = r.GetInt32(0).ToString();
//                        Controls.Add(lnk);
//                        invoicetoCount++;
//                        }
//                    var rd = dbi.Fetch("select ISNULL(m.b_bovision,0) as Checked, ISNULL(m2.N_BV_ANNONSPAKET,1) as SelectedIndex from maklare as m join maklare2 as m2 on (m.N_MAKLARID = m2.N_MAKLARID) where m.N_MAKLARID=?", c.Id).FirstOrDefault();
//                    if (rd != null)
//                        {
//                  var x=      rd["Checked"].ToString();
//                        var Checked = rd["Checked"].ToString().Equals("1");
//                        var SelectedIndex = int.Parse(rd["SelectedIndex"].ToString()) - 1;
//                        List<SelectListItem> myList = new List<SelectListItem>();
//                        var data = new[]{
//                 new SelectListItem{ Value="1",Text="Bas"},
//                 new SelectListItem{ Value="2",Text="Medium"},
//                 new SelectListItem{ Value="3",Text="Plus"},
//                 new SelectListItem{ Value="4",Text="PlusPlus"},

//             };
//                        data[SelectedIndex].Selected = true;
//                        ViewBag.items  = data.ToList();
//                        ViewBag.BChecked=Checked;   
//                        }
//                    }
//                }
//                using (var dbi = new Dbi())
//                {
//                var start = new DateTime(DateAndTime.Now.Year, DateAndTime.Now.Month, 1);
//                var res = dbi.Fetch(sql, c.Id, start);
//              //  var list = res.Where(r => !r.IsDBNull(0)).Select(r => new { Id = r[0], ClientId = r[1], Address = r[2], Municipality = MunicipalityName(Convert.ToInt32(r[3])), Rooms = r[4], LivingArea = r[5], Registered = r[6], Deleted = r[7], Count = r[8] }).ToList();
//              //  crudList3.Setup(list, new CrudSettings { ExcelExport = true, Widths = new List<int>() { 80, 120, 120, 120, 40, 40, 114, 114 } });
//                }
//            using (var ctx = new Data<ImportEstate>())
//                {
//                var list = ctx.Find(est => est.AgentId == c.Id);
//                var src = list.Select(est => new { Id = est.Id, ClientId = est.ClientId, Address = est.Address, City = est.City, Type = est.ObjectType + ':' + est.ContractType, Rent = est.Rent, Price = est.Price, Currency = est.Currency, Url = string.IsNullOrEmpty(est.DescriptionUrl) ? ("http://bovision.se/Beskrivning/" + est.Id) : est.DescriptionUrl, Xml = "http://services.bovision.se/api/Export/" + est.Id }).ToList();
//                ViewBag.src = src;
               
//                }
//            var invoices = Invoice.Find(new[] { Expr.Eq("CustomerId", c.Id) }, new[] { new OrderBy("Created", Direction.Descend) });
//            ViewBag.invoices = invoices;
//            var notes = CustomerNote.Find(new[] { Expr.Eq("CustomerId", c.Id) }, new[] { new OrderBy("Created", Direction.Descend) });
//            ViewBag.notes = notes;
//            var orders2 = Order.HistoryByCustomer(c.Id);
//            var ao = orders2.OrderBy(o => !o.IsActive()).ThenByDescending(o => o.Id);
//            ViewBag.ao = ao;    
//            var cript = new CryptedUserId(u.Id).CryptedId;
//            ViewBag.cript = cript;  
            
//            return View(Customer.ById(id));
//            }

//        }
//    }
