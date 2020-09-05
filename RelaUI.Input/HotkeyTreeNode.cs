using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input
{
    public class HotkeyTreeNode
    {
        public Dictionary<Keys, HotkeyTreeNode> Nodes = new Dictionary<Keys, HotkeyTreeNode>();

        public List<HotkeyResult> Result = new List<HotkeyResult>();

        public void Unregister(string name, string source)
        {
            for (int i = 0; i < Result.Count; i++)
            {
                if ((Result[i].Name == name || name == string.Empty) && Result[i].Source == source)
                {
                    Result.RemoveAt(i);
                    i--;
                }
            }

            foreach (HotkeyTreeNode node in Nodes.Values)
                Unregister(name, source);
        }

        public void Destroy()
        {
            foreach (HotkeyTreeNode node in Nodes.Values)
                node.Destroy();

            Result.Clear();
            Result = null;
            Nodes.Clear();
            Nodes = null;
        }
    }
}
