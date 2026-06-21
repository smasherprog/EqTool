using EQTool.Services;
using System;

namespace EQtoolsTests.Fakes
{
    public class TextToSpeachFake : ITextToSpeach
    {
        public Action<string> TextToSpeachCallback;
        public void Say(string text)
        {
            this.TextToSpeachCallback?.Invoke(text);
        }

        public void Say(string text, bool interrupt)
        {
            this.TextToSpeachCallback?.Invoke(text);
        }
    }
}
