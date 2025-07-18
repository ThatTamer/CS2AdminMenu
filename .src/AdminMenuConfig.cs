public class AdminMenuConfig
{
    public List<string> BanReasons { get; set; } = new()
    {
        "Harassment", "WallHacks/AimHacks", "Alt", "Racism", "Admin Disrespect", "Other"
    };

    public List<string> CtReasons { get; set; } = new()
    {
        "Mass FK", "Constant FK", "Improper Orders", "Gunplant", "Other"
    };

    public List<string> Reasons { get; set; } = new()
    {
        "Harassment", "Toxicity", "Mic Spam", "Advertising", "Admin Disrespect", "Other"
    };

    public List<string> MapNameList { get; set; } = new()
    {
        "jb_clouds_night", "jb_moti2_final", "jb_summer_jail"
    };

    public List<string> MapList { get; set; } = new()
    {
        "3466298211", "3306672714", "3243256342"
    };

    public Dictionary<string, int> Durations { get; set; } = new()
    {
        { "1 Minute", 1 },
        { "5 Minutes", 5 },
        { "10 Minutes", 10 },
        { "30 Minutes", 30 },
        { "1 Hour", 60 },
        { "1 Day", 1440 }
    };

    public Dictionary<string, double> GravityValue { get; set; } = new()
    {
        { "0.4", 0.4 },
        { "1.0", 1.0 },
        { "1.2", 1.2 }
    };

    public Dictionary<string, double> SpeedValue { get; set; } = new()
    {
        { "0.4", 0.4 },
        { "1.0", 1.0 },
        { "2.0", 2.0 }
    };
}