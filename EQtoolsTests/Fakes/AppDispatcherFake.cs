using EQTool.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EQtoolsTests.Fakes
{
    public class AppDispatcherFake : IAppDispatcher
    {
        public void DispatchUI(Action action)
        {
            action();
        }

        // We don't really wanna fake this stuff to be honest. I don't love having this code duplicated but it shouldn't ever change so it should be fine.
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

                action();
            }, debounceToken);
        }
    }
}
