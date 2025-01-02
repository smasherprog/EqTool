using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services.Handlers
{
    // alert user when a tell is received
    public class TellsYouHandler : BaseHandler
    {
        //
        // ctor
        //
        // register this service as a listener for the Events it cares about
        //
        public TellsYouHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.CommsEvent += LogEvents_CommsEvent;
        }

        //
        // function that gets called for a CommsEvent
        //
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
                            string text = $"{commsEvent.Sender} sent a tell";

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
