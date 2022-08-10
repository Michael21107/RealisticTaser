using System;
using System.Linq;  //for checking strings
using Rage;
using System.Drawing;

namespace RealisticTaser
{
    class MainThread
    {
        public static Ped player = Game.LocalPlayer.Character;
        public static Entity Suspect;
        private static int FinalTaserSuccess;
        public static int ShotCount = 0;
        public static bool DisplayAmmoCount;

        public static void Main()
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        GameFiber.Yield();

                        while (player.Exists()) //this is to sleep the plugin when the player does not using any weapon. This is also a workaround to a crash when LSPDFR reloads.
                        {
                            GameFiber.Yield();
                            player = Game.LocalPlayer.Character; //test this
                            if (player.Exists())
                            {
                                if (player.IsAiming || player.IsShooting)
                                {
                                    break;
                                }
                            }
                        }

                        if (player.Exists()) //this is to prevent the plugin from crashing when lspdfr reloads and resets the ped and its inventory.
                        {
                            if (player.Inventory.EquippedWeapon != null)
                            {
                                if (player.Inventory.EquippedWeapon.Hash == WeaponHash.StunGun)
                                {
                                    Suspect = Game.LocalPlayer.GetFreeAimingTarget(); //gets the closest target the player is aiming at
                                    if (Suspect.Exists() && Suspect.IsValid())
                                    {
                                        if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: SUSPECT VALID");
                                        System.Random rando = new System.Random();

                                        if (Config.TaserSuccessRange) //Taser Success based on Range Enabled
                                        {
                                            float distance = player.DistanceTo(Suspect);
                                            FinalTaserSuccess = (int)(Config.ScaleFactor * (60.7539 / Math.Pow(distance, 0.824732)) - 12.821); //taser success rate based on npr article (in metres)
                                            if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER SUCCESS PROBABILITY: " + FinalTaserSuccess);

                                            int TaserSuccess = rando.Next(Config.ScaleFactor * 5, 101); //remove the bottom Scale Factor * 5 percent chance of success
                                            if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER SUCCESS PROBABILITY MUST BE HIGHER THAN: " + TaserSuccess);

                                            if (FinalTaserSuccess > TaserSuccess)
                                            {
                                                if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER SUCCESS");
                                                if (Suspect.Exists()) Suspect.IsInvincible = false;
                                                if (player.Exists()) while (player.IsWeaponReadyToShoot) GameFiber.Wait(0);
                                            }
                                            else
                                            {
                                                if (Suspect.Exists()) Suspect.IsInvincible = true;
                                                if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER NOT SUCCESS");
                                                try
                                                {
                                                    if (player.Exists()) while (Game.LocalPlayer.Character.IsWeaponReadyToShoot) GameFiber.Wait(0);
                                                }
                                                catch (Rage.Exceptions.InvalidHandleableException)
                                                {
                                                    Game.LogTrivial("REALISTICTASER - KNOWN ISSUE / INVALID HANDABLE LINE 72 CAUGHT / CAUSED BY EUP CHARACTER SWITCH. PLEASE RELOAD LSPDFR TO FIX IT.");
                                                }
                                                if (Suspect.Exists()) Suspect.IsInvincible = false;
                                            }
                                        }
                                        else //Taser Success based on Range Disabled
                                        {
                                            int TaserSuccess = rando.Next(0, 101);
                                            if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER SUCCESS PROBABILITY: " + TaserSuccess);

                                            if (TaserSuccess < Config.TaserSuccess)
                                            {
                                                if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER SUCCESS");
                                                if (Suspect.Exists()) Suspect.IsInvincible = false;
                                                if (player.Exists()) while (player.IsWeaponReadyToShoot) GameFiber.Wait(0);
                                            }
                                            else
                                            {
                                                if (Suspect.Exists()) Suspect.IsInvincible = true;
                                                if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER NOT SUCCESS");
                                                if (player.Exists()) while (player.IsWeaponReadyToShoot) GameFiber.Wait(0);
                                                if (Suspect.Exists()) Suspect.IsInvincible = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //end of loop
                    }
                }
                catch (System.Threading.ThreadAbortException) { }
                catch (Exception e)
                {

                    Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
                    string error = e.ToString();
                    Game.LogTrivial("ERROR: " + error);
                    Game.LogTrivial("IN - REALISTICTASER MAIN THREAD");
                    Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~RealisticTaser. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                    //Game.DisplayNotification("Error: ~r~" + error);
                    Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                    Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
                }
            });
        }

        public static void ShotCounter()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("REALISTICTASER: SHOTCOUNTER STARTED");
                try
                {
                    while (player.Exists())
                    {
                        GameFiber.Yield();
                        DisplayAmmoCount = false; //always shut off taser UI unless player is aiming with taser in hand

                        if (player.Exists())
                        {
                            if (player.IsInAnyPoliceVehicle && Config.ReplenishShots)
                            {
                                Game.LogTrivial("REALISTICTASER: Player entered Police Vehicle. Reloading...");
                                ShotCount = 0; //replenish taser ammo if in vehicle
                                //some audio? or notification?
                                //
                            }
                        }

                        if (player.Exists() && player.IsAiming)
                        {
                            if (player.Inventory.EquippedWeapon != null)
                            {
                                if (player.Inventory.EquippedWeapon.Hash == WeaponHash.StunGun)
                                {
                                    DisplayAmmoCount = true; //player aiming, enable ammo UI
                                    if (Config.LimitShots) Game.FrameRender += DrawAmmoCount; //call the ammo UI with FrameRender (moved)
                                    while (player.IsAiming)
                                    {
                                        //if (Config.LimitShots) Game.FrameRender += DrawAmmoCount; //call the ammo UI with FrameRender
                                        GameFiber.Yield();
                                        if (ShotCount >= Config.Shots) //Taser out of Ammo
                                        {
                                            if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER OUT OF AMMO");
                                            if (Game.IsKeyDown(Config.TaserDeployKey)) PlaySound(); //Taser Dry Fire audio if mouse1/main weapon key depressed
                                            if (player.Exists()) Rage.Native.NativeFunction.Natives.DISABLE_PLAYER_FIRING(player, true); //prevent the taser from firing
                                        }
                                        if (player.Exists() && player.IsShooting)
                                        {
                                            if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER VALID");

                                            if (ShotCount >= Config.Shots) //Taser out of ammo
                                            {
                                                if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: TASER OUT OF AMMO");
                                                if (player.Exists()) Rage.Native.NativeFunction.Natives.DISABLE_PLAYER_FIRING(player, true);
                                            }
                                            else
                                            {
                                                ShotCount++; //Increase the amount of shots on this Taser
                                                if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: Shot Count on this Taser is " + ShotCount);
                                                if (Config.DoReloads && player.Exists())
                                                {
                                                    if (ShotCount < Config.Shots) //If there are more shots left in the taser, reload animation
                                                    {
                                                        player.Tasks.PlayAnimation("anim@weapons@pistol@flare_str", "reload_aim", 0.8f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
                                                    }
                                                }
                                            }
                                            if (player.Exists()) Rage.Native.NativeFunction.Natives.DISABLE_PLAYER_FIRING(player, false); //Allow player to fire again
                                            break; //Break back into loop
                                        }
                                        if (!player.IsAiming) //set player firing back after shot is fired
                                        {
                                            if (player.Exists()) Rage.Native.NativeFunction.Natives.DISABLE_PLAYER_FIRING(player, false);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (System.Threading.ThreadAbortException) { }
                catch (Exception e)
                {
                    Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
                    string error = e.ToString();
                    Game.LogTrivial("ERROR: " + error);
                    Game.LogTrivial("IN - REALISTICTASER SHOT COUNTER");
                    Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~RealisticTaser. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                    //Game.DisplayNotification("Error: ~r~" + error);
                    Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                    Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
                }
            });
        }

        public static void PlaySound()
        {
            try
            {
                if (Config.LogDebugMessages) Game.LogTrivial("REALISTICTASER: SOUND PLAY");
                System.Media.SoundPlayer sound = new System.Media.SoundPlayer();
                sound.SoundLocation = @"lspdfr\audio\sfx\GTA5_SHT_DRYFIRE_COPY.wav";
                GameFiber.StartNew(delegate
                {
                    sound.Load();
                    sound.Play();
                    GameFiber.Wait(300);
                    sound.Stop();
                });
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (System.IO.FileNotFoundException)
            {
                Game.DisplayNotification("The ~b~Audio File~w~ for ~g~RealisticTaser~w~ is ~r~not Installed Properly.~w~ Please ~b~Reinstall~w~ the Plugin Properly.");
                Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
                Game.LogTrivial("AUDIO FILE FOR REALISTICTASER NOT INSTALLED. PLEASE REINSTALL THE PLUGIN PROPERLY.");
                Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
            }
            catch (Exception e)
            {
                Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
                string error = e.ToString();
                Game.LogTrivial("ERROR: " + error);
                Game.LogTrivial("IN - REALISTICTASER SOUND PLAYER");
                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~RealisticTaser. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                //Game.DisplayNotification("Error: ~r~" + error);
                Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
            }
        }

        public static void DrawAmmoCount(System.Object sender, Rage.GraphicsEventArgs e)
        {
            try
            {
                if (DisplayAmmoCount)
                {
                    if (ShotCount < Config.Shots) e.Graphics.DrawText("" + (Config.Shots - ShotCount) + "", "pricedown", Config.ShotCountSize, new PointF(Config.ShotCountX, Config.ShotCountY), System.Drawing.Color.White);
                    else e.Graphics.DrawText("" + (Config.Shots - ShotCount) + "", "pricedown", Config.ShotCountSize, new PointF(Config.ShotCountX, Config.ShotCountY), System.Drawing.Color.Crimson);
                }
                else
                {
                    Game.FrameRender -= DrawAmmoCount;
                }
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception f)
            {
                Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
                string error = f.ToString();
                Game.LogTrivial("ERROR: " + error);
                Game.LogTrivial("IN - REALISTICTASER AMMO COUNT UI DRAWER");
                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~RealisticTaser. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                //Game.DisplayNotification("Error: ~r~" + error);
                Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                Game.LogTrivial("==========REALISTICTASER: ERROR CAUGHT==========");
            }
        }
    }
}
