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
        OneMinute=1,
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

    public enum TimeOutAmount
    {
        Three = 3,
        Four = 4,
        Five = 5,
        Six=6,
        Seven=7,
        Eight=8
    }

    public enum NumberOfQuarters
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5
    }

    public enum FoulTypes
    {
        P0,
        P1,
        P2,
        P3, 
    }

    public enum BasketPoints
    {
        One = 1,  //lancer franc
        Two = 2,  //panier normal
        Three = 3 // en-dehors de la surface
    }

    public enum MatchStatus
    {
        NotStarted,
        Live,
        Finished,
        Cancelled
    }

}
