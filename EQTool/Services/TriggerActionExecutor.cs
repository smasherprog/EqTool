using EQTool.Models;
using System;
using System.Windows.Media;

namespace EQTool.Services
{
    // Executes a TriggerOutput: overlay display text and audio (text-to-speech or
    // sound file). Shared by the Basic, Timer Ending and Timer Ended outputs.
    // Simplified {name} placeholders are expanded via the provided expand function
    // (the matching trigger).
    public class TriggerActionExecutor
    {
        private readonly LogEvents logEvents;
        private readonly ITextToSpeach textToSpeach;
        private readonly IAudioService audioService;

        public TriggerActionExecutor(LogEvents logEvents, ITextToSpeach textToSpeach, IAudioService audioService)
        {
            this.logEvents = logEvents;
            this.textToSpeach = textToSpeach;
            this.audioService = audioService;
        }

        public void Execute(TriggerOutput output, Func<string, string> expand)
        {
            if (output == null)
            {
                return;
            }

            if (output.DisplayTextEnabled && !string.IsNullOrWhiteSpace(output.DisplayText))
            {
                var text = expand(output.DisplayText);
                var color = TriggerColors.ToBrush(output.DisplayTextColor, Brushes.Red);
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = color, Reset = false });
                    System.Threading.Thread.Sleep(5000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = color, Reset = true });
                });
            }

            if (output.AudioType == TriggerAudioType.TextToSpeech && !string.IsNullOrWhiteSpace(output.TtsText))
            {
                textToSpeach.Say(expand(output.TtsText), output.InterruptSpeech);
            }
            else if (output.AudioType == TriggerAudioType.SoundFile && !string.IsNullOrWhiteSpace(output.SoundFile))
            {
                audioService.Play(output.SoundFile);
            }
        }
    }
}
