using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class TriggerHandler : BaseHandler
    {
        public TriggerHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.LineEvent += LogEvents_LineEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            foreach (var trigger in eQToolSettings.Triggers.Where(a => a.TriggerEnabled))
            {
                // check for a match
                var regex = trigger.TriggerRegex;
                var match = regex.Match(e.Line);
                if (match.Success)
                {
                    // save the values discovered in the named groups
                    trigger.SaveNamedGroupValues(match);
                    // text to speech?
                    if (trigger.AudioTextEnabled == true)
                    {
                        textToSpeach.Say(trigger.ConvertedAudioText);
                    }

                    // displayed text?
                    if (trigger.DisplayTextEnabled == true)
                    {
                        _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            logEvents.Handle(new OverlayEvent { Text = trigger.ConvertedDisplayText, ForeGround = Brushes.Red, Reset = false });
                            System.Threading.Thread.Sleep(5000);
                            logEvents.Handle(new OverlayEvent { Text = trigger.ConvertedDisplayText, ForeGround = Brushes.Red, Reset = true });
                        });
                    }
                }
            }
        }
    }
}
