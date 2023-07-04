using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BVA.Models
{
    public class Annons
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ange namnet på annonsören")]
        [Display(Name = "Namn")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Var ska annonsen placeras?")]
        [Display(Name = "Plats")]
        [StringLength(100)]
        public string Place { get; set; }

        [Required(ErrorMessage = "Ange publiceringsdatum")]
        [Display(Name = "publiceringsdatum")]
        public DateTime Publishing_date { get; set; }

        [Required(ErrorMessage = "Ange slutdatumet för publiceringen")]
        [Display(Name = "slutdatumet för publiceringen")]
        public DateTime End_date_of_publication { get; set; }

        [Required(ErrorMessage = "Ange prioritet")]
        [Display(Name = "Prioritet")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Ange länken")]
        [Display(Name = "Länken")]
        public string Url { get; set; }

        [Required(ErrorMessage = "Välj en bild")]
        [Display(Name = "Bild")]
        public string Picture { get; set; }

        [Required(ErrorMessage = "Välj en kommun")]
        [Display(Name = "kommun")]
        public string municipality { get; set; }
        [Required(ErrorMessage = "Aktiv")]
        [Display(Name = "Aktiv")]
        public bool Enable { get; set; }
        [Required(ErrorMessage = "Aktiv")]
        [Display(Name = "Aktiv")]
        public bool TestEnable { get; set; }

        public Annons()
        {
          
        }
    }
}
