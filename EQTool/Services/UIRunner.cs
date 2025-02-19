using EQTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public class UIRunner : IDisposable
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private System.Timers.Timer timer;

        public UIRunner(SpellWindowViewModel spellWindowViewModel)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += UITimer_Elapsed;
            timer.Enabled = true;
        }

        private DateTime? LastUIRun = null;
        private DateTime? LastBoatUpdate = null;
        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var dt_ms = 0.0;
            if (LastUIRun.HasValue)
            {
                dt_ms = (now - LastUIRun.Value).TotalMilliseconds;
            }

            LastUIRun = now;
            spellWindowViewModel.UpdateSpells(dt_ms);
            if (!LastBoatUpdate.HasValue || (LastBoatUpdate.HasValue && (now - LastBoatUpdate.Value).TotalMinutes > 5))
            {
                Task.Factory.StartNew(() =>
                { 
                    spellWindowViewModel.UpdateBoats();
                });
                LastBoatUpdate = now;
            }
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }
    }
}
