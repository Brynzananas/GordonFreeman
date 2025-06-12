using EmotesAPI;
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
    }
    
}
