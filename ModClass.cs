using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CutsceneHelper;
using UObject = UnityEngine.Object;

namespace OWO_HollowKnight
{
    public class OWO_HollowKnight : Mod
    {
        new public string GetName() => "OWO_HollowKnight";
        public override string GetVersion() => "v0.0.1";
        public OWOSkin owoSkin;
        internal static OWO_HollowKnight Instance;

        public bool isGamePaused = false;

        
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
            On.HeroController.StartMPDrain += OnStartMPDrain;
            On.HeroController.StopMPDrain += OnStopMPDrain;
            On.HeroController.Update += OnHeroUpdate;
            On.GameManager.EquipCharm += OnEquipCharm;
            On.GameManager.PauseGameToggle += OnGamePause;
            On.GameManager.OnApplicationQuit += OnApplicationQuit;
            On.GameManager.PlayerDead += OnPlayerDeath;
            On.GameManager.PlayerDeadFromHazard += OnPlayerDeathFromHazard;
            On.GameManager.FadeSceneIn += OnEnterHero; //FinishedEnteringScene por si este no sirve
            On.PlayerData.AddHealth += OnHeal;
            ModHooks.AttackHook += OnAttack;      
        }

        private void OnHeroUpdate(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);

            if (self.cState.onGround) 
            {                
                //owoSkin.StopFallingLoop();
            }

            if (!self.cState.wallSliding)
            {                
                //owoSkin.StopWallSlideLoop();
            }
        }

        private void OnStopMPDrain(On.HeroController.orig_StopMPDrain orig, HeroController self)
        {
            Log("Stop MP Drain");
        }

        private void OnStartMPDrain(On.HeroController.orig_StartMPDrain orig, HeroController self, float time)
        {
            Log("Start MP Drain");
        }

        private void OnHeal(On.PlayerData.orig_AddHealth orig, PlayerData self, int amount)
        {
            PreFeel("Heal");
            orig(self, amount);
        }

        private void OnEnterHero(On.GameManager.orig_FadeSceneIn orig, GameManager self)
        {
            Log("OnEnterHero");
            orig(self);
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

        private IEnumerator OnPlayerDeath(On.GameManager.orig_PlayerDead orig, GameManager self, float waitTime)
        {
            owoSkin.StopAllHapticFeedback();
            PreFeel("Death");

            yield return orig(self, waitTime);
        }

        private IEnumerator OnPlayerDeathFromHazard(On.GameManager.orig_PlayerDeadFromHazard orig, GameManager self, float waitTime)
        {
            owoSkin.StopAllHapticFeedback();
            PreFeel("Death");

            yield return orig(self, waitTime);
        }

        #endregion

        #region Game

        private IEnumerator OnGamePause(On.GameManager.orig_PauseGameToggle orig, GameManager self)
        {
            isGamePaused = !isGamePaused;

            Log("GameIsPaused: " + isGamePaused);

            yield return orig(self);
        }

        private void OnEquipCharm(On.GameManager.orig_EquipCharm orig, GameManager self, int charmNum)
        {
            orig(self, charmNum);
            PreFeel("Equip Charm");
        }

        private void OnApplicationQuit(On.GameManager.orig_OnApplicationQuit orig, GameManager self)
        {
            owoSkin.StopAllHapticFeedback();
            orig(self);
        }

        #endregion
    }
}