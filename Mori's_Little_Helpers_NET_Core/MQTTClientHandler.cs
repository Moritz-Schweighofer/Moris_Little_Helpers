using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace Schweigm_NETCore_Helpers
{
    public class MqttClientHandler
    {

        #region Public-Member

        /// <summary>
        /// List of all CA Signals which shall be updated in the next Send Cycle
        /// Object: Signal to Update / String: Topic
        /// </summary>
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<(object,string)> SignalsToUpdate { get; set; } = new List<(object, string)>();

        /// <summary>
        /// Lock to lock the CA_SignalsToUpdate List so no Concurrency Issues occur
        /// </summary>
        public object SignalsToUpdateLock { get; } = new object();

        #endregion Public-Member

        #region Private-Member

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
        }

        /// <summary>
        /// Disconnect from the Broker
        /// </summary>
        public void Disconnect()
        {
            if (!_mqttClient.IsConnected) return;
            _mqttClient.DisconnectAsync();
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

                    foreach (var (signalToSend, topic) in SignalsToUpdate)
                    {
                        SendCaValue(signalToSend, topic);
                    }

                    SignalsToUpdate.Clear();
                }
            }
        }

        /// <summary>
        /// Sends the Numeric Signal to the CA
        /// </summary>
        /// <param name="signalToSend"></param>
        /// <param name="topic"></param>
        private void SendCaValue(object signalToSend, string topic)
        {
            var json = JsonConvert.SerializeObject(signalToSend, Formatting.Indented);
            var msg = Encoding.ASCII.GetBytes(json);
            _mqttClient.PublishAsync(topic, msg);

            CoolConsoleOutput.Write("MQTT Publish", topic + Environment.NewLine, ConsoleColor.Cyan);
        }

        #endregion Private-Functions

    }
}