using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManagerV1.Models
{
    public class Material
    {
        public int Id { get; set; }                       
        public string Name { get; set; } = string.Empty;  
        public string? Description { get; set; }          
        public string? Code { get; set; }                 
        public string MeasuringUnit { get; set; } = "шт"; 
    }

}