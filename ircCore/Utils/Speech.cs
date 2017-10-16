/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Speech.Synthesis;

namespace ircCore.Utils
{
    public class Speech : IDisposable
    {
        private readonly SpeechSynthesizer _speech = new SpeechSynthesizer {Volume = 50};

        private readonly List<string> _lines = new List<string>();
        private Prompt _currentText;
        private bool _stop;

        public int Rate
        {
            get { return _speech.Rate; }
            set { _speech.Rate = value; }
        }

        public int Volume
        {
            get { return _speech.Volume; }
            set { _speech.Volume = value; }
        }

        public string Voice
        {
            get { return _speech.Voice.Name; }
            set
            {
                _speech.SelectVoice(value);
            }
        }

        public Speech()
        {
            _speech.SpeakCompleted += ReadTextCompleted;
        }

        ~Speech()
        {
            Dispose();
        }

        public void Preview()
        {
            /* Previews the current voice (settings mode) */
            ReadText(string.Format("Hi, this is a preview of {0} text-to-speech. It's really great to be of assistance to you.", _speech.Voice.Name));
        }

        public void ReadText(string text)
        {
            /* Technically, if we really wanted to, we *could* do word replacement on words that are said phonetically different
             * to how they're spelt, and/or replace :) with "smily face", etc. */
            if (_speech.State != SynthesizerState.Speaking)
            {
                _currentText = new Prompt(text);
                try
                {
                    _speech.SpeakAsync(_currentText);
                }
                catch
                {
                    /* Something went wrong... */
                    return;
                }               
                return;
            }
            /* Add the line to temporary list so it can be read after it finishes speaking current text */
            _lines.Add(text);
        }

        public void Stop()
        {
            if (_speech.State != SynthesizerState.Speaking)
            {
                return;
            }
            _speech.SpeakAsyncCancel(_currentText);
            _stop = true;
        }

        public void Dispose()
        {
            if (_speech == null)
            {
                return;
            }
            _speech.Dispose();
        }

        /* Callback */
        private void ReadTextCompleted(object sender, EventArgs e)
        {            
            if (_stop)
            {
                _stop = false;
                return;
            }
            if (_lines.Count == 0)
            {
                return;
            }
            ReadText(_lines[0]);
            _lines.RemoveAt(0);
        }
    }
}
