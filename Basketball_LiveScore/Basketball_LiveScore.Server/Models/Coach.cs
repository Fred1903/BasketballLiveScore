using System.ComponentModel.DataAnnotations;

namespace Basketball_LiveScore.Server.Models
{
    public class Coach
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string ?FirstName { get; set; }

        [Required]
        [MaxLength(30)]
        public string ?LastName { get; set; }
        [Required]
        public Team ?Team { get; set; }
    }
}
