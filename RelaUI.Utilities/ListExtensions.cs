using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Utilities
{
    public static class ListExtensions
    {
        public static bool EqualOrderContent<T>(this List<T> self, List<T> compare)
        {
            if (self.Count != compare.Count)
                return false;

            for (int i = 0; i < self.Count; i++)
                if (!self[i].Equals(compare[i]))
                    return false;

            return true;
        }

        public static void AddAscendingOrder(this List<ushort> self, ushort obj, bool allowDuplicates = true)
        {
            for (int i = 0; i < self.Count; i++)
            {
                if (self[i] > obj)
                {
                    self.Insert(i, obj);
                    return;
                }
                else if (self[i] == obj)
                {
                    if (allowDuplicates)
                    {
                        self.Insert(i, obj);
                    }
                    return;
                }
            }
            self.Add(obj);
        }

        public static void AddAscendingOrder(this List<int> self, int obj, bool allowDuplicates = true)
        {
            for (int i = 0; i < self.Count; i++)
            {
                if (self[i] > obj)
                {
                    self.Insert(i, obj);
                    return;
                }
                else if (self[i] == obj)
                {
                    if (allowDuplicates)
                    {
                        self.Insert(i, obj);
                    }
                    return;
                }
            }
            self.Add(obj);
        }
    }
}
