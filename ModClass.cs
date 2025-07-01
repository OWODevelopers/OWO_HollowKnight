using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace OWO_HollowKnight
{
    public class OWO_HollowKnight : Mod
    {
        internal static OWO_HollowKnight Instance;
        public override string GetVersion() => "v0.0.1";
        public OWOSkin owoSkin;

        //public override List<ValueTuple<string, string>> GetPreloadNames()
        //{
        //    return new List<ValueTuple<string, string>>
        //    {
        //        new ValueTuple<string, string>("White_Palace_18", "White Palace Fly")
        //    };
        //}

        //public OWO_HollowKnight() : base("OWO_HollowKnight")
        //{
        //    Instance = this;
        //}

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            owoSkin = new OWOSkin();

            Instance = this;

            owoSkin.LOG("OwoSkin Initialized");
        }
    }
}