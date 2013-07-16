using System;
using System.Threading;

namespace WitMorph.UI
{
    class DisposableAction : IDisposable
    {
        private Action _onDispose;

        public DisposableAction(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            var onDispose = Interlocked.Exchange(ref _onDispose, null);
            if (onDispose != null) onDispose();
        }
    }
}
