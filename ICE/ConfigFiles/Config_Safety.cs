using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.ConfigFiles;

public partial class Config
{
    public bool StopOnAbort { get; set; } = true;
    public bool RejectUnknownYesno { get; set; } = true;
    public bool DelayGrabMission { get; set; } = true;
    public int DelayIncrease { get; set; } = 500;
    public bool DelayCraft { get; set; } = true;
    public int DelayCraftIncrease { get; set; } = 2500;
    public bool AnimationLockAbandon { get; set; } = true;
    public bool JumpIfStuck_V2 { get; set; } = true;
    public bool RetargetIfStuck { get; set; } = false;
    public int StuckDelayMs { get; set; } = 1000;
    public int DelayPostRelic { get; set; } = 0;
    public bool Delay_Aethernet { get; set; } = false;
    public bool Delay_Gather { get; set; } = false;
}
