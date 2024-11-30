using EQTool.Services;
using System;

namespace EQtoolsTests.Fakes
{
    public class AppDispatcherFake : IAppDispatcher
    {
        public void DispatchUI(Action action)
        {
            action();
        }
    }
}
