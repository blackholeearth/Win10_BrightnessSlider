using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Threading;


/// <summary>
//   https://stackoverflow.com/a/44876143/3137362  - rickstrahl
/// </summary>
public class DebounceDispatcher
{
    private DispatcherTimer timer;
    private DateTime timerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);

    public void Debounce(int interval, Action<object> action,
        object param = null,
        DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
        Dispatcher disp = null)
    {
        // kill pending timer and pending ticks
        timer?.Stop();
        timer = null;

        if (disp == null)
            disp = Dispatcher.CurrentDispatcher;

        // timer is recreated for each event and effectively
        // resets the timeout. Action only fires after timeout has fully
        // elapsed without other events firing in between
        timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
        {
            if (timer == null)
                return;

            timer?.Stop();
            timer = null;
            action.Invoke(param);
        }, disp);

        timer.Start();
    }


    public void Throttle(int interval, Action<object> action,
     object param = null,
     DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
     Dispatcher disp = null)
    {
        // kill pending timer and pending ticks
        timer?.Stop();
        timer = null;

        if (disp == null)
            disp = Dispatcher.CurrentDispatcher;

        var curTime = DateTime.UtcNow;

        // if timeout is not up yet - adjust timeout to fire 
        // with potentially new Action parameters           
        if (curTime.Subtract(timerStarted).TotalMilliseconds < interval)
            interval = (int)curTime.Subtract(timerStarted).TotalMilliseconds;

        timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
        {
            if (timer == null)
                return;

            timer?.Stop();
            timer = null;
            action.Invoke(param);
        }, disp);

        timer.Start();
        timerStarted = curTime;
    }



}











