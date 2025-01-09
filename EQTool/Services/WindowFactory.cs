using Autofac;
using EQTool.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EQTool.Services
{
    public class WindowFactory
    {
        public readonly List<BaseSaveStateWindow> WindowList = new List<BaseSaveStateWindow>();

        //anti pattern below, but its the only way to do this.
        private readonly ILifetimeScope container;
        public WindowFactory(ILifetimeScope container)
        {
            this.container = container;
        }

        public T CreateWindow<T>() where T : Window
        {
            return container.Resolve<T>();
        }

        public void ToggleWindow<T>(System.Windows.Forms.MenuItem m) where T : BaseSaveStateWindow
        {
            var w = WindowList.FirstOrDefault(a => a.GetType() == typeof(T));
            m.Checked = !m.Checked;
            if (m.Checked)
            {
                if (w != null)
                {
                    _ = w.Focus();
                }
                else
                {
                    w?.Close();
                    w = container.Resolve<T>();
                    WindowList.Add(w);
                    w.Closed += (se, ee) =>
                    {
                        m.Checked = false;
                        _ = WindowList.Remove(w);
                    };
                    w.Show();
                }
            }
            else
            {
                w?.CloseWindow();
                _ = WindowList.Remove(w);
            }
        }

        public void OpenWindow<T>(System.Windows.Forms.MenuItem m) where T : BaseSaveStateWindow
        {
            var w = WindowList.FirstOrDefault(a => a.GetType() == typeof(T));
            if (w != null)
            {
                _ = w.Focus();
            }
            else
            {
                m.Checked = true;
                w?.Close();
                w = container.Resolve<T>();
                WindowList.Add(w);
                w.Closed += (se, ee) =>
                {
                    m.Checked = false;
                    _ = WindowList.Remove(w);
                };
                w.Show();
            }
        }
    }
}
