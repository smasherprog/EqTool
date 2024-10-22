using System;
using System.Threading;

namespace EQTool.Services
{
    public interface IAppDispatcher
    {
        void DispatchUI(Action action);
    }

    public class AppDispatcher : IAppDispatcher
    {
        public void DispatchUI(Action action)
        {
            try
            {
                if (App.Current == null)
                {
                    return;
                }

                if (Thread.CurrentThread == App.Current.Dispatcher.Thread)
                {
                    action();
                }
                else
                {
                    App.Current.Dispatcher.Invoke(action);
                }
            }
            catch
            {

            }
        }
    }
}
