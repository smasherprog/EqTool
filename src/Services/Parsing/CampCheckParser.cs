using System.Diagnostics;
using static EQTool.Services.EventsList;

namespace EQTool.Services.Parsing
{
    public class CampCheckParser : ILogParser
    {
        public bool StillCamping;
        public bool HasUsedStartupEnterWorld;
        private readonly EventsList eventsList;
        private readonly IAppDispatcher appDispatcher;

        public CampCheckParser(IAppDispatcher appDispatcher, EventsList eventsList)
        {
            HasUsedStartupEnterWorld = StillCamping = false;
            this.eventsList = eventsList;
            this.appDispatcher = appDispatcher;
        }

        public bool Evaluate(string line, string previousline)
        {
            if (line == "It will take about 5 more seconds to prepare your camp.")
            {
                StillCamping = true;
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(1000 * 6);
                    if (StillCamping)
                    {
                        appDispatcher.DispatchUI(() =>
                        {
                            Debug.WriteLine("CampEvent");
                            this.eventsList.Handle(new CampEventArgs());
                        });
                    }
                });
                return true;
            }
            else if (line == "You abandon your preparations to camp.")
            {
                StillCamping = false;
                return true;
            }
            else if (line == "Welcome to EverQuest!")
            {
                HasUsedStartupEnterWorld = true;
                Debug.WriteLine("EnteredWorldEvent In Game");
                this.eventsList.Handle(new EnteredWorldArgs());
                return true;
            }
            return false;
        }
    }
}
