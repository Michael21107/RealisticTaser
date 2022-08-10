using System;
using System.Net;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;


namespace RealisticTaser
{
    public class EntryPoint : Plugin
    {
        //UPDATE CHECKER
        public static Version NewVersion = new Version();
        public static Version curVersion = new Version(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());  //DON'T FORGET TO CHANGE THIS, MATTHEW!!!

        public static bool UpToDate;
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("REALISTICTASER: RealisticTaser " + curVersion + " by YobB1n has been loaded.");   //Returns Version          
        }
        public override void Finally()
        {
            Game.LogTrivial("REALISTICTASER: RealisticTaser has been cleaned up.");
        }
        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            try
            {
                Thread FetchVersionThread = new Thread(() =>
                {
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            string s = client.DownloadString("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=39847&textOnly=1");
                            NewVersion = new Version(s);
                        }
                        catch (Exception) { Game.LogTrivial("REALISTICTASER: Cannot Connect to Plugin Info Page. Aborting Update Checks."); }
                    }
                });
                FetchVersionThread.Start();
                while (FetchVersionThread.ThreadState != System.Threading.ThreadState.Stopped)  //if we have a thread to check the update. Otherwise go straight to catch blocks
                {
                    GameFiber.Yield();
                }
                // compare the versions  
                if (curVersion.CompareTo(NewVersion) < 0)
                {
                    Game.LogTrivial("REALISTICTASER: Update Available for Realistic Taser. Installed Version " + curVersion + "New Version " + NewVersion);
                    Game.DisplayNotification("It is ~y~Strongly Recommended~w~ to~g~ Update~b~ Realistic Taser. ~w~Playing on an Old Version ~r~May Cause Issues!");
                    UpToDate = false;
                }
                else if (curVersion.CompareTo(NewVersion) > 0)
                {
                    Game.LogTrivial("YOBBINCALLOUTS: DETECTED BETA RELEASE. DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                    Game.DisplayNotification("YOBBINCALLOUTS: ~r~DETECTED BETA RELEASE. ~w~DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                    UpToDate = true;
                }
                else
                {
                    Game.DisplayNotification("You are on the ~g~Latest Version~w~ of ~b~Realistic Taser.");
                    UpToDate = true;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                Game.LogTrivial("REALISTICTASER: Error while checking Realistic Taser for updates. System.ThreadAbortException.");
            }
            catch (Exception)
            {
                Game.LogTrivial("REALISTICTASER: Error while checking Realistic Taser for updates. Some Other Exception.");
            }
            Game.LogTrivial("==========REALISTICTASER INFORMATION==========");
            Game.LogTrivial("Realistic Taser by YobB1n");
            Game.LogTrivial("Version " + curVersion);
            if (UpToDate) Game.LogTrivial("Realistic Taser is Up-To-Date.");
            else Game.LogTrivial("Realistic Taser is NOT Up-To-Date.");
            if (Config.INIFile.Exists()) Game.LogTrivial("Realistic Taser Config is Installed by User.");
            else Game.LogTrivial("Realistic Taser Config is NOT Installed by User.");
            if (Config.TaserSuccessRange)
            {
                Game.LogTrivial("TaserSuccess range is set to: " + Config.TaserSuccessRange);
                Game.LogTrivial("Scale Factor for TaserSuccess range is set to: " + Config.ScaleFactor);
            }
            else Game.LogTrivial("TaserSuccess probability is set to: " + Config.TaserSuccess);
            Game.LogTrivial("Taser Shot Limiting is set to: " + Config.LimitShots);
            if (Config.LimitShots) Game.LogTrivial("Taser Shot Limit is set to: " + Config.Shots);
            Game.LogTrivial("Reload Animations are Set to: " + Config.DoReloads);
            Game.LogTrivial("Vehicle Reloading is Set to: " + Config.ReplenishShots);
            Game.LogTrivial("Debug Logging is set to: " + Config.LogDebugMessages);
            Game.LogTrivial("Taser Deploy Key is set to: " + Config.TaserDeployKey);
            if (Config.LimitShots) Game.LogTrivial("Taser Shot Count UI is set to: " + Config.ShotCountX + ", " + Config.ShotCountY + " with size " + Config.ShotCountSize);
            Game.LogTrivial("Please Join My Discord Server to Report Bugs/Improvements: https://discord.gg/Wj522qa5mT. Enjoy!");
            Game.LogTrivial("==========REALISTICTASER INFORMATION==========");
            int num = (int)Game.DisplayNotification("mpweaponsgang0", "w_pi_stungun", "Realistic Taser", "~y~v." + curVersion + " ~b~by YobB1n", " ~g~Loaded Successfully. ~b~Enjoy!");

            MainThread.Main();
            if (Config.LimitShots) MainThread.ShotCounter(); //shot counter thread only runs if shots are limited
        }
    }
}
