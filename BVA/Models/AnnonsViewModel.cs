using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace BVA.Models
{
 
    public class AnnonsViewModel
    {
     
        [Required(ErrorMessage = "Ange namnet på annonsören")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Var ska annonsen placeras?")]
        [Display(Name = "place")]
        public string Place { get; set; }

        [Required(ErrorMessage = "Ange publiceringsdatum")]
        [DataType(DataType.Date)]
        [CustomStartDate(ErrorMessage = "Date must be more than or equal to Today's Date")]
        public DateTime Publishing_date { get; set; }

        [Required(ErrorMessage = "Ange slutdatumet för publiceringen")]
        [DataType(DataType.Date)]
        public DateTime End_date_of_publication { get; set; }

        [Required(ErrorMessage = "Ange prioritet")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Ange länken")]
        public string Url { get; set; }

        [Required(ErrorMessage = "Välj en bild")]
        [Display(Name = "Reklamfoto")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "Välj en kommun")]
        [Display(Name = "kommun")]
        public string municipality { get; set; }

        [Required(ErrorMessage = "Aktiv")]
        [Display(Name = "Aktiv")]
        public bool Enable { get; set; }

        }

    public class CustomStartDate : ValidationAttribute
        {
        public override bool IsValid(object value)
            {
            DateTime dateTime = Convert.ToDateTime(value);
            return dateTime.Date >= DateTime.Now.Date;
            }
        }

    }



