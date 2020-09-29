using System;
using System.Windows.Threading;

namespace TestApp.Models.Helpers
{
    public class TimeNotifier
    {
        private bool _corrected = false;
        private readonly DispatcherTimer _timer;

        #region События

        public event Handlers.EmptyHandler NewMinute;
        public event Handlers.EmptyHandler NewHour;
        public event Handlers.EmptyHandler NewDay;

        #endregion

        public TimeNotifier()
        {
            _timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
        }

        public void StartWatching()
        {
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        public void StopWatching()
        {
            _timer.Tick -= TimerTick;
            _timer.Stop();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            var time = DateTime.Now;
            var seconds = time.Second;
            if (seconds == 0 && !_corrected)
            {
                _corrected = true;
                _timer.Stop();
                _timer.Interval = new TimeSpan(0, 1, 0);
                _timer.Start();
            }
            else if (!_corrected) return;

            NewMinute?.Invoke();
            if (time.Minute == 0) NewHour?.Invoke();
            if (time.Hour == 0 && time.Minute == 0) NewDay?.Invoke();
        }
    }
}
