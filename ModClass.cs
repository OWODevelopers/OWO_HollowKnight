using Modding;
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
            
            //ModHooks.HeroUpdateHook += OnHeroUpdate; 
        }

        private void OnHeroUpdate()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            { 
                LogWarn("PRESSING SPACE");
            }
        }
    }
}