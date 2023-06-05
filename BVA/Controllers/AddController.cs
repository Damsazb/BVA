using BVA.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BVA.Controllers
    {
    [Route("api/[controller]")]
    [ApiController]
    public class AddController : ControllerBase
        {
        private readonly BVAContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AddController(BVAContext context, IWebHostEnvironment hostEnvironment)
            {
            _dbContext = context;
            _webHostEnvironment = hostEnvironment;
            }
        [HttpGet]
        public ActionResult Adds()
            {

            return Ok(_dbContext.Annons.Where(m=>m.Enable==true && m.Publishing_date.Date <= DateTime.Now.Date && m.End_date_of_publication.Date>= DateTime.Now.Date).OrderBy(m=>m.Priority).ThenByDescending (m=>m.Publishing_date).ToList());
            }
        [HttpGet("new")]
        public ActionResult Adds(string municipality)
            {

            return Ok(_dbContext.Annons.Where(m => m.Enable == true && m.Publishing_date.Date <= DateTime.Now.Date && m.End_date_of_publication.Date >= DateTime.Now.Date&&(m.municipality.ToLower().Equals(municipality.ToLower())|| m.municipality.Equals("Hela Sverige"))).OrderBy(m => m.Priority).ThenByDescending(m => m.Publishing_date).ToList());
            }
        [HttpGet("AddsDev")]
        public ActionResult AddsDev()
            {

            return Ok(_dbContext.Annons.ToList());
            }
        }
    }
