using System.ComponentModel.DataAnnotations;

namespace Basketball_LiveScore.Server.DTO
{
    public class CreateTeamDTO
    {
        [MaxLength(50)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(30)]
        public string? Coach { get; set; }
    }
}
