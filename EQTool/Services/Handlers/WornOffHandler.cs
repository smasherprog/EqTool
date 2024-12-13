using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services.Handlers
{
    public class WornOffHandler : BaseHandler
    {
        // ctor
        public WornOffHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.WornOffEvent += LogParser_WornOffEvent;
        }

        // catch the event and handle it
        private void LogParser_WornOffEvent(object sender, WornOffEvent e)
        {
            bool playAudio = (bool)(activePlayer?.Player?.WornOffAudio);
            bool showText = (bool)(activePlayer?.Player?.WornOffOverlay);
            var text = $"{e.SpellName} faded";

            if (playAudio)
            {
                textToSpeach.Say(text);
            }

            if (showText)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(5000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }

        }

    }
}
