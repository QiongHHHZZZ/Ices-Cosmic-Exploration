namespace ICE.ConfigFiles;

public partial class Config
{
    public bool Cosmodrone_Buy { get; set; } = true;
    public int Cosmodrone_BuyAt { get; set; } = 4000;
    public int Cosmodrone_MaxKeep { get; set; } = 0;
    public bool Cosmodrone_Run { get; set; } = false;
    public bool Cosmodrone_UseDailyRoutinesTP { get; set; } = false;
}
