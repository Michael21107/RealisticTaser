using Rage;
using System.Windows.Forms;

namespace RealisticTaser
{
    internal static class Config
    {
        //finish ini
        public static readonly InitializationFile INIFile = new InitializationFile(@"Plugins\LSPDFR\RealisticTaser.ini");

        public static readonly int TaserSuccess = INIFile.ReadInt16("Main", "Taser Success Probability", 69); //default 69?
        public static readonly bool TaserSuccessRange = INIFile.ReadBoolean("Main", "Taser Success Based on Range", true);
        public static readonly int ScaleFactor = INIFile.ReadInt16("Main", "Scale Factor", 3); //default 3

        public static readonly bool LimitShots = INIFile.ReadBoolean("Reloads", "Limit Shots", true);
        public static readonly int Shots = INIFile.ReadInt16("Reloads", "Shot Count", 2); //default 2
        public static readonly bool DoReloads = INIFile.ReadBoolean("Reloads", "Do Reload Animations", true);
        public static readonly bool ReplenishShots = INIFile.ReadBoolean("Reloads", "Replenish Taser in Vehicle", true);

        public static readonly int ShotCountSize = INIFile.ReadInt16("UI", "Shot Count UI Size", 40);
        public static readonly int ShotCountX = INIFile.ReadInt16("UI", "Shot Count UI x-Position", 2500); // UI only enabled if shots are limited
        public static readonly int ShotCountY = INIFile.ReadInt16("UI", "Shot Count UI y-Position", 57);

        public static readonly Keys TaserDeployKey = INIFile.ReadEnum<Keys>("Misc", "Taser Deploy Key", Keys.LButton);
        public static readonly bool LogDebugMessages = INIFile.ReadBoolean("Misc", "Log Debug Messages", false); //don't forget to change this to false!
    }
}
