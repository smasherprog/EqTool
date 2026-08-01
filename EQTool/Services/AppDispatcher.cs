using System;
using System.Threading;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public interface IAppDispatcher
    {
        void DispatchUI(Action action);
        // Queues the action on the UI thread at Background priority (below rendering and
        // input), even when already on the UI thread. Use to yield between slices of a
        // long-running UI-thread job so the window stays responsive.
        void DispatchUIBackground(Action action);
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

        public void DispatchUIBackground(Action action)
        {
            try
            {
                _ = App.Current?.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, action);
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
