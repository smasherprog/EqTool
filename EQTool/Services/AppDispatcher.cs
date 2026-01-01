using System;
using System.Threading;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public interface IAppDispatcher
    {
        void DispatchUI(Action action);
        void DebounceToUI(ref CancellationTokenSource debounceCancellationSource, int delay, Action action);
        void DebounceToUI(ref CancellationTokenSource debounceCancellationSource, int delay, Action action, Func<bool> shouldCancel);
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

        public void DebounceToUI(ref CancellationTokenSource debounceCancellationSource, int delay, Action action) => DebounceToUI(ref debounceCancellationSource, delay, action, () => false);
        public void DebounceToUI(ref CancellationTokenSource debounceCancellationSource, int delay, Action action, Func<bool> shouldCancel)
        {
            debounceCancellationSource?.Cancel();
            debounceCancellationSource = new CancellationTokenSource();
            var debounceToken = debounceCancellationSource.Token;
            
            Task.Run(async () =>
            {
                await Task.Delay(delay, debounceToken);
                
                if (debounceToken.IsCancellationRequested || shouldCancel())
                    return;

                DispatchUI(() => action());
            }, debounceToken);
        }
    }
}
