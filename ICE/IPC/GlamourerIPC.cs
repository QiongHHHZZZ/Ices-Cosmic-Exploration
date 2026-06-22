using Dalamud.Plugin;
using Glamourer.Api.Enums;
using Glamourer.Api.IpcSubscribers;
using ICE.Utilities.Cosmic_Helper;

namespace ICE.IPC;

public class GlamourerIPC
{
    private readonly ApiVersion _apiVersion;
    private readonly SetItem _setItem;
    private readonly SetMetaState _setMetaState;

    public GlamourerIPC(IDalamudPluginInterface pi)
    {
        _apiVersion = new ApiVersion(pi);
        _setItem = new SetItem(pi);
        _setMetaState = new SetMetaState(pi);
    }

    public bool IsAvailable()
    {
        try { return _apiVersion.Invoke().Major == 1; }
        catch { return false; }
    }

    public bool SetHeadItem(uint itemId, byte stain = 0)
    {
        if (!IsAvailable()) return true;
        IceLogging.Verbose("Applying Clown Head");

        _setItem.Invoke(0, ApiEquipSlot.Head, itemId, [stain]);
        return true;
    }

    private bool _clownHeadApplied = false;

    public void SetClownHead()
    {
        if (!IsAvailable()) return;

        if (EzThrottler.Throttle("Apply Clown Head", 60_000))
        {
            Svc.Log.Info("[I.C.E.] Hey! Glamourer team if you're reading this somebody enabled a tehe setting\n" +
                "And either: Didn't read the tooltip, or did read the tooltip and still enabled it as a joke\n" +
                "This isn't a you bug, this is just me applying the clown head on them\n" +
                "Have a good day");

            if (!P.TaskManager.IsBusy)
            {
                P.TaskManager.Enqueue(() => ApplyStates(), "Setting Clown Head");
            }
        }
    }
    private int ThrottleCount = 0;
    public bool ApplyStates()
    {
        if (EzThrottler.Throttle("Apply State", 1000))
            ThrottleCount += 1;
        if (ThrottleCount == 1)
        {
            SetHeadItem(35863);
            ThrottleCount++;
        }
        else if (ThrottleCount == 3)
        {
            SetHat();
            ThrottleCount++;
        }
        else if (ThrottleCount == 5)
        {
            SetVisor();
            ThrottleCount = 0;
            return true;
        }
        return false;
    }

    public void ResetClownHead()
    {
        _clownHeadApplied = false;
    }
    public bool SetHat()
    {
        if (!IsAvailable()) return true;

        try
        {
            var hatEc = _setMetaState.Invoke(0, MetaFlag.HatState, true);
            IceLogging.Debug($"Hat: {hatEc}");
            return true;
        }
        catch (Exception e)
        {
            IceLogging.Debug($"SetMetaState threw: {e.Message}");
        }
        return false;
    }
    public bool SetVisor()
    {
        if (!IsAvailable()) return true;
        try
        {
            var hatEc = _setMetaState.Invoke(0, MetaFlag.VisorState, false);
            IceLogging.Debug($"Hat: {hatEc}");
            return true;
        }
        catch (Exception e)
        {
            IceLogging.Debug($"SetMetaState threw: {e.Message}");
        }
        return false;
    }
}