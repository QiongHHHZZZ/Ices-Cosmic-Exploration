using ECommons.EzIpcManager;
using NotificationMasterAPI;

namespace ICE.IPC
{
    public class NotificationMasterIPC
    {
        public const string Name = "NotificationMaster";
        public const string Repo = "https://github.com/NightmareXIV/NotificationMaster";

        private const string RequesterName = "ICE";

        public NotificationMasterIPC() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

        public bool Installed => Utils.HasPlugin(Name);

        [EzIPC(NMAPINames.BringGameForeground, false)]
        private readonly Func<string, bool> BringGameForegroundIpc;

        [EzIPC(NMAPINames.FlashTaskbarIcon, false)]
        private readonly Func<string, bool> FlashTaskbarIconIpc;

        [EzIPC(NMAPINames.DisplayToastNotification, false)]
        private readonly Func<string, string, string, bool> DisplayToastNotificationIpc;

        [EzIPC(NMAPINames.IsGameWindowActivated, false)]
        private readonly Func<bool> IsGameWindowActivatedIpc;

        public bool BringGameForeground()
        {
            if (!Installed)
                return false;

            return BringGameForegroundIpc(RequesterName);
        }

        public bool FlashTaskbarIcon()
        {
            if (!Installed)
                return false;

            return FlashTaskbarIconIpc(RequesterName);
        }

        public bool DisplayToastNotification(string title, string text)
        {
            if (!Installed)
                return false;

            return DisplayToastNotificationIpc(RequesterName, title, text);
        }

        public bool IsGameWindowActivated()
        {
            if (!Installed)
                return false;

            return IsGameWindowActivatedIpc();
        }

        public void NotifyUser()
        {
            if (!Installed)
                return;

            if (C.Notification_Foreground && !IsGameWindowActivated())
                BringGameForeground();

            if (C.Notification_Toast)
                DisplayToastNotification("Cosmic Exploration", "I.C.E. has stopped running");

            if (C.Notification_FlashTaskbar)
                FlashTaskbarIcon();
        }
    }
}