using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using RelaUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelaUI.Input
{
    public class HotkeyManager
    {
        public Action<string> ErrorLogCallback;

        public Dictionary<Keys, HotkeyTreeNode> StrokeTree = new Dictionary<Keys, HotkeyTreeNode>();
        public Dictionary<string, Dictionary<string, Func<float, InputManager, Keys[], int, bool>>> HotkeyFunctions = new Dictionary<string, Dictionary<string, Func<float, InputManager, Keys[], int, bool>>>();
        // note: if a hotkeyfunction returns true, it means that we will stop
        // further execution of hotkeys on that stroke. This allows you to make it so
        // hotkeys do not both go off, but can instead happen contextually
        // e.g. A and B are on ctrl-T
        // user hits ctrl-T, A fires first and checks if user has a unit selected
        // they do: A returns true, B never fires
        // they don't: A returns false, B fires

        public List<HotkeySave> Profile = new List<HotkeySave>();
        // the profile is loaded from file and used to find what Stroke to use for a given command
        // Strokes that are not found in the Profile will be stored in the empty list (List<Keys>(){ })

        private Keys[] ActiveStroke = new Keys[8];
        private int ActiveStrokeCount = 0;
        private Keys[] ActiveStrokeSorted = new Keys[8];


        public HotkeyManager(Action<string> errorLogCallback = null)
        {
            ErrorLogCallback = errorLogCallback;
        }

        public void Register(string name, string source, Func<float, InputManager, Keys[], int, bool> func)
        {
            List<List<Keys>> stroke = new List<List<Keys>>();
            // try to get strokes from Profile, throw error if not found or if could not translate to strokes
            HotkeySave prof = FindInProfile(name, source);
            if (prof == null)
            {
                //ConsoleManager.Log(eLogType.ERROR, "HotkeyManager", "Failed to find binding for '" + name + "' from '" + source + "'.", null);
                stroke.Add(new List<Keys>());
                // make default profile
                Profile.Add(new HotkeySave(name, source, new List<string>() { "" }));
            }
            else
            {
                try
                {
                    stroke = prof.GetStroke();
                }
                catch (Exception e)
                {
                    if (ErrorLogCallback != null)
                        ErrorLogCallback("Failed to read binding for '" + name + "' from '" + source + "': " + e.Message);
                    stroke.Add(new List<Keys>());
                }
            }
            for (int i = 0; i < stroke.Count; i++)
            {
                stroke[i] = SortKeys(stroke[i]);
            }
            foreach (List<Keys> s in stroke)
            {
                if (s.Count <= 0)
                    continue; // skip empty hotkeys...
                AddStroke(s.ToArray(), s.Count, new HotkeyResult(name, source));
            }

            // add function
            if (!HotkeyFunctions.ContainsKey(source))
                HotkeyFunctions.Add(source, new Dictionary<string, Func<float, InputManager, Keys[], int, bool>>());
            Dictionary<string, Func<float, InputManager, Keys[], int, bool>> innerHotkeyFunctions = HotkeyFunctions[source];
            if (!innerHotkeyFunctions.ContainsKey(name))
                innerHotkeyFunctions.Add(name, func);
            else
                innerHotkeyFunctions[name] = func;
        }

        public void Unregister(string name, string source)
        {
            // note if name passed in is string.empty, all hotkeys from [source] will be removed
            foreach (HotkeyTreeNode node in StrokeTree.Values)
                Unregister(name, source);
        }

        public HotkeySave FindInProfile(string name, string source)
        {
            HotkeySave found = null;
            foreach (HotkeySave h in Profile)
            {
                if (h.Name == name && h.Source == source)
                {
                    found = h;
                    break;
                }
            }
            return found;
        }

        private List<Keys> SortKeys(List<Keys> ks)
        {
            return ks.OrderByDescending(x => x).ToList();
        }

        public void Update(float elapsedms, InputManager input)
        {
            ActiveStrokeCount = 0;

            // pull out the awaiting keys
            while (ActiveStroke.Length < input.State.FreshKeysCount + input.InputUncapturedCount)
            {
                ActiveStroke = new Keys[ActiveStroke.Length * 2];
                ActiveStrokeSorted = new Keys[ActiveStroke.Length * 2];
            }

            Keys k;
            for (int i = 0; i < input.State.FreshKeysCount; i++)
            {
                k = input.State.FreshKeys[i];
                if (input.IsKeyFresh(k))
                {
                    ActiveStroke[ActiveStrokeCount] = k;
                    ActiveStrokeCount++;
                }
            }

            bool found = false;
            for (int i = 0; i < input.InputUncapturedCount; i++)
            {
                k = input.InputUncaptured[i];
                found = false;
                for (int o = 0; o < ActiveStrokeCount; o++)
                {
                    if (ActiveStroke[o] == k)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    ActiveStroke[ActiveStrokeCount] = k;
                    ActiveStrokeCount++;
                }
            }

            // if at this point we have no strokes, there's no need to proceed
            if (ActiveStrokeCount == 0)
                return;

            // now sort the stroke
            // sort by descending
            ActiveStrokeSorted[0] = ActiveStroke[0];
            for (int i = 1; i < ActiveStrokeCount; i++)
            {
                // find the appropriate place to put this key
                k = ActiveStroke[i];
                found = false;
                for (int o = 0; o < i; o++)
                {
                    if (k > ActiveStrokeSorted[o])
                    {
                        found = true;
                        for (int p = i; p > o; p--)
                        {
                            ActiveStrokeSorted[p] = ActiveStrokeSorted[p - 1];
                        }
                        ActiveStrokeSorted[o] = k;
                        break;
                    }
                }
                if (!found)
                    ActiveStrokeSorted[i] = k;
            }

            // now check the stroke tree
            List<HotkeyResult> results = SearchStrokeTree(ActiveStrokeSorted, ActiveStrokeCount);
            if (results != null)
            {
                foreach (HotkeyResult h in results)
                {
                    if (HotkeyFunctions.TryGetValue(h.Source, out Dictionary<string, Func<float, InputManager, Keys[], int, bool>> dict)
                        && dict.TryGetValue(h.Name, out Func<float, InputManager, Keys[], int, bool> func)
                        && func(elapsedms, input, ActiveStrokeSorted, ActiveStrokeCount))
                    {
                        for (int i = 0; i < ActiveStrokeCount; i++)
                            input.CaptureInput(ActiveStrokeSorted[i]);
                        break;
                    }
                }
            }
        }

        public void Save(string path)
        {
            try
            {
                FileUtility.WriteToFile(path, JsonConvert.SerializeObject(Profile, Formatting.Indented));
            }
            catch (Exception e)
            {
                if (ErrorLogCallback != null)
                    ErrorLogCallback("Failed to save hotkey profile: " + e.Message);
                return;
            }
            //ConsoleManager.Log(eLogType.LOG, "HotkeyManager", "Saved profile at '" + Path.GetFullPath(path) + "'.", null);
        }

        public List<HotkeySave> Load(string path)
        {
            List<HotkeySave> loaded;
            try
            {
                loaded = JsonConvert.DeserializeObject<List<HotkeySave>>(FileUtility.ReadFromFile(path));
            }
            catch (Exception e)
            {
                if (ErrorLogCallback != null)
                    ErrorLogCallback("Failed to load profile: " + e.Message);
                return null;
            }

            if (loaded == null)
            {
                if (ErrorLogCallback != null)
                    ErrorLogCallback("Failed to load profile: File empty or not found.");
                return null;
            }

            return loaded;
        }

        public void LoadProfile(List<HotkeySave> prof)
        {
            for (int i = 0; i < Profile.Count; i++)
            {
                Profile[i].Stroke.Clear();
            }

            for (int i = 0; i < prof.Count; i++)
            {
                // does it exist in the current settings?
                bool found = false;
                for (int o = 0; o < Profile.Count; o++)
                {
                    if (prof[i].Name == Profile[o].Name && prof[i].Source == Profile[o].Source)
                    {
                        found = true;
                        Profile[o].Stroke.AddRange(prof[i].Stroke);
                        break;
                    }
                }

                if (found)
                    continue;

                List<string> stroke = new List<string>();
                Profile.Add(new HotkeySave(prof[i].Name, prof[i].Source, stroke));
                stroke.AddRange(prof[i].Stroke);
            }

            // update Strokes

            // first clear the existing tree out
            foreach (HotkeyTreeNode node in StrokeTree.Values)
                node.Destroy();
            StrokeTree.Clear();

            // reregister every registered method
            foreach (KeyValuePair<string, Dictionary<string, 
                Func<float, InputManager, Keys[], int, bool>>>
                sourceKVP in HotkeyFunctions)
            {
                foreach (KeyValuePair<string,
                    Func<float, InputManager, Keys[], int, bool>> nameKVP in sourceKVP.Value)
                {
                    Register(sourceKVP.Key, nameKVP.Key, nameKVP.Value);
                }
            }
        }

        public List<HotkeySave> MergeHotkeySaves(List<HotkeySave> lista, List<HotkeySave> listb)
        {
            // note: listb takes priority
            List<HotkeySave> results = new List<HotkeySave>();
            
            for (int i = 0; i < lista.Count; i++)
            {
                HotkeySave hs = lista[i];
                List<string> stroke = new List<string>();
                stroke.AddRange(hs.Stroke);
                results.Add(new HotkeySave(hs.Name, hs.Source, stroke));
            }

            for (int i = 0; i < listb.Count; i++)
            {
                HotkeySave hs = listb[i];
                bool found = false;
                for (int o = 0; o < results.Count; o++)
                {
                    if (results[o].Source == hs.Source
                        && results[o].Name == hs.Name)
                    {
                        results[o].Stroke.Clear();
                        results[o].Stroke.AddRange(hs.Stroke);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    List<string> stroke = new List<string>();
                    stroke.AddRange(hs.Stroke);
                    results.Add(new HotkeySave(hs.Name, hs.Source, stroke));
                }
            }

            return results;
        }

        public List<HotkeyResult> SearchStrokeTree(Keys[] keys, int count)
        {
            int depth = 0;
            Dictionary<Keys, HotkeyTreeNode> next = StrokeTree;
            while (next != null)
            {
                if (next.TryGetValue(keys[depth], out HotkeyTreeNode node))
                {
                    // found the next one
                    depth++;
                    if (depth == count)
                        return node.Result;

                    next = node.Nodes;
                }
                else
                {
                    next = null;
                }
            }
            return null;
        }

        public void AddStroke(Keys[] keys, int count, HotkeyResult result)
        {
            int depth = 0;
            Dictionary<Keys, HotkeyTreeNode> next = StrokeTree;
            while (depth < count)
            {
                if (!next.TryGetValue(keys[depth], out HotkeyTreeNode node))
                {
                    node = new HotkeyTreeNode();
                    next.Add(keys[depth], node);
                }

                depth++;
                if (depth == count)
                {
                    node.Result.Add(result);
                    return;
                }

                next = node.Nodes;
            }
        }
    }
}