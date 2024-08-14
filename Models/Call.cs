using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Build.Framework;

namespace Rolled_metal_products.Models
{
    public class Call
    {
        public int Id { get; set; }
        [DisplayName("Тема сообщения")]
        public string Subject { get; set; }
        [DisplayName("Текст")]
        public string HtmlMessage { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [DisplayName("Имя")]
        public string Name { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [Display(Name = "Телефон")]
        [Phone]
        public string Telefon { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [DisplayName("Название товара")]
        public string ProductName { get; set; }
        [DisplayName("Дата")]
        public DateTime Date { get; set; }
    }
}
