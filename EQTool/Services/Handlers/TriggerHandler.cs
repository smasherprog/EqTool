using EQTool.Models;
using System.Linq;
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
                var matched = trigger.Match(e.Line);
                if (matched)
                {
                    // text to speech?
                    if (trigger.AudioEnabled)
                    {
                        textToSpeach.Say(trigger.AudioText);
                    }

                    // displayed text?
                    if (trigger.DisplayTextEnabled)
                    {
                        _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            logEvents.Handle(new OverlayEvent { Text = trigger.DisplayText, ForeGround = Brushes.Red, Reset = false });
                            System.Threading.Thread.Sleep(5000);
                            logEvents.Handle(new OverlayEvent { Text = trigger.DisplayText, ForeGround = Brushes.Red, Reset = true });
                        });
                    }
                }
            }
        }
    }
}
