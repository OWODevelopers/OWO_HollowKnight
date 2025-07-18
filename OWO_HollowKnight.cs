﻿using GlobalEnums;
using Modding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OWO_HollowKnight
{
    public class OWO_HollowKnight : Mod
    {
        new public string GetName() => "OWO_HollowKnight";
        public override string GetVersion() => "v0.0.1";
        public OWOSkin owoSkin;
        internal static OWO_HollowKnight Instance;        

        
        public bool IsSet()
        {
            return Instance != null;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
            owoSkin = new OWOSkin();

            //Patch methods            
            On.HeroAudioController.PlaySound += OnHeroSounds;
            On.HeroController.DoDoubleJump += OnDoubleJump;
            On.HeroController.StartMPDrain += OnStartMPDrain;
            On.HeroController.StopMPDrain += OnStopMPDrain;
            On.HeroController.StartCyclone += OnStartCyclone;
            On.HeroController.EndCyclone += OnStopCyclone;
            On.HeroController.Update += OnHeroUpdate;
            On.GameManager.EquipCharm += OnEquipCharm;
            On.GameManager.PauseGameToggle += OnGamePause;
            On.GameManager.OnApplicationQuit += OnApplicationQuit;
            On.GameManager.PlayerDead += OnPlayerDeath;
            On.GameManager.PlayerDeadFromHazard += OnPlayerDeathFromHazard;
            On.GameManager.FadeSceneIn += OnEnterHero;
            On.PlayerData.AddHealth += OnHealth;            
            ModHooks.AttackHook += OnAttack;      
        }

        private void OnHealth(On.PlayerData.orig_AddHealth orig, PlayerData self, int amount)
        {
            owoSkin.Feel("Heal",1);
            orig(self, amount);
        }

        private void OnHeroUpdate(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            string statusLog;

            if (self.cState.onGround) 
            {                
               owoSkin.StopFalling();
            }

            if (!self.cState.wallSliding)
            {                
                owoSkin.StopSliding();
            }

            if (self.cState.superDashing)
            {
                owoSkin.StartSuperDash();
            }
            else 
            {
                owoSkin.StopSuperDash();
            }
        }

        private void OnEnterHero(On.GameManager.orig_FadeSceneIn orig, GameManager self)
        {
            owoSkin.StopAllHapticFeedback();
            orig(self);
        }

        #region Hero
        private void OnAttack(AttackDirection direction)
        {
            owoSkin.Feel("Attack", 2);
        }

        private void OnDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);
            owoSkin.Feel("Double Jump", 2);
        }

        private void OnHeroSounds(On.HeroAudioController.orig_PlaySound orig, HeroAudioController self, HeroSounds soundEffect)
        {
            orig(self, soundEffect);

            switch (soundEffect)
            {
                case HeroSounds.JUMP:
                    owoSkin.Feel("Jump", 2);
                    break;
                case HeroSounds.DASH:
                    owoSkin.Feel("Dash", 2);
                    break;
                case HeroSounds.HARD_LANDING:
                    HardLanding();
                    break;
                case HeroSounds.FALLING:
                    owoSkin.StartFalling();
                    break;
                case HeroSounds.WALLJUMP:
                    WallJump();
                    break;
                case HeroSounds.WALLSLIDE:
                    owoSkin.StarSliding();
                    break;
                case HeroSounds.TAKE_HIT:
                    owoSkin.Feel("Hurt", 3);
                    break;
            }
        }

        private void HardLanding()
        {
            owoSkin.StopFalling();
            owoSkin.Feel("Hard Landing", 2);
        }

        private void WallJump()
        {
            owoSkin.Feel("Jump", 2);
        }

        private IEnumerator OnPlayerDeath(On.GameManager.orig_PlayerDead orig, GameManager self, float waitTime)
        {
            owoSkin.StopAllHapticFeedback();
            owoSkin.Feel("Death", 4);

            yield return orig(self, waitTime);
        }

        private IEnumerator OnPlayerDeathFromHazard(On.GameManager.orig_PlayerDeadFromHazard orig, GameManager self, float waitTime)
        {
            owoSkin.StopAllHapticFeedback();
            owoSkin.Feel("Hazard Death", 4);

            yield return orig(self, waitTime);
        }

        private void OnStopCyclone(On.HeroController.orig_EndCyclone orig, HeroController self)
        {
            owoSkin.StopCyclone();
            orig(self);
        }

        private void OnStartCyclone(On.HeroController.orig_StartCyclone orig, HeroController self)
        {
            owoSkin.StartCyclone();
            orig(self);
        }

        private void OnStopMPDrain(On.HeroController.orig_StopMPDrain orig, HeroController self)
        {
            owoSkin.StopCharging();
            orig(self);
        }

        private void OnStartMPDrain(On.HeroController.orig_StartMPDrain orig, HeroController self, float time)
        {
            owoSkin.StartCharging();
            orig(self, time);
        }

        #endregion

        #region Game

        private IEnumerator OnGamePause(On.GameManager.orig_PauseGameToggle orig, GameManager self)
        {
            owoSkin.isGameUnpaused = !owoSkin.isGameUnpaused;

            yield return orig(self);
        }

        private void OnEquipCharm(On.GameManager.orig_EquipCharm orig, GameManager self, int charmNum)
        {
            orig(self, charmNum);
            owoSkin.Feel("Charm Equip", 1);
        }

        private void OnApplicationQuit(On.GameManager.orig_OnApplicationQuit orig, GameManager self)
        {
            owoSkin.StopAllHapticFeedback();
            orig(self);
        }

        #endregion
    }
}