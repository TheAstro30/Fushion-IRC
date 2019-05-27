/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;

namespace ircScript.Classes.ScriptFunctions
{
    public class UserTimer : Timer
    {
        private readonly bool _isMilliseconds;

        private int _tick;
        private int _interval;

        /* Public properties */
        public string Name { get; set; }
        
        public new int Interval
        {
            get { return base.Interval; }
            set
            {
                if (_isMilliseconds)
                {
                    _interval = value;
                    base.Interval = value;
                }
                else
                {
                    _interval = value;
                    base.Interval = 1000;
                }
                
            }
        }
        
        public bool IsContinuous { get; set; }

        /* Events raised */
        public event Action<UserTimer> OnTimer;

        public event Action<UserTimer> OnTimerEnded;

        /* Constructor */
        public UserTimer(bool isMilliseconds)
        {
            _isMilliseconds = isMilliseconds;
        }

        /* Timer callback */
        protected override void OnTick(EventArgs e)
        {
            _tick++;
            if (!_isMilliseconds && _tick < _interval)
            {
                return;
            }
            _tick = 0;
            if (OnTimer != null)
            {
                OnTimer(this);
            }
            if (IsContinuous)
            {
                return;
            }
            Stop();
            if (OnTimerEnded != null)
            {
                OnTimerEnded(this);
            }
            base.OnTick(e);
        }
    }
}
