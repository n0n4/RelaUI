using RelaUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Sampler.Screens
{
    public interface IScreen
    {
        UIPanel GetPanel();
        void Loaded();
        void Unloaded();
        void Update(float elapsedms);
        void Resize(int newWidth, int newHeight);
    }
}
