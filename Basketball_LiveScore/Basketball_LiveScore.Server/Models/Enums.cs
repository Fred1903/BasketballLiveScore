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

    public enum QuarterDuration
    {
        EightMinutes = 8,
        TenMinutes = 10,
        TwelveMinutes = 12
    }

    public enum TimeOutDuration
    {
        ThirtySeconds = 30,
        SixtySeconds = 60,
        SeventyFiveSeconds = 75
    }

    public enum NumberOfQuarters
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5
    }
}
