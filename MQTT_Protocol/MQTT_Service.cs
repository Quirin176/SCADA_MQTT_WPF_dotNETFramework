using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace MQTT_Protocol
{
    public delegate void CatchExceptionDelegate(Exception ex);
    public class MQTT_Service
    {
        private static List<Building> Buildings;
        private static Dictionary<string, MQTT_Client> Clients = null;
        //private static int COUNTER = 0;
        private static Thread[] threads;
        public static bool IsConnected = false;

        public static string server = "localhost";

        //public static string server = "192.168.0.10";

        //public static string server = "test.mosquitto.org";
        //public static string server = "broker.hivemq.com";
        //public static string server = "broker.emqx.io";

        //public static string server = "eu1.cloud.thethings.network";

        public static void InitializeService(List<Building> buildings)
        {
            try
            {
                Buildings = buildings;
                Clients = new Dictionary<string, MQTT_Client>();
                if (Buildings == null) return;

                foreach (Building building in Buildings)
                {
                    IMQTT_Client mqtt = null;

                    //DIEthernet die = (DIEthernet)building;
                    //mqtt = new MQTT_Client(die.IPAddress);

                    mqtt = new MQTT_Client(server);

                    mqtt.ConnectionClosed += (sender, args) =>
                    {
                        IsConnected = false;
                        AttemptReconnect(mqtt, building);
                    };

                    Clients.Add(building.BuildingName, (MQTT_Client)mqtt);

                    foreach (Floor floor in building.Floors)
                    {
                        foreach (Room room in floor.Rooms)
                        {
                            foreach (Device device in room.Devices)
                            {
                                foreach (Tag tag in device.Tags)
                                {
                                    MQTT_TagCollection.Tags.Add(string.Format("{0}/{1}/{2}/{3}/{4}", building.BuildingName, floor.FloorName, room.RoomName, device.DeviceName, tag.TagName), tag);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Start()
        {
            try
            {
                IsConnected = true;
                //Console.WriteLine(string.Format("STARTED: {0}", ++COUNTER));
                threads = new Thread[Buildings.Count];
                if (threads == null) throw new NullReferenceException("No Data");

                for (int i = 0; i < Buildings.Count; i++)
                {
                    threads[i] = new Thread((chParam) =>
                    {
                        IMQTT_Client mqtt = null;
                        Building building = (Building)chParam;
                        mqtt = Clients[building.BuildingName];

                        ConnectToBroker(mqtt, building);

                        if (IsConnected)
                        {
                            SubscribeToTopics(building, mqtt);
                        }
                    });
                    threads[i].IsBackground = true;
                    threads[i].Start(Buildings[i]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SubscribeToTopics(Building building, IMQTT_Client mqtt)
        {
            // Collect topics and QoS levels
            List<string> topicsList = new List<string>();
            List<byte> qosList = new List<byte>();

            foreach (Floor floor in building.Floors)
            {
                foreach (Room room in floor.Rooms)
                {
                    foreach (Device device in room.Devices)
                    {
                        foreach (Tag tag in device.Tags)
                        {
                            topicsList.Add(tag.Topic);
                            qosList.Add(tag.QoS);

                            mqtt.MQTTMsgPublishReceived += (sender, e) =>
                            {
                                if (e.Topic == tag.Topic)
                                {
                                    tag.Value = Encoding.UTF8.GetString(e.Message);
                                }
                            };
                        }
                    }
                }
            }

            // Subscribe to all collected topics with corresponding QoS levels
            mqtt.Subscribe(topicsList.ToArray(), qosList.ToArray());
        }

        private static void ConnectToBroker(IMQTT_Client mqtt, Building building)
        {
            try
            {
                IsConnected = true;

                //mqtt.Connect("myApp", "voquangqui176@gmail.com", "Voquangqui17062002");
                mqtt.Connect("myApp", "hcmut-28032024@ttn", "NNSXS.5H42SGEVFVU6JIQZB7BBJQIAIX753QQ37BODQJA.GWZ6V7IXDAYI3OZL46Z4XEA377QNL4MYNRKT7452CIY263TERDZA", true, 30);
                //mqtt.Connect("myApp", "hcmut-28032024@ttn", "NNSXS.5H42SGEVFVU6JIQZB7BBJQIAIX753QQ37BODQJA.GWZ6V7IXDAYI3OZL46Z4XEA377QNL4MYNRKT7452CIY263TERDZA", false, 30);

            }
            catch (Exception ex)
            {
                IsConnected = false;
                Console.WriteLine($"Failed to connect to broker: {ex.Message}");
            }
        }

        private static void AttemptReconnect(IMQTT_Client mqtt, Building building)
        {
            Task.Run(() =>
            {
                while (!IsConnected)
                {
                    try
                    {
                        Console.WriteLine($"Attempting to reconnect to broker for building: {building.BuildingName}");
                        mqtt.Connect("myApp", "hcmut-28032024@ttn", "NNSXS.5H42SGEVFVU6JIQZB7BBJQIAIX753QQ37BODQJA.GWZ6V7IXDAYI3OZL46Z4XEA377QNL4MYNRKT7452CIY263TERDZA", true, 30);
                        IsConnected = true;

                        // Resubscribe to topics after reconnecting
                        SubscribeToTopics(building, mqtt);
                        Console.WriteLine($"Reconnected to broker for building: {building.BuildingName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Reconnection attempt failed: {ex.Message}");
                        Thread.Sleep(5000); // Wait 5 seconds before trying again
                    }
                }
            });
        }

        public static CatchExceptionDelegate eventCatchExceptionDelegate = null;

        public static void PublishToTopic(string topic, dynamic message)
        {
            try
            {
                // Split the topic
                string[] ary = topic.Split('/');
                string buildingName = ary[0];
                string floorName = ary[1];
                string roomName = ary[2];
                string deviceName = ary[3];
                string tagName = ary[4];
                string messageString = Convert.ToString(message);

                // Full tag key
                string fullTagKey = string.Format("{0}/{1}/{2}/{3}/{4}", buildingName, floorName, roomName, deviceName, tagName);

                // Find the MQTT Client
                IMQTT_Client mqtt = null;
                foreach (Building building in Buildings)
                {
                    if (building.BuildingName.Equals(buildingName))
                    {
                        mqtt = Clients[building.BuildingName];
                        if (mqtt == null) return;

                        foreach (Floor floor in building.Floors)
                        {
                            if (floor.FloorName.Equals(floorName))
                            {
                                foreach (Room room in floor.Rooms)
                                {
                                    if (room.RoomName.Equals(roomName))
                                    {
                                        foreach (Device device in room.Devices)
                                        {
                                            if (device.DeviceName.Equals(deviceName))
                                            {
                                                foreach (Tag tag in device.Tags)
                                                {
                                                    if (tag.TagName.Equals(tagName))
                                                    {
                                                        lock (mqtt)
                                                        {
                                                            mqtt.Publish(topic, Encoding.UTF8.GetBytes(messageString), tag.QoS, tag.Retain);
                                                        }
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                throw new Exception("Tag not found for the specified topic");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Stop()
        {
            IsConnected = false;

            foreach (var mqtt in Clients.Values)
            {
                mqtt.Disconnect();
            }
        }
    }
}
