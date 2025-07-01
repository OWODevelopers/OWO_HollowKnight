using Modding;
using OWOGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace OWO_HollowKnight
{
    public class OWOSkin : Loggable
    {
        public bool suitEnabled = false;
        public bool isPlayerActive = false;
        private bool ultraSpeedIsEnable = false;
        private bool nailGunIsEnable = false;
        private bool chargingIsEnable = false;
        private float ultraAngle = 0f;
        private int ultraIntensity = 0;
        public bool isRightHanded;
        public bool dualWeapon;
        public bool reverseDash;


        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();
        private readonly Muscle[] rightRecoilMuscles = { Muscle.Arm_R, Muscle.Pectoral_R.WithIntensity(80), Muscle.Dorsal_R.WithIntensity(50) };
        private readonly Muscle[] leftRecoilMuscles = { Muscle.Arm_L, Muscle.Pectoral_L.WithIntensity(80), Muscle.Dorsal_L.WithIntensity(50) };
        private readonly Muscle[] rightSpeedMuscles = { Muscle.Arm_R, Muscle.Pectoral_R, Muscle.Abdominal_R, Muscle.Dorsal_R, Muscle.Lumbar_R };
        private readonly Muscle[] leftSpeedMuscles = { Muscle.Arm_L, Muscle.Pectoral_L, Muscle.Abdominal_L, Muscle.Dorsal_L, Muscle.Lumbar_L };

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        #region Skin Configuration

        private void RegisterAllSensationsFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.Message); }

            }
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("90027016");

            OWO.Configure(gameAuth);
            string[] myIPs = GetIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == OWOGame.ConnectionState.Connected)
            {
                suitEnabled = true;
                LOG("OWO suit connected.");
                Feel("Loading Up", 1);
            }
            if (!suitEnabled) LOG("OWO is not enabled?!?!");
        }

        public BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }

        public string[] GetIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\BepinEx\\Plugins\\OWO" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    if (IPAddress.TryParse(line, out _)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }

        ~OWOSkin()
        {
            LOG("Destructor called");
            DisconnectOWO();
        }

        public void DisconnectOWO()
        {
            LOG("Disconnecting OWO skin.");
            OWO.Disconnect();
        }
        #endregion

        public void LOG(String msg)
        {
            Log(msg);
        }

        #region Feel

        public void Feel(String key, int Priority = 0, int intensity = 0)
        {
            Sensation toSend = GetBackedId(key);
            if (toSend == null) return;

            if (intensity != 0)
            {
                toSend = toSend.WithMuscles(Muscle.All.WithIntensity(intensity));
            }

            OWO.Send(toSend.WithPriority(Priority));
        }

        public void FeelWithHand(String key, bool isRightHand = true, int Priority = 0, int intensity = 100)
        {
            Sensation toSend = GetBackedId(key);
            if (toSend == null) return;

            if (key != "Punch" && dualWeapon)
            {
                Muscle[] dualMuscles = rightRecoilMuscles.Concat(leftRecoilMuscles).ToArray();
                toSend = toSend.WithMuscles(dualMuscles.WithIntensity(intensity));
            }
            else
            {
                toSend = toSend.WithMuscles(isRightHand ? rightRecoilMuscles.WithIntensity(intensity) : leftRecoilMuscles.WithIntensity(intensity));
            }

            OWO.Send(toSend.WithPriority(Priority));
        }

        public void FeelSpeed()
        {
            if (ultraIntensity < 17) return;


            Sensation toSend = GetBackedId("Ultra Speed");
            if (toSend == null) return;
            //LOG($"### SpeedAngle: {ultraAngle}");

            Muscle[] musclesList = GetMuscleAngle(ultraAngle, reverseDash);

            toSend = toSend.WithMuscles(musclesList.WithIntensity(Mathf.Clamp(ultraIntensity + 10, 20, 100)));

            OWO.Send(toSend.WithPriority(0));
        }

        //public void FeelDamage(Vector3 hitForward)
        //{
        //    Sensation toSend = GetBackedId("Hurt");
        //    if (toSend == null) return;

        //    Transform player = MonoSingleton<NewMovement>.Instance.transform;

        //    float angleHit = Vector3.SignedAngle(-hitForward.normalized, player.forward, Vector3.up) + 180;

        //    Muscle[] musclesList = GetMuscleAngle(angleHit);


        //    toSend = toSend.WithMuscles(musclesList.WithIntensity(100));

        //    OWO.Send(toSend.WithPriority(3));
        //}

        private Muscle[] GetMuscleAngle(float angle, bool reveseMuscle = false)
        {
            switch (angle)
            {
                case float a when (a > 135 && a <= 225):
                    return reveseMuscle ? Muscle.Back : Muscle.Front; //Default Front
                case float a when (a > 45 && a <= 135):
                    return reveseMuscle ? leftSpeedMuscles : rightSpeedMuscles; //Default Right
                case float a when ((a >= 0 && a <= 45) || (a > 315 && a <= 360)):
                    return reveseMuscle ? Muscle.Front : Muscle.Back; //Default Back
                case float a when (a > 225 && a <= 315):
                    return reveseMuscle ? rightSpeedMuscles : leftSpeedMuscles; //Default Left
                default:
                    return Muscle.Back;
            }
        }

        private Sensation GetBackedId(string sensationKey)
        {
            if (FeedbackMap.ContainsKey(sensationKey))
            {
                return FeedbackMap[sensationKey];
            }
            else
            {
                LOG($"Feedback not registered: {sensationKey}");
                return null;
            }
        }

        #endregion

        #region Loops

        #region UltraSpeed
        public void StartUltraSpeed()
        {
            if (ultraSpeedIsEnable) return;

            ultraSpeedIsEnable = true;
            UltraSpeedFuncAsync();
        }

        public void StopUltraSpeed()
        {
            ultraSpeedIsEnable = false;
        }

        public async Task UltraSpeedFuncAsync()
        {
            while (ultraSpeedIsEnable)
            {
                FeelSpeed();
                await Task.Delay(200);
            }
        }
        public void UpdateUltraSpeed(float angle, int intensity)
        {
            ultraAngle = angle;
            ultraIntensity = intensity;
        }

        #endregion

        #region NailGun
        public void StartNailGun()
        {
            if (nailGunIsEnable) return;

            nailGunIsEnable = true;
            NailGunFuncAsync();
        }

        public void StopNailGun()
        {
            nailGunIsEnable = false;
        }

        public async Task NailGunFuncAsync()
        {
            while (nailGunIsEnable)
            {
                GunRecoil("Nailgun");
                await Task.Delay(200);
                StopNailGun();
            }
        }

        #endregion

        #region Charging
        public void StartCharging()
        {
            if (chargingIsEnable) return;

            chargingIsEnable = true;
            ChargingFuncAsync();
        }

        public void StopCharging()
        {
            chargingIsEnable = false;
        }

        public async Task ChargingFuncAsync()
        {
            while (chargingIsEnable)
            {
                GunRecoil("Charging");
                await Task.Delay(200);
                StopCharging();
            }
        }

        #endregion

        #endregion

        #region guns 

        public void GunRecoil(String recoilSensation)
        {
            FeelWithHand(recoilSensation, isRightHanded, 2);
        }

        public void PunchRecoil()
        {
            FeelWithHand("Punch", !isRightHanded, 2);
        }

        #endregion

        public void StopAllHapticFeedback()
        {
            StopUltraSpeed();

            OWO.Stop();
        }

        public bool CanFeel()
        {
            return suitEnabled && isPlayerActive;
        }
    }
}
