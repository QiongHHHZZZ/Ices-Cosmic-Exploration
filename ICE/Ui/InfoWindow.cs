using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.Reflection;
using FFXIVClientStructs;
using ICE.Ui.MainUi.HelpFolder;
using ICE.Ui.MainUi.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ICE.Localization.L10n;

namespace ICE.Ui
{
    internal class InfoWindow : Window
    {
        public InfoWindow() : base(T("Ice's Cosmic Exploration - Info"))
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(300, 300),
                MaximumSize = new Vector2(4000, 4000),
            };

            P.windowSystem.AddWindow(this);
            AllowPinning = true;
            AllowClickthrough = true;
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        private static bool HasGatheringSetup = false;

        public override void Draw()
        {
ImGui.Text(T("Hi! Welcome to Ice's Cosmic Exploration [Short form, I.C.E.]"));
            ImGui.Bullet();
ImGui.TextWrapped(T("This plugin is meant to help you with your cosmic exploration needs, ") +
                              T("from automating the gathering and crafting process, to the buying of shop items or spending those planetary credits away."));

            helpSelect_Required.Draw();

            ImGui.Separator();

            ImGuiEx.IconWithText(FontAwesomeIcon.Feather, T("Gathering Setup"));
            ImGui.Dummy(new Vector2(0, 5));

ImGui.Text(T("If you would like to auto setup gathering to where all missions have their gathering buffs to what I would recommend"));

            if (!HasGatheringSetup)
            {
if (ImGui.Button(T("Setup Gathering Profiles")))
                {
                    foreach (var profile in C.GatherProfiles)
                    {
                        if (profile.Key == 0)
                            continue;
                        else
                        {
                            C.GatherProfiles.Remove(profile.Key);
                            foreach (var mission in C.MissionConfig)
                            {
                                if (mission.Value.GProfileId == profile.Key)
                                {
                                    mission.Value.GProfileId = 0; // fallback to default
                                }
                            }
                        }
                    }

                    string timedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IlRpbWVkIE1pc3Npb25zIiwiTWluaW11bUdwIjoxMDAsIkR1YWxDbGFzc0NyYWZ0QW1vdW50IjoxLCJHYXRoZXJCdWZmcyI6eyJCdWZmcyI6eyJCb29uSW5jcmVhc2UyIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9vbkluY3JlYXNlMSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
                    string limitedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkxpbWl0ZWQgTm9kZXMiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIlRpZGluZ3MiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyMDAsIk1heFVzZSI6LTF9LCJZaWVsZElJIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjo1MDAsIk1heFVzZSI6LTF9LCJZaWVsZEkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo0MDAsIk1heFVzZSI6LTF9LCJCb3VudGlmdWxZaWVsZElJIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjMwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Q2hhbmNlIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjowLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5VGVtcCI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9fSwiQm91bnRpZnVsTWluSXRlbSI6NH19";
                    string chainedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkNoYWluZWQiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb29uSW5jcmVhc2UxIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9LCJUaWRpbmdzIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjAwLCJNYXhVc2UiOi0xfSwiWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
                    string DualClass = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkR1YWwgQ2xhc3MiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjIsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
                    string boonMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkJvb24iLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIlRpZGluZ3MiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyMDAsIk1heFVzZSI6LTF9LCJZaWVsZElJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAwLCJNYXhVc2UiOi0xfSwiWWllbGRJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NDAwLCJNYXhVc2UiOi0xfSwiQm91bnRpZnVsWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjozMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eUNoYW5jZSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjI1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5VGVtcCI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfX0sIkJvdW50aWZ1bE1pbkl0ZW0iOjR9fQ==";
                    string ChainBoonMission = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkNoYWluZWQgXHUwMDJCIEJvb24iLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MDAsIk1heFVzZSI6LTF9LCJZaWVsZEkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo0MDAsIk1heFVzZSI6LTF9LCJCb3VudGlmdWxZaWVsZElJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjMwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Q2hhbmNlIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjowLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlUZW1wIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9fSwiQm91bnRpZnVsTWluSXRlbSI6NH19";
                    string GatherXAmount = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkdhdGhlciBYIEFtb3VudCIsIk1pbmltdW1HcCI6LTEsIkR1YWxDbGFzc0NyYWZ0QW1vdW50IjoxLCJHYXRoZXJCdWZmcyI6eyJCdWZmcyI6eyJCb29uSW5jcmVhc2UyIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9vbkluY3JlYXNlMSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";


                    GatherSettings.InitialSetupProfile(timedMissions, GatherSettings.MissionKinds.TimeAttack, out var _);
                    GatherSettings.InitialSetupProfile(limitedMissions, GatherSettings.MissionKinds.LimitedNodes, out var _);
                    GatherSettings.InitialSetupProfile(chainedMissions, GatherSettings.MissionKinds.Chain_Scoring, out var _);
                    GatherSettings.InitialSetupProfile(boonMissions, GatherSettings.MissionKinds.Boon_Scoring, out var _);
                    GatherSettings.InitialSetupProfile(ChainBoonMission, GatherSettings.MissionKinds.Chain_Boon, out var _);
                    GatherSettings.InitialSetupProfile(DualClass, GatherSettings.MissionKinds.DualClass, out var _);
                    GatherSettings.InitialSetupProfile(GatherXAmount, GatherSettings.MissionKinds.DualClass, out var _);

                    C.Save();
                }
            }
            else
            {
ImGui.Text(T("All gathering profile have been updated/automatically applied"));
            }
        }
    }
}
