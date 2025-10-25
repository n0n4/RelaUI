using Newtonsoft.Json;
using RelaUI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace RelaUI.Profiles
{
    public class ProfileManager<T>
    {
        public string FolderPath;

        private string ConfigFileName = "config.json";
        private ProfileConfig Config;
        public Func<Profile<T>> DefaultProfileFactory;
        public Action<string> ErrorLogCallback;

        public Dictionary<string, Profile<T>> Profiles = new Dictionary<string, Profile<T>>();
        public Profile<T> CurrentProfile;

        public ProfileManager(string folderPath, Func<Profile<T>> defaultProfileFactory,
            Action<string> errorLogCallback)
        {
            FolderPath = folderPath;
            DefaultProfileFactory = defaultProfileFactory;
            ErrorLogCallback = errorLogCallback;

            // load the profile config, if it exists
            // otherwise, create it
            string conpath = Path.Combine(FolderPath, ConfigFileName);
            if (!File.Exists(conpath))
            {
                Config = new ProfileConfig();
                SaveProfileConfig(conpath);
            }
            else
            {
                LoadProfileConfig(conpath);
            }

            // if the default profile doesn't exist, create it with the factory
            string defpath = Path.Combine(FolderPath, "profile_" + Config.DefaultProfileName + ".json");
            if (!File.Exists(defpath))
            {
                Profile<T> prof = DefaultProfileFactory();
                prof.SaveName = Config.DefaultProfileName;
                SaveProfile(prof, defpath);
            }

            // load all profiles in the folder
            IEnumerable<string> files = Directory.EnumerateFiles(FolderPath, "profile_*.json");
            foreach(string fname in files)
            {
                try
                {
                    Profile<T> prof = LoadProfile(fname);
                    Profiles.Add(prof.SaveName, prof);
                }
                catch
                {
                    ErrorLogCallback("Couldn't load profile '" + fname + "'");
                }
            }

            CurrentProfile = Profiles[Config.DefaultProfileName];
        }

        public void SaveCurrentProfile()
        {
            string defpath = Path.Combine(FolderPath, "profile_" + CurrentProfile.SaveName + ".json");
            SaveProfile(CurrentProfile, defpath);
        }
        public void SaveProfile(Profile<T> profile)
        {
            string defpath = Path.Combine(FolderPath, "profile_" + profile.SaveName + ".json");
            SaveProfile(profile, defpath);
        }

        public void ChangeProfile(string savename)
        {
            if (Profiles.ContainsKey(savename))
                CurrentProfile = Profiles[savename];
            else
                ErrorLogCallback("Tried to change profile to '" + savename + "', but no such profile exists");
        }


        public void SaveProfileConfig(string path)
        {
            FileUtility.WriteToFile(path, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }

        public void LoadProfileConfig(string path)
        {
            Config = JsonConvert.DeserializeObject<ProfileConfig>(FileUtility.ReadFromFile(path));
        }

        public void SaveProfile(Profile<T> prof, string path)
        {
            FileUtility.WriteToFile(path, JsonConvert.SerializeObject(prof, Formatting.Indented));
        }

        public Profile<T> LoadProfile(string path)
        {
            return JsonConvert.DeserializeObject<Profile<T>>(FileUtility.ReadFromFile(path));
        }

        public string GetProfilePath(Profile<T> prof)
        {
            return Path.Combine(FolderPath, "profile_" + prof.SaveName + ".json");
        }
    }
}
