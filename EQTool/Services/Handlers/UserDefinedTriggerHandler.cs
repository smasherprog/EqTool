using EQTool.Models;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class UserDefinedTriggerHandler : BaseHandler
    {
        public UserDefinedTriggerHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.UserDefinedTriggerEvent += LogEvents_UserDefinedTriggerEvent;
        }

        private void LogEvents_UserDefinedTriggerEvent(object sender, UserDefinedTriggerEvent triggerEvent)
        {
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
