using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class UserDefinedTriggerHandler : BaseHandler
    {
        //
        // ctor
        //
        // register this service as a listener for the Events it cares about
        //
        public UserDefinedTriggerHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.UserDefinedTriggerEvent += LogEvents_UserDefinedTriggerEvent;
        }

        //
        // function that gets called for a UserDefinedTriggerEvent
        //
        private void LogEvents_UserDefinedTriggerEvent(object sender, UserDefinedTriggerEvent triggerEvent)
        {
            return;
            // text to speech?
            if (triggerEvent.Trigger.AudioEnabled == true)
            {
                textToSpeach.Say(triggerEvent.Trigger.AudioText);
            }

            // displayed text?
            if (triggerEvent.Trigger.TextEnabled == true)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = triggerEvent.Trigger.DisplayText, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(5000);
                    logEvents.Handle(new OverlayEvent { Text = triggerEvent.Trigger.DisplayText, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
