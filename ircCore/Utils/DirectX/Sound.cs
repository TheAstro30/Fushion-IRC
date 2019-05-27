/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ircCore.Utils.DirectX.Core;
using Timer = System.Windows.Forms.Timer;

namespace ircCore.Utils.DirectX
{
    public class Sound
    {
        private Timer _play;
        private IBasicAudio _audio;
        private IGraphBuilder _graph;
        private IMediaControl _media;
        private int _pan;
        private IMediaPosition _position;
        private int _volume;

        public event Action<Sound> OnMediaEnded;
        public event Action<Sound, double> OnMediaPositionChanged;

        public string Clip { get; set; }

        /* Public properties */
        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_volume < 0)
                {
                    _volume = 0;
                }
                if (_volume > 100)
                {
                    _volume = 100;
                }
                if (_audio != null)
                {
                    _audio.SetVolume((_volume - 100)*50);
                }
            }
        }

        public int Pan
        {
            get { return _pan; }
            set
            {
                _pan = value;
                if (_pan < 0)
                {
                    _pan = 0;
                }
                if (_pan > 100)
                {
                    _pan = 100;
                }
                if (_audio != null)
                {
                    _audio.SetBalance((_pan - 50)*200);
                }
            }
        }

        public double Position
        {
            get
            {
                double pos;
                _position.GetCurrentPosition(out pos);
                return pos;
            }
            set
            {
                double duration;
                _position.GetDuration(out duration);
                if (value <= duration)
                {
                    _position.SetCurrentPosition(value);
                }
            }
        }

        public double Duration
        {
            get
            {
                double duration;
                _position.GetDuration(out duration);
                return duration;
            }
        }

        /* Constructor/destuctor */
        public Sound()
        {
            Init();
        }

        public Sound(string fileName)
        {
            Clip = fileName;
            Init();
        }

        ~Sound()
        {
            Close();
        }

        /* Public methods */
        public void PlayAsync()
        {
            /* This does NOT check for cross-threading, so if events are raised, it's up to the code at the other
             * end to do Invoke/BeginInvoke */
            var t = new Thread(Play) {IsBackground = true};
            t.Start();
        }

        public void Play()
        {
            Close();
            if (!Open() || _media == null || _media.Run() < 0)
            {
                return;
            }
            _audio.SetVolume((_volume - 100)*50);
            _audio.SetBalance((_pan - 50)*200);
            _play.Enabled = true;
        }

        public void Stop()
        {
            if (_media != null)
            {
                _media.Stop();
            }
            _play.Enabled = false;
        }

        public void Pause()
        {
           if (_media != null)
           {
               _media.Pause();
           }
            _play.Enabled = false;
        }

        public void Resume()
        {
            if (_media != null)
            {
                if (_media.Run() < 0)
                {
                    return;
                }
            }
            _play.Enabled = true;
        }

        /* Private helper methods */    
        private void Init()
        {
            _pan = 50;
            var timer = new Timer
                            {
                                Interval = 200
                            };
            _play = timer;
            _play.Tick += TimerTick;
        }

        private bool Open()
        {
            bool flag;
            if (string.IsNullOrEmpty(Clip) || !File.Exists(Clip))
            {
                return false;
            }
            object o = null;
            try
            {
                var typeFromClsid = Type.GetTypeFromCLSID(Clsid.FilterGraph);
                if (typeFromClsid == null)
                {
                    return false;
                }
                o = Activator.CreateInstance(typeFromClsid);
                _graph = (IGraphBuilder) o;
                o = null;
                if (_graph.RenderFile(Clip, null) < 0)
                {
                    return false;
                }
                _media = (IMediaControl)_graph;
                _position = (IMediaPosition)_graph;
                _audio = _graph as IBasicAudio;                   
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            finally
            {
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return flag;
        }

        private void Close()
        {
            _position = null;
            _media = null;
            _audio = null;
            if (_graph != null)
            {
                Marshal.ReleaseComObject(_graph);
                _graph = null;
            }
            GC.Collect();
        }

        /* Timer callback for getting the current position of the streaming audio */
        private void TimerTick(object sender, EventArgs e)
        {
            double pos;
            double duration;
            _position.GetCurrentPosition(out pos);
            _position.GetDuration(out duration);
            if (OnMediaPositionChanged != null)
            {
                OnMediaPositionChanged(this, pos);
            }
            if (pos < duration)
            {
                return;
            }
            Stop();
            if (OnMediaEnded != null)
            {
                OnMediaEnded.Invoke(this);
            }
        }
    }
}