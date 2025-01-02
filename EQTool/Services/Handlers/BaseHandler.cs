using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class BaseHandlerData
    {
        public readonly LogEvents logEvents;
        public readonly ActivePlayer activePlayer;
        public readonly EQToolSettings eQToolSettings;
        public readonly ITextToSpeach textToSpeach;
        public readonly IAppDispatcher appDispatcher;
        public readonly DebugOutput debugOutput;

        public BaseHandlerData(DebugOutput debugOutput, IAppDispatcher appDispatcher, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach)
        {
            this.debugOutput = debugOutput;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
            this.eQToolSettings = eQToolSettings;
            this.textToSpeach = textToSpeach;
        }
    }

    public abstract class BaseHandler
    {
        protected readonly LogEvents logEvents;
        protected readonly ActivePlayer activePlayer;
        protected readonly EQToolSettings eQToolSettings;
        protected readonly ITextToSpeach textToSpeach;
        protected readonly IAppDispatcher appDispatcher;
        protected readonly DebugOutput debugOutput;

        public BaseHandler(BaseHandlerData baseHandlerData)
        {
            debugOutput = baseHandlerData.debugOutput;
            appDispatcher = baseHandlerData.appDispatcher;
            activePlayer = baseHandlerData.activePlayer;
            logEvents = baseHandlerData.logEvents;
            eQToolSettings = baseHandlerData.eQToolSettings;
            textToSpeach = baseHandlerData.textToSpeach;
        }
    }
}
