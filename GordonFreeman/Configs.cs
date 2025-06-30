using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GordonFreeman
{
    public static class Configs
    {
        public static ConfigEntry<string> MusicPicker;
        public static void AddConfigToRiskOfOptions<T>(this ConfigEntry<T> configEntry)
        {
            if (!Main.riskOfOptionsEnabled) return;
            ModCompatabilities.RiskOfOptionsCompatability.AddConfig(configEntry);
        }
    }
}
