using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FloraMind_V1.Models
{
    public class UserPlant
    {
        [Key]
        public int UserPlantID { get; set; } // Kullanıcı-Bitki İlişki ID'si

        [Display(Name = "Bitki Takma Adı")]
        public string? Nickname { get; set; }

        public DateTime DateAdopted { get; set; } = DateTime.Now; // Bitkinin Sahiplenilme Tarihi

        public DateTime LastWatered { get; set; } // Bitkinin Son Sulanma Tarihi

        public double WateringIntervalHours { get; set; } // Sulama Aralığı (Saat Cinsinden)

        public DateTime? NextWateringDate { get;set; } // Bir sonraki sulama tarihi

        public bool? IsEmailSent { get; set; } // Sulama Hatırlatma E-postası Gönderildi mi?



        //Foreign Keys


        // Hangi Kullanıcıya ait ?
        public int UserID { get; set; } 

        public User User { get; set; }

        // Katalogdaki hangi türden geldi ?

        public int PlantID { get; set; } 

        [ValidateNever]
        public Plant Plant { get; set; }


        public void PerformWatering()
        {
            LastWatered = DateTime.Now;
            NextWateringDate = LastWatered.AddHours(WateringIntervalHours);
            IsEmailSent = false; // Sulama yapıldıktan sonra e-posta gönderim durumu sıfırlanır


        }




    }
}
