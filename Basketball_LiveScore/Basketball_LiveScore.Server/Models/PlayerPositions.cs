using System.ComponentModel;

namespace Basketball_LiveScore.Server.Models
{
    public enum PlayerPosition
    {
        [Description("Point Guard")] //Description permet que dans le front on va afficher avec un espace
        PointGuard,

        [Description("Shooting Guard")]
        ShootingGuard,

        [Description("Small Forward")]
        SmallForward,

        [Description("Power Forward")]
        PowerForward,
        Center        
    }
}
