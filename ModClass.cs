using GlobalEnums;
using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace OWO_HollowKnight
{
    public class OWO_HollowKnight : Mod
    {
        new public string GetName() => "OWO_HollowKnight";
        public override string GetVersion() => "v0.0.1";
        public OWOSkin owoSkin;
        internal static OWO_HollowKnight Instance;

        //--
        string playerMove = "";
        //--
        
        public bool IsSet()
        {
            return Instance != null;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = this;
            owoSkin = new OWOSkin();
            
            Log("Initialized");

            //Patch methods            
            On.HeroAudioController.PlaySound += OnHeroSounds;
            On.HeroController.DoDoubleJump += OnDoubleJump;
            ModHooks.AttackHook += OnAttack;            
        }

        private void PreFeel(string sensationName)
        {
            Log("Feel:" + sensationName);
            owoSkin.Feel(sensationName);
        }

        #region Hero
        private void OnAttack(AttackDirection direction)
        {
            Log($"Attack {direction.ToString()}");
        }

        private void OnDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);
            PreFeel("Double Jump");
        }

        private void OnHeroSounds(On.HeroAudioController.orig_PlaySound orig, HeroAudioController self, HeroSounds soundEffect)
        {
            orig(self, soundEffect);

            switch (soundEffect)
            {
                case HeroSounds.JUMP:
                    PreFeel("Jump");
                    break;
                case HeroSounds.DASH:
                    PreFeel("Dash");
                    break;
                case HeroSounds.SOFT_LANDING:
                    PreFeel("Soft Landing");
                    break;
                case HeroSounds.HARD_LANDING:
                    PreFeel("Hard Landing");
                    break;
                case HeroSounds.FALLING:
                    PreFeel("Falling");
                    break;
                case HeroSounds.WALLJUMP:
                    PreFeel("Wall Jump");
                    break;
                case HeroSounds.WALLSLIDE:
                    PreFeel("Wall Slide");
                    break;
                case HeroSounds.TAKE_HIT:
                    PreFeel("Take Hit");
                    break;
            }
        }

        #endregion
    }
}