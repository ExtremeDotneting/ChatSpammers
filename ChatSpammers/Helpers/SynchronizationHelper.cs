using System;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;

namespace Helpers
{
    public static class SynchronizationHelper
    {
        static Action emptyAct = new Action(delegate { });

        public static void DoEvents()
        {
            Dispatcher.CurrentDispatcher.Invoke(
                DispatcherPriority.Background,
                emptyAct
                );

        }
        /// <summary>
        /// Not blocking UI thread.
        /// </summary>
        public static void Pause(int ms)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                bool waiting = true;
                new Thread(() =>
                {
                    Thread.Sleep(ms);
                    waiting = false;
                }).Start();
                //Если действие выполняется в главном потоке - блокируем метод через DoEvents. Это не блокирует сам поток.
                while (waiting)
                {
                    DoEvents();
                }
            }
            else
            {
                Thread.Sleep(ms);
            }
        }
        /// <summary>
        /// Возвращает истину, если было превышено время ожидания
        /// </summary>
        public static bool WaitFor(Func<bool> isWaitingDelegate, int timeoutMS)
        {
            int loopDelay = 10;
            while (isWaitingDelegate() && timeoutMS > 0)
            {
                Pause(loopDelay);
                timeoutMS -= loopDelay;
            }
            return timeoutMS <= 0;
        }

    }

}