using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using BVA.Models;
using Convert = System.Convert;
using MemoryStream = System.IO.MemoryStream;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using BVA.Data;
using LazZiya.ImageResize;
using DocumentFormat.OpenXml.Drawing;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace BVA.Controllers
    {
    [Authorize(Roles = "Administrator,admin")]
    public class AddsController : Controller
        {
        private readonly BVAContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AddsController(BVAContext context, IWebHostEnvironment hostEnvironment)
            {
            _dbContext = context;
            _webHostEnvironment = hostEnvironment;
            }

        public async Task<IActionResult> Index()
            {
            var employee = await _dbContext.Annons.ToListAsync();
            return View(employee);
            }


        public async Task<IActionResult> Editor(int id, string Picture)
            {
            var employee = await _dbContext.Annons.FindAsync(id);
            ViewBag.Picture = Picture;
            ViewBag.Id = id;
            string Folder = Path.Combine(_webHostEnvironment.WebRootPath, "Data\\kommuner.json");

            using (StreamReader r2 = new StreamReader(Folder))
                {
                string json2 = r2.ReadToEnd();
                r2.Close();
                ViewBag.kommuner = JsonConvert.DeserializeObject<List<string>>(json2);
                //   ViewBag.agent = JsonConvert.DeserializeObject<List<Agentslist>>(json).GroupBy(x => x.Agentname).Select(gr => new { CellID = gr.FirstOrDefault().AgentId, Count = gr.Count(), Agentname = gr.FirstOrDefault().Agentname, Ispayer = gr.FirstOrDefault().Ispayed.Equals("true"), System = gr.FirstOrDefault().System }).ToList().OrderByDescending(x => x.Count).ToList();

                }
        
            return View(employee);
            }

        public IActionResult New()
            {
            string Folder = Path.Combine(_webHostEnvironment.WebRootPath, "Data\\kommuner.json");
            
            using (StreamReader r2 = new StreamReader(Folder))
                {
                string json2 = r2.ReadToEnd();
                r2.Close();
            ViewBag.kommuner = JsonConvert.DeserializeObject<List<string>>(json2);
                //   ViewBag.agent = JsonConvert.DeserializeObject<List<Agentslist>>(json).GroupBy(x => x.Agentname).Select(gr => new { CellID = gr.FirstOrDefault().AgentId, Count = gr.Count(), Agentname = gr.FirstOrDefault().Agentname, Ispayer = gr.FirstOrDefault().Ispayed.Equals("true"), System = gr.FirstOrDefault().System }).ToList().OrderByDescending(x => x.Count).ToList();

                }
            return View();
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(AnnonsViewModel model)
            {
            if (ModelState.IsValid)
                {
                string uniqueFileName = UploadedFile(model);

                Annons annons = new Annons
                    {
                    Name = model.Name,
                    Publishing_date = model.Publishing_date,
                    Place = model.Place,
                    Priority = model.Priority,
                    End_date_of_publication = model.End_date_of_publication,
                    Url = model.Url,
                    municipality = model.municipality,
                    Picture = uniqueFileName,
                    };
                _dbContext.Add(annons);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
                }
            string Folder = Path.Combine(_webHostEnvironment.WebRootPath, "Data\\kommuner.json");

            using (StreamReader r2 = new StreamReader(Folder))
                {
                string json2 = r2.ReadToEnd();
                r2.Close();
                ViewBag.kommuner = JsonConvert.DeserializeObject<List<string>>(json2);
                //   ViewBag.agent = JsonConvert.DeserializeObject<List<Agentslist>>(json).GroupBy(x => x.Agentname).Select(gr => new { CellID = gr.FirstOrDefault().AgentId, Count = gr.Count(), Agentname = gr.FirstOrDefault().Agentname, Ispayer = gr.FirstOrDefault().Ispayed.Equals("true"), System = gr.FirstOrDefault().System }).ToList().OrderByDescending(x => x.Count).ToList();

                }
            return View();
            }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Annons model)
            {
            if (ModelState.IsValid)
                {
                // string uniqueFileName = UploadedFile(model);


                _dbContext.Update(model);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
                }
            return View();
            }
        private string UploadedFile(AnnonsViewModel model)
            {
            
            string uniqueFileName = null;

            if (model.Image != null)
                {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                    model.Image.CopyTo(fileStream);
                    }
                
            var img = Image.FromFile(filePath);

            //scale and crop, 600x250 center focus 
            var newImg = ImageResize.ScaleAndCrop(img, 600, 250, TargetSpot.Center);

            //watermark image path
            var imgWatermark = uploadsFolder+ "\\img_568478.png";

            //add image watermark
            var iOps = new ImageWatermarkOptions
                {
                // Change image opacity (0 - 100)
                Opacity = 100,


                // Change image watermark location
                Location = TargetSpot.TopRight,
                Margin=0
                };
                 newImg.AddImageWatermark(imgWatermark, iOps);                 //opacity (0-100)
                img.Dispose();
                //save new image
                // System.IO.File.Delete(filePath);
              //  newImg.SaveAs(filePath);

            //dispose to free up memory

            newImg.Dispose();
                }
            return uniqueFileName;
            }
        [HttpPost]
        public async Task<IActionResult> Delete2(string img, string src, int id)
            {
            string uniqueFileName = null;
            try
                {
                var employee = await _dbContext.Annons.FindAsync(id);
                var base64Data = Regex.Match(img, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                var binData = Convert.FromBase64String(base64Data);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = employee.Picture;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                //   var s = File(binData, "image/png");
                System.IO.File.WriteAllBytes(filePath, binData);

                }
            catch (Exception ex)
                {
                Console.WriteLine(ex.Message);
                }
            return RedirectToAction(nameof(Index));
            }
        [HttpPost]
        public async Task checkbox(int id , bool val)
            {
            var model = await _dbContext.Annons.FindAsync(id);
            model.Enable=val;
            _dbContext.Update(model);
            await _dbContext.SaveChangesAsync();
            }
        public async Task<IActionResult> Delete(int Id)
            {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            string uniqueFileName = "";
            var employee = await _dbContext.Annons.FindAsync(Id);
            uniqueFileName = employee.Picture;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            _dbContext.Remove(employee);
            System.IO.File.Delete(filePath);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            }

        }
    }
