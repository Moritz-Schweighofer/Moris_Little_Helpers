using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Schweigm_NETCore_Helpers.Interfaces;

namespace Schweigm_NETCore_Helpers
{
    public class HttpClientHandler<T>
        where T: class, IHttpSignalToSend
    {

        /// <summary>
        /// List of all CA Signals which shall be updated in the next Send Cycle
        /// </summary>
        public List<T> SignalsToUpdate { get; set; } = new List<T>();

        /// <summary>
        /// Lock to lock the CA_SignalsToUpdate List so no Concurrency Issues occur
        /// </summary>
        public object CaSignalsToUpdateLock { get; set; } = new object();

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

        private void UpdateHttpTask(TimeSpan updateTime)
        {
            while (!_updateMqttCancellationTokenSource.Token.IsCancellationRequested)
            {
                _updateMqttCancellationTokenSource.Token.WaitHandle.WaitOne(updateTime);

                if(SignalsToUpdate.Count == 0) continue;

                lock (CaSignalsToUpdateLock)
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
                {"value", signalToSend.Value},
                {"ValueUnit", signalToSend.ValueUnit},
                {"ValueDataType", signalToSend.ValueDataType},
                {"Topic", signalToSend.Topic},
                {"Timestamp", signalToSend.Timestamp}
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
