using BepInEx.Configuration;
using EmotesAPI;
using RiskOfOptions.Lib;
using RiskOfOptions.Options;
using RiskOfOptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GordonFreeman
{
    public class ModCompatabilities
    {
        public class EmoteCompatability
        {
            public const string GUID = "com.weliveinasociety.CustomEmotesAPI";
            public static void Init()
            {
                CustomEmotesAPI.ImportArmature(Assets.ProfessionalBody, Assets.EmotePrefab);
                CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
            }
            private static void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
            {
                if (mapper.name == "HeroEmotes")
                {
                    ProfessionalBodyComponent professionalBodyComponent = mapper.mapperBody && mapper.mapperBody as ProfessionalBodyComponent ? mapper.mapperBody as ProfessionalBodyComponent : null;
                    if (newAnimation == "none")
                    {
                        if (professionalBodyComponent != null) professionalBodyComponent.professionalCharacterComponent.firstPersonCamera.enabled = true;
                    }
                    else
                    {
                        if (professionalBodyComponent != null) professionalBodyComponent.professionalCharacterComponent.firstPersonCamera.enabled = false;
                    }

                }
            }
        }
        public class RiskOfOptionsCompatability
        {
            public const string GUID = "com.rune580.riskofoptions";
            public static void AddConfig<T>(T config) where T : ConfigEntryBase
            {
                if (config is ConfigEntry<float>)
                {
                    ModSettingsManager.AddOption(new FloatFieldOption(config as ConfigEntry<float>));
                }
                if (config is ConfigEntry<bool>)
                {
                    ModSettingsManager.AddOption(new CheckBoxOption(config as ConfigEntry<bool>));
                }
                if (config is ConfigEntry<int>)
                {
                    ModSettingsManager.AddOption(new IntFieldOption(config as ConfigEntry<int>));
                }
                if (config is ConfigEntry<string>)
                {
                    ModSettingsManager.AddOption(new StringInputFieldOption(config as ConfigEntry<string>));
                }
            }
        }
    }
    
}
