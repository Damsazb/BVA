//using Bovision;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Reflection.Emit;
//using NuGet.Protocol;
//using Newtonsoft.Json;

//namespace BVA.Controllers
//    {
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AgentController : ControllerBase
//        {
//        private List<Customer> list=new List<Customer>();
//        private Customer c;
//        [HttpGet("getall")]
//        public async Task<string> getall(string id)
//            {


//            list.AddRange ( Customer.Find());
             
//            var x = JsonConvert.SerializeObject(list.Select(x=>x.DisplayName), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

//            return x;
//            }
//        [HttpGet("autocomplete")]
//        public async Task<string> autocomplete(string id)
//            {
//            int id2 = 0;


//            if (id.Length > 1)
//                {
//                if (int.TryParse(id, out id2))
//                    {
//                    c = Customer.ById(id2);

//                    //   c.Address = c.Address;
//                    if (c != null)
//                        list.Add(c);
//                    if (id.Length > 6)
//                        {
//                        var inv = Invoice.Get(Expr.Eq("InvoiceId", id));
//                        if (inv != null)
//                            {
//                            var c = Customer.ById(inv.CustomerId);
//                            if (c != null)
//                                list.Add(Customer.ById(inv.CustomerId));
//                            //  flowLayoutPanel1.Controls.Add(new SearchRow(c, inv, null));

//                            }

//                        }
//                    }
//                else if (id.Length > 2)
//                    {
//                    var cs = Customer.Find(Expr.Like("Name", "%" + id + "%"));
//                    var orgnr = Customer.Find(Expr.Like("OrgNr", id + "%"));
//                    list.AddRange(cs.Concat(orgnr));
//                    //  list = id.Length > 7 ? list.AsEnumerable() : list.Take(15);

//                    }
//                }
//            var y = (from b in list select new {id= b.Id, DisplayName=b.DisplayName}).ToList();
//          //  var x = JsonConvert.SerializeObject(list.Select( x => x.DisplayName ), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

//            return y.ToJson();
//            }
//        }
//    }
