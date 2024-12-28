using EQTool.Models;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class TellsYouHandler : BaseHandler
    {
        public TellsYouHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.CommsEvent += LogEvents_CommsEvent;
        }
        private void LogEvents_CommsEvent(object sender, CommsEvent commsEvent)
        {
            // tell?
            if (commsEvent.TheChannel == CommsEvent.Channel.TELL)
            {
                // was the tell to player?
                if ((commsEvent.Receiver == "You") || (commsEvent.Receiver == "you"))
                {
                    // screen out sender names containing a space - this will screen out most vendors
                    if (commsEvent.Sender.Contains(" ") == false)
                    {
                        // screen out tells from pets who are responding to /pet attack
                        if (commsEvent.Content.StartsWith("Attacking") == false)
                        {
                            // text to be displayed / spoken
                            var text = $"{commsEvent.Sender} sent a tell";

                            // text to speech?
                            if (activePlayer?.Player?.TellsYouAudio == true)
                            {
                                textToSpeach.Say(text);
                            }

                            if (activePlayer?.Player?.TellsYouOverlay == true)
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
            }
        }
    }
}
