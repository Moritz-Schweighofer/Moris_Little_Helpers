using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Schweigm_NETCore_Helpers.Interfaces;

namespace Schweigm_NETCore_Helpers.Controller
{
    public class MqttClientHandler<T>
        where T: class, IMqttSignal
    {

        #region Public-Member

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsActive { get; private set; }

        #endregion Public-Member

        #region Private-Member
        /// <summary>
        /// List of all CA Signals which shall be updated in the next Send Cycle
        /// Object: Signal to Update / String: Topic
        /// </summary>
        // ReSharper disable once CollectionNeverUpdated.Global
        private List<T> SignalsToUpdate { get; } = new List<T>();

        /// <summary>
        /// Lock to lock the CA_SignalsToUpdate List so no Concurrency Issues occur
        /// </summary>
        private object SignalsToUpdateLock { get; } = new object();

        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;
        private CancellationTokenSource _updateMqttCancellationTokenSource;

        #endregion Private-Member

        #region Constructor

        public MqttClientHandler(string brokerAddress, string clientName,
            string brokerUser, string brokerPassword)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId(clientName)
                .WithTcpServer(brokerAddress)
                .WithCredentials(brokerUser, brokerPassword)
                .Build();
        }

        #endregion Constructor

        #region Public-Functions

        /// <summary>
        /// Connect to the given Broker with username and password
        /// </summary>
        public void Connect()
        {
            if(_mqttClient.IsConnected) return;
            _mqttClient.ConnectAsync(_mqttOptions);
            IsActive = true;
        }

        /// <summary>
        /// Disconnect from the Broker
        /// </summary>
        public void Disconnect()
        {
            if (!_mqttClient.IsConnected) return;
            _mqttClient.DisconnectAsync();
            IsActive = false;
        }

        /// <summary>
        /// Start the Data Transmission Task in the given update Time
        /// </summary>
        /// <param name="updateTime"></param>
        public void StartDataTransmission(TimeSpan updateTime)
        {
            _updateMqttCancellationTokenSource = new CancellationTokenSource();

            new Task(() =>
            UpdateMqttTask(updateTime), _updateMqttCancellationTokenSource.Token, TaskCreationOptions.LongRunning)
                .Start();
        }

        /// <summary>
        /// Stops the Data Transmission Task
        /// </summary>
        public void StopDataTransmission()
        {
            _updateMqttCancellationTokenSource.Cancel();
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

        #endregion Public-Functions

        #region Private-Functions

        /// <summary>
        /// The Update Task which sends the Signals to update periodically to the Broker
        /// </summary>
        /// <param name="updateTime"></param>
        private void UpdateMqttTask(TimeSpan updateTime)
        {
            while (!_updateMqttCancellationTokenSource.Token.IsCancellationRequested)
            {
                _updateMqttCancellationTokenSource.Token.WaitHandle.WaitOne(updateTime);

                if (SignalsToUpdate.Count == 0) continue;

                lock (SignalsToUpdateLock)
                {

                    foreach (var signalToSend in SignalsToUpdate)
                    {
                        SendCaValue(signalToSend);
                    }

                    SignalsToUpdate.Clear();
                }
            }
        }

        /// <summary>
        /// Sends the Numeric Signal to the CA
        /// </summary>
        /// <param name="signalToSend"></param>
        private void SendCaValue(T signalToSend)
        {
            var json = JsonConvert.SerializeObject(signalToSend, Formatting.Indented);
            var msg = Encoding.ASCII.GetBytes(json);
            _mqttClient.PublishAsync(signalToSend.Topic, msg);

            CoolConsoleOutput.Write("MQTT Publish", signalToSend.Topic + Environment.NewLine, ConsoleColor.Cyan);
        }

        #endregion Private-Functions

    }
}