using EQTool.Models;
using System;
using System.Windows.Media;

namespace EQTool.Services
{
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
                logEvents.Handle(new OverlayEvent { Text = text, ForeGround = color, Duration = TimeSpan.FromSeconds(5) });
            }

            if (output.AudioType == TriggerAudioType.TextToSpeech && !string.IsNullOrWhiteSpace(output.TtsText))
            {
                textToSpeach.Say(expand(output.TtsText));
            }
            else if (output.AudioType == TriggerAudioType.SoundFile && !string.IsNullOrWhiteSpace(output.SoundFile))
            {
                audioService.Play(output.SoundFile);
            }
        }
    }
}
