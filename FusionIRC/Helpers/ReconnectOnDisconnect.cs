/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Networks;

namespace FusionIRC.Helpers
{
    public class ReconnectOnDisconnect
    {
        /* The aim of this class is to give the client a re-connect on disconnect option, get the next server in group, etc
         * in a central class */
        private readonly ClientConnection _client;

        private int _currentTick;
        private int _times;

        private bool _connectOnce;

        private readonly Timer _delay;

        public event Action OnReconnectCancel;
        public event Action<int> OnReconnectTimer;
        public event Action<Server> OnConnectionTry;

        public bool IsRetryingConnection { get; set; }

        /* Public methods */
        public ReconnectOnDisconnect(ClientConnection client)
        {
            _client = client;

            _delay = new Timer {Interval = 1000};
            _delay.Tick += DelayTimer;
        }

        public void BeginReconnect()
        {
            if (!SettingsManager.Settings.Connection.Options.Reconnect)
            {
                return;
            }
            if (SettingsManager.Settings.Connection.Options.RetryConnection)
            {
                System.Diagnostics.Debug.Print("fuck " + _times);
                IsRetryingConnection = true;
                var t = SettingsManager.Settings.Connection.Options.RetryTimes;
                if (t == 0)
                {
                    ReconnectToServer();
                    return;
                }
                _times++;
                if (_times >= t)
                {
                    _times = 0;
                    _currentTick = 0;
                    _connectOnce = false;
                    IsRetryingConnection = false;
                    return;
                }
                if (OnReconnectTimer != null)
                {
                    OnReconnectTimer(SettingsManager.Settings.Connection.Options.RetryDelay);
                }
                _delay.Enabled = true;
                return;
            }
            if (!_connectOnce)
            {
                IsRetryingConnection = true;
                _connectOnce = true;
                ReconnectToServer();
                return;
            }
            _connectOnce = false;
            _times = 0;
            _currentTick = 0;
        }

        public void Cancel()
        {
            _times = 0;
            _currentTick = 0;
            _connectOnce = false;
            _delay.Enabled = false;
            IsRetryingConnection = false;
            if (OnReconnectCancel != null)
            {
                OnReconnectCancel();
            }
        }

        /* Private methods */
        private void ReconnectToServer()
        {
            var s = _client.Server;
            if (string.IsNullOrEmpty(s.Address))
            {
                return;
            }
            var net = ServerManager.GetNetworkNameByServerAddress(s.Address);
            if (SettingsManager.Settings.Connection.Options.NextServer)
            {
                s = ServerManager.GetNextServer(net);
            }
            if (s == null)
            {
                s = _client.Server;
            }
            if (OnConnectionTry != null)
            {
                OnConnectionTry(s);
            }
        }

        private void DelayTimer(object sender, EventArgs e)
        {
            _currentTick++;
            var t = SettingsManager.Settings.Connection.Options.RetryDelay - _currentTick;
            if (OnReconnectTimer != null)
            {
                OnReconnectTimer(t);
            }
            if (_currentTick < SettingsManager.Settings.Connection.Options.RetryDelay)
            {
                return;
            }
            ReconnectToServer();
            _currentTick = 0;
            _delay.Enabled = false;
        }
    }
}
