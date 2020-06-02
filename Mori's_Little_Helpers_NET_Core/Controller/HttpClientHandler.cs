using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Schweigm_NETCore_Helpers.Interfaces;

namespace Schweigm_NETCore_Helpers.Controller
{
    public class HttpClientHandler<T>
        where T: class, IHttpSignal
    {

        /// <summary>
        /// List of all CA Signals which shall be updated in the next Send Cycle
        /// </summary>
        private List<T> SignalsToUpdate { get; } = new List<T>();

        /// <summary>
        /// Lock to lock the CA_SignalsToUpdate List so no Concurrency Issues occur
        /// </summary>
        private object SignalsToUpdateLock { get; } = new object();


        public string Host
        {
            get => _host;
            set 
            {
                _host = value;
                _uri = $"http://{_host}:{_port}/{_controller}";
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                _uri = $"http://{_host}:{_port}/{_controller}";
            }
        }
        public string Controller
        {
            get => _controller;
            set
            {
                _controller = value;
                _uri = $"http://{_host}:{_port}/{_controller}";
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsActive { get; private set; }

        private string _host;
        private int _port;
        private string _controller;
        private readonly HttpClient _httpClient = new HttpClient();
        private string _uri = string.Empty;
        private CancellationTokenSource _updateMqttCancellationTokenSource;


        public async Task<bool> CheckConnection()
        {
            await _httpClient.GetAsync(_uri);
            return true;
        }

        public void StartDataTransmission(TimeSpan updateTime)
        {
            _updateMqttCancellationTokenSource = new CancellationTokenSource();

            new Task(() =>
                    UpdateHttpTask(updateTime), _updateMqttCancellationTokenSource.Token,
                TaskCreationOptions.LongRunning)
                .Start();

            IsActive = true;
        }

        public void StopDataTransmission()
        {
            _updateMqttCancellationTokenSource.Cancel();
            IsActive = false;
        }

        public void UpdateSignal(T signal)
        {
            lock (SignalsToUpdateLock)
            {
                var indexSignal = SignalsToUpdate.FindIndex(m => m.ID == signal.ID);
                if (indexSignal >= 0) SignalsToUpdate[indexSignal] = signal;
                else SignalsToUpdate.Add(signal);
            }
        }

        private void UpdateHttpTask(TimeSpan updateTime)
        {
            while (!_updateMqttCancellationTokenSource.Token.IsCancellationRequested)
            {
                _updateMqttCancellationTokenSource.Token.WaitHandle.WaitOne(updateTime);

                if(SignalsToUpdate.Count == 0) continue;

                lock (SignalsToUpdateLock)
                {

                    foreach (var signal in SignalsToUpdate)
                    {
                        SendCaValueHttp(signal);
                    }
                    SignalsToUpdate.Clear();
                }

            }
        }

        private void SendCaValueHttp(T signalToSend)
        {

            var msg = new Dictionary<string, string>
            {
                {"item", signalToSend.Item},
                {"value", signalToSend.Value},
                {"valueUnit", signalToSend.ValueUnit},
                {"timestamp", signalToSend.Timestamp}
            };

            var content = new FormUrlEncodedContent(msg);

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var response = await _httpClient.PostAsync(_uri, content);
                    CoolConsoleOutput.Write("HTTP Publish", response.Content.ToString(), ConsoleColor.Cyan);
                }
                catch (HttpRequestException)
                {

                }
            });

        }
    }
}
