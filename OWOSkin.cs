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
        private string owoPath = "\\hollow_knight_Data\\Managed\\Mods\\OWO_HollowKnight\\OWO";
        public bool suitEnabled = false;
        public bool isPlayerActive = false;

        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        #region Skin Configuration

        private void RegisterAllSensationsFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + owoPath;
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
                catch (Exception e)
                { 
                    LOG(e.Message);
                }

            }
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");
            
            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("90027016");
            LOG("Auth generated");

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
            if (!suitEnabled)
                LOG("OWO is not enabled?!?!");
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
            string filePath = Directory.GetCurrentDirectory() + owoPath + filename;
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
            DisconnectOWO();
        }

        public void DisconnectOWO()
        {
            OWO.Disconnect();
        }
        #endregion

        public void LOG(String msg)
        {
            OWO_HollowKnight.Instance.Log($"{msg}");
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

        public void StopAllHapticFeedback()
        {
            OWO.Stop();
        }

        public bool CanFeel()
        {
            return suitEnabled && isPlayerActive;
        }
    }
}
