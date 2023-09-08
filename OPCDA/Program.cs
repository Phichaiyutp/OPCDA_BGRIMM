using System;
using System.Timers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using MQTTnet.Protocol;
using System.Linq;
using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace OPCDA
{
    class Program
    {
        static Uri url = UrlBuilder.Build("OSISoft.OPCDA2.DA.1", "172.30.4.2");
        static string MQTT_HOST = "itrend.dev.itthirit.io";
        static string ClientId = "C#Program";
        static string Username = "itthirit";
        static string Password = "P@ssw0rd";
        static string[] PrefixTopic = new string[10];
        static System.Timers.Timer readTimer;
        static System.Timers.Timer publishTimer;
        static OpcDaItemValue[] values_WT, values_TR, values_PQM, values_SWG, values_RMUIn, values_RMUOut, values_OTHER , values_SINV , values_W_PQM , values_W_METER = null;
        static OpcDaGroup group_WT,group_TR,group_PQM,group_SWG,group_RMUIn,group_RMUOut,group_OTHER,group_SINV, group_W_PQM, group_W_METER;
        //static List<OpcDaItemDefinition> TR = OpcTags.ItemId_TR();
        //static List<OpcDaItemDefinition> SWG = OpcTags.ItemId_SWG();
        //static List<OpcDaItemDefinition> RMUIn = OpcTags.ItemId_RMUIn();
        //static List<OpcDaItemDefinition> RMUOut = OpcTags.ItemId_RMUOut();
        //static List<OpcDaItemDefinition> OTHER = OpcTags.ItemId_OTHER();
        static List<OpcDaItemDefinition> SINV = OpcTags.ItemId_SINV();
        static List<OpcDaItemDefinition> PQM = OpcTags.ItemId_PQM();
        static List<OpcDaItemDefinition> WT = OpcTags.ItemId_WT();
        static void Main(string[] args)
        {
            TitaniumAS.Opc.Client.Bootstrap.Initialize();
            using (var server = new OpcDaServer(url))
            {
                server.Connect();
                /*

                group_TR = server.AddGroup("TR");
                group_SWG = server.AddGroup("SWG");
                group_RMUIn = server.AddGroup("RMUIn");
                group_RMUOut = server.AddGroup("RMUOut");
                group_OTHER = server.AddGroup("OTHER");
                */
                group_SINV = server.AddGroup("SINV");
                group_PQM = server.AddGroup("PQM");
                group_WT = server.AddGroup("WT");
                /*

                group_TR.IsActive = true;
                group_SWG.IsActive = true;
                group_RMUIn.IsActive = true;
                group_RMUOut.IsActive = true;
                group_OTHER.IsActive = true;
                */
                group_SINV.IsActive = true;
                group_PQM.IsActive = true;
                group_WT.IsActive = true;
                /*

                OpcDaItemResult[] results_TR = group_TR.AddItems(TR.ToArray());
                OpcDaItemResult[] results_SWG = group_SWG.AddItems(SWG.ToArray());
                OpcDaItemResult[] results_RMUIn = group_RMUIn.AddItems(RMUIn.ToArray());
                OpcDaItemResult[] results_RMUOut = group_RMUOut.AddItems(RMUOut.ToArray());
                OpcDaItemResult[] results_OTHER = group_OTHER.AddItems(OTHER.ToArray());
                */
                OpcDaItemResult[] results_SINV = group_SINV.AddItems(SINV.ToArray());
                OpcDaItemResult[] results_PQM = group_PQM.AddItems(PQM.ToArray());
                OpcDaItemResult[] results_WT = group_WT.AddItems(WT.ToArray());
                /*

                PrintErrors(results_TR, "TR");
                PrintErrors(results_SWG, "SWG");
                PrintErrors(results_RMUIn, "RMUIn");
                PrintErrors(results_RMUOut, "RMUOut");
                PrintErrors(results_OTHER, "OTHER");
                */
                PrintErrors(results_SINV, "SINV");
                PrintErrors(results_PQM, "PQM");
                PrintErrors(results_WT, "WT");
                // Set up a timer to read OPC DA values every 5 seconds
                readTimer = new System.Timers.Timer(30000);
                readTimer.Elapsed += ReadTimerElapsed;
                readTimer.Start();

                // Set up a timer to publish MQTT values every 10 seconds
                publishTimer = new System.Timers.Timer(60000);
                publishTimer.Elapsed += PublishTimerElapsed;
                publishTimer.Start();

                while (true)
                {
                    // Run forever
                }
            }
        }

        private static void ReadTimerElapsed(object sender, ElapsedEventArgs e)
        {
            /*
            values_TR = group_TR.Read(group_TR.Items, OpcDaDataSource.Device);
            values_SWG = group_SWG.Read(group_SWG.Items, OpcDaDataSource.Device);
            values_RMUIn = group_RMUIn.Read(group_RMUIn.Items, OpcDaDataSource.Device);
            values_RMUOut = group_RMUOut.Read(group_RMUOut.Items, OpcDaDataSource.Device);
            values_OTHER = group_OTHER.Read(group_OTHER.Items, OpcDaDataSource.Device);
            */
            values_SINV = group_SINV.Read(group_SINV.Items, OpcDaDataSource.Device);
            values_PQM = group_PQM.Read(group_PQM.Items, OpcDaDataSource.Device);
            values_WT = group_WT.Read(group_WT.Items, OpcDaDataSource.Device);
            Console.WriteLine("Read OPC DA");
            /*
            for (int i = 0; i < values_SINV.Count(); i++)
            {
                Console.WriteLine("Item {0} Value {1} ",values_SINV[i].Item.ItemId, values_SINV[i].Value.ToString());
            }
            */

        }

        private static async void PublishTimerElapsed(object sender, ElapsedEventArgs e)
        {
            PrefixTopic[1] = "BGRIMM/SOLARFARM/INVERTER/";
            PrefixTopic[2] = "BGRIMM/SOLARFARM/PVS/";
            PrefixTopic[3] = "BGRIMM/SOLARFARM/PQM/";
            PrefixTopic[4] = "BGRIMM/SOLARFARM/ENVIROMENTAL/";

            /*
            if (values_TR != null)
            {
                await Task.Run(() => Publish_MQTT("BGRIMM/1/ENVIROMENTAL/", values_TR));
            }

            if (values_SWG != null)
            {
                await Task.Run(() => Publish_MQTT("BGRIMM/1/SWG/", values_SWG));
            }
            if (values_RMUIn != null)
            {
                await Task.Run(() => Publish_MQTT("BGRIMM/1/RMUIn/", values_RMUIn));
            }
            if (values_RMUOut != null)
            {
                await Task.Run(() => Publish_MQTT("BGRIMM/1/RMUOut/", values_RMUOut));
            }
            if (values_OTHER != null)
            {
                await Task.Run(() => Publish_MQTT("BGRIMM/1/OTHER/", values_OTHER));
            }
            */
            if (values_PQM != null)
            {
                await Task.Run(() => Publish_MQTT(PrefixTopic[1], values_PQM));
            }

            if (values_SINV != null)
            {
                await Task.Run(() => Publish_MQTT(PrefixTopic[2], values_SINV));
                await Task.Run(() => Publish_MQTT(PrefixTopic[3], values_SINV));
            }

            if (values_WT != null)
            {
                await Task.Run(() => Publish_MQTT(PrefixTopic[4], values_WT));
            }
        }



        static async Task Publish_MQTT(string topicPrefix, OpcDaItemValue[] values)
        {
            try
            {
                var mqttFactory = new MqttFactory();
                Dictionary<string, string>[] opcdaItems = new Dictionary<string, string>[100];
                string jsonPayload;
                string[] topic = new string[100];
                int deviceId;
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(MQTT_HOST)
                        .WithClientId(ClientId)
                        .WithCredentials(Username, Password)
                        .Build();
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    for (int i = 0; i < 100; i++)
                    {
                        opcdaItems[i] = new Dictionary<string, string>();
                    }
                        

                    for (int i = 0; i < values.Count(); i++)
                    {
                        string opcdaItem = GetTextAfterLastDot(values[i].Item.ItemId);
                        string prefixSINV1 = "TH00P01PVMPIS01\\S-TH-BBO-Modbus.Sub1.SINV";
                        string prefixSINV2 = "TH00P01PVMPIS01\\S-TH-BBO-Modbus.Sub2.SINV";
                        int startIndex = values[i].Item.ItemId.IndexOf("SINV") + "SINV".Length;
                        string subNumber = values[i].Item.ItemId.Substring(startIndex, 2);
                        switch (topicPrefix)
                        {
                            case "BGRIMM/SOLARFARM/INVERTER/": 
                                if (values[i].Item.ItemId.StartsWith(prefixSINV1))
                                {
                                    deviceId = int.Parse(subNumber);
                                    topic[deviceId] = topicPrefix + deviceId.ToString();
                                    opcdaItems[deviceId][opcdaItem] = values[i].Value.ToString();
                                }
                                if (values[i].Item.ItemId.StartsWith(prefixSINV2))
                                {
                                    deviceId = int.Parse(subNumber) + 38;
                                    topic[deviceId] = topicPrefix + deviceId.ToString();
                                    opcdaItems[deviceId][opcdaItem] = values[i].Value.ToString();
                                }
                            break;
                            case "BGRIMM/SOLARFARM/PVS/":
                                if (values[i].Item.ItemId.StartsWith(prefixSINV1))
                                {
                                    deviceId = int.Parse(subNumber) +76;
                                    topic[deviceId] = topicPrefix + deviceId.ToString();
                                    opcdaItems[deviceId][opcdaItem] = values[i].Value.ToString();
                                }
                                if (values[i].Item.ItemId.StartsWith(prefixSINV2))
                                {
                                    deviceId = int.Parse(subNumber) +114;
                                    topic[deviceId] = topicPrefix + deviceId.ToString();
                                    opcdaItems[deviceId][opcdaItem] = values[i].Value.ToString();
                                }
                                break;
                            case "BGRIMM/SOLARFARM/ENVIROMENTAL/":
                                string prefixWT1 = "TH00P01PVMPIS01\\S-TH-BBO-Modbus.Sub1.WT.";
                                if (values[i].Item.ItemId.StartsWith(prefixWT1))
                                {
                                    deviceId = 153;
                                    topic[deviceId] = topicPrefix + deviceId.ToString();
                                    opcdaItems[deviceId][opcdaItem] = values[i].Value.ToString();
                                }
                                break;
                            case "BGRIMM/SOLARFARM/PQM/":
                                string prefixPQM1 = "TH00P01PVMPIS01\\S-TH-BBO-Modbus.CtrlRoom.PQM.";
                                if (values[i].Item.ItemId.StartsWith(prefixPQM1))
                                {
                                    deviceId = 154;
                                    topic[deviceId] = topicPrefix + deviceId.ToString();
                                    opcdaItems[deviceId][opcdaItem] = values[i].Value.ToString();
                                }
                            break;


                        }
                    }

                    for (int i = 0; i < 100; i++)
                    {
                        var mqttItems = new Dictionary<string, string>();
                        DateTime dt = DateTime.Now;
                        if (opcdaItems[i] != null && topic[i] != null)
                        {
                            switch (topicPrefix)
                            {
                                case "BGRIMM/SOLARFARM/INVERTER/":
                                    mqttItems["ID"] = i.ToString();
                                    mqttItems["Date"] = dt.ToString("yyyy-MM-dd");
                                    mqttItems["Time"] = dt.ToString("HH:mm:ss"); ;
                                    mqttItems["AC_Voltage_L1"] = opcdaItems[i]["VoltageA"];
                                    mqttItems["AC_Voltage_L2"] = opcdaItems[i]["VoltageB"];
                                    mqttItems["AC_Voltage_L3"] = opcdaItems[i]["VoltageC"];
                                    mqttItems["AC_Voltage_L12"] = "";
                                    mqttItems["AC_Voltage_L23"] = "";
                                    mqttItems["AC_Voltage_L31"] = "";
                                    mqttItems["AC_Current_L1"] = opcdaItems[i]["CurrentA"];
                                    mqttItems["AC_Current_L2"] = opcdaItems[i]["CurrentB"];
                                    mqttItems["AC_Current_L3"] = opcdaItems[i]["CurrentC"];
                                    mqttItems["AC_Current_Total"] = "";
                                    mqttItems["AC_Power_Active_Total"] = opcdaItems[i]["TotalActivePower"];
                                    mqttItems["AC_Power_Apparent_Total"] = opcdaItems[i]["TotalAppearentPower"];
                                    mqttItems["AC_Power_Reactive_Total"] = opcdaItems[i]["TotalReactivePower"];
                                    mqttItems["Peak_Active_Power"] = "";
                                    mqttItems["Power_Factor"] = opcdaItems[i]["PowerFactor"];
                                    mqttItems["Frequency"] = opcdaItems[i]["GridFrequency"];
                                    mqttItems["Efficency"] = "";
                                    mqttItems["Internal_Temp"] = opcdaItems[i]["Internal_temperature"];
                                    mqttItems["Insulation_Resistance"] = "";
                                    mqttItems["Energy_Active_Total"] = opcdaItems[i]["TotalPowerYield"];
                                    mqttItems["Energy_Active_Daily"] = opcdaItems[i]["DailyPowerYield"];
                                    mqttItems["DC_Voltage_Total"] = opcdaItems[i]["DCVoltage"];
                                    mqttItems["DC_Current_Total"] = opcdaItems[i]["DCCurrent"];
                                    mqttItems["DC_Power_Total"] = opcdaItems[i]["TotalDCPower"];
                                    mqttItems["Operation_State_1"] = "";
                                    mqttItems["Operation_State_2"] = "";
                                    mqttItems["Operation_State_3"] = "";
                                    mqttItems["Alarm_Code_1"] = "";
                                    mqttItems["Alarm_Code_2"] = "";
                                    mqttItems["Alarm_Code_3"] = "";
                                    mqttItems["Device_Status"] = "";
                                break;

                                case "BGRIMM/SOLARFARM/PVS/":
                                    mqttItems["ID"] = i.ToString();
                                    mqttItems["Date"] = dt.ToString("yyyy-MM-dd");
                                    mqttItems["Time"] = dt.ToString("HH:mm:ss");
                                    mqttItems["Voltage_String1"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String1"] = opcdaItems[i]["StringCurrent1"];
                                    mqttItems["Power_String1"] = opcdaItems[i]["StringCurrent1"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String2"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String2"] = opcdaItems[i]["StringCurrent2"];
                                    mqttItems["Power_String2"] = opcdaItems[i]["StringCurrent2"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String3"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String3"] = opcdaItems[i]["StringCurrent3"];
                                    mqttItems["Power_String3"] = opcdaItems[i]["StringCurrent3"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String4"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String4"] = opcdaItems[i]["StringCurrent4"];
                                    mqttItems["Power_String4"] = opcdaItems[i]["StringCurrent4"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String5"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String5"] = opcdaItems[i]["StringCurrent5"];
                                    mqttItems["Power_String5"] = opcdaItems[i]["StringCurrent5"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String6"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String6"] = opcdaItems[i]["StringCurrent6"];
                                    mqttItems["Power_String6"] = opcdaItems[i]["StringCurrent6"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String7"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String7"] = opcdaItems[i]["StringCurrent7"];
                                    mqttItems["Power_String7"] = opcdaItems[i]["StringCurrent7"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String8"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String8"] = opcdaItems[i]["StringCurrent8"];
                                    mqttItems["Power_String8"] = opcdaItems[i]["StringCurrent8"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String9"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String9"] = opcdaItems[i]["StringCurrent9"];
                                    mqttItems["Power_String9"] = opcdaItems[i]["StringCurrent9"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String10"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String10"] = opcdaItems[i]["StringCurrent10"];
                                    mqttItems["Power_String10"] = opcdaItems[i]["StringCurrent10"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String11"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String11"] = opcdaItems[i]["StringCurrent11"];
                                    mqttItems["Power_String11"] = opcdaItems[i]["StringCurrent11"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String12"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String12"] = opcdaItems[i]["StringCurrent12"];
                                    mqttItems["Power_String12"] = opcdaItems[i]["StringCurrent12"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String13"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String13"] = opcdaItems[i]["StringCurrent13"];
                                    mqttItems["Power_String13"] = opcdaItems[i]["StringCurrent13"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Voltage_String14"] = opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_String14"] = opcdaItems[i]["StringCurrent14"];
                                    mqttItems["Power_String14"] = opcdaItems[i]["StringCurrent14"] + opcdaItems[i]["BusVoltage"];
                                    mqttItems["Current_Total"] = opcdaItems[i]["DCCurrent"];
                                    mqttItems["Power_Total"] = opcdaItems[i]["DCCurrent"] + opcdaItems[i]["BusVoltage"];
                                break;


                                case "BGRIMM/SOLARFARM/PQM/":
                                    mqttItems["ID"] = i.ToString();
                                    mqttItems["Date"] = dt.ToString("yyyy-MM-dd");
                                    mqttItems["Time"] = dt.ToString("HH:mm:ss"); ;
                                    mqttItems["Voltage_L1"] = opcdaItems[i]["VoltageLn1"];
                                    mqttItems["Voltage_L2"] = opcdaItems[i]["VoltageLn2"];
                                    mqttItems["Voltage_L3"] = opcdaItems[i]["VoltageLn3"];
                                    mqttItems["Voltage_L12"] = "";
                                    mqttItems["Voltage_L23"] = "";
                                    mqttItems["Voltage_L31"] = "";
                                    mqttItems["Current_L1"] = opcdaItems[i]["Current1"];
                                    mqttItems["Current_L2"] = opcdaItems[i]["Current2"];
                                    mqttItems["Current_L3"] = opcdaItems[i]["Current3"];
                                    mqttItems["Current_Total"] = "";
                                    mqttItems["Power_Active_L1"] = "";
                                    mqttItems["Power_Active_L2"] = "";
                                    mqttItems["Power_Active_L3"] = "";
                                    mqttItems["Power_Active_Total"] = opcdaItems[i]["TotalActivePower"];
                                    mqttItems["Power_Apparent_L1"] = "";
                                    mqttItems["Power_Apparent_L2"] = "";
                                    mqttItems["Power_Apparent_L3"] = "";
                                    mqttItems["Power_Apparent_Total"] = opcdaItems[i]["TotalApparentPower"];
                                    mqttItems["Power_Reactive_L1"] = "";
                                    mqttItems["Power_Reactive_L2"] = "";
                                    mqttItems["Power_Reactive_L3"] = "";
                                    mqttItems["Power_Reactive_Total"] = opcdaItems[i]["TotalReactivePower"];
                                    mqttItems["CosPhi_L1"] = opcdaItems[i]["VoltagePh1"];
                                    mqttItems["CosPhi_L2"] = opcdaItems[i]["VoltagePh2"];
                                    mqttItems["CosPhi_L3"] = opcdaItems[i]["VoltagePh3"];
                                    mqttItems["Power_Factor"] = opcdaItems[i]["PowerFactor"];
                                    mqttItems["Frequency"] = opcdaItems[i]["Frequency"];
                                    mqttItems["Energy_Active_Total"] = opcdaItems[i]["Energy"];
                                    mqttItems["Energy_Active_Import_L1"] = "";
                                    mqttItems["Energy_Active_Import_L2"] = "";
                                    mqttItems["Energy_Active_Import_L3"] = "";
                                    mqttItems["Energy_Active_Import_Total"] = "";
                                    mqttItems["Energy_Active_Export_L1"] = "";
                                    mqttItems["Energy_Active_Export_L2"] = "";
                                    mqttItems["Energy_Active_Export_L3"] = "";
                                    mqttItems["Energy_Active_Export_Total"] = "";
                                    mqttItems["Energy_Apparent_L1"] = "";
                                    mqttItems["Energy_Apparent_L2"] = "";
                                    mqttItems["Energy_Apparent_L3"] = "";
                                    mqttItems["Energy_Apparent_Total"] = "";
                                    mqttItems["Energy_Reactive_L1"] = "";
                                    mqttItems["Energy_Reactive_L2"] = "";
                                    mqttItems["Energy_Reactive_L3"] = "";
                                    mqttItems["Energy_Reactive_Total"] = opcdaItems[i]["EnergyReactive"];
                                break;
                                case "BGRIMM/SOLARFARM/ENVIROMENTAL/":
                                    mqttItems["ID"] = i.ToString();
                                    mqttItems["Date"] = dt.ToString("yyyy-MM-dd");
                                    mqttItems["Time"] = dt.ToString("HH:mm:ss"); ;
                                    mqttItems["Wind_Speed"] = opcdaItems[i]["WindSpeed"];
                                    mqttItems["Wind_Direction"] = opcdaItems[i]["WindDirection"];
                                    mqttItems["Module_Temperature_1"] = opcdaItems[i]["PVTemp1"];
                                    mqttItems["Module_Temperature_2"] = opcdaItems[i]["PVTemp2"];
                                    mqttItems["Ambient_Temperature"] = opcdaItems[i]["AmbTemp"];
                                    mqttItems["Ambient_Humidity"] = opcdaItems[i]["AmbHumidity"];
                                    mqttItems["Total_Irradiance_1"] = opcdaItems[i]["Irradiation1"];
                                    mqttItems["Total_Irradiance_2"] = opcdaItems[i]["Irradiation2"];
                                    mqttItems["Total_Irradiance_3"] = "";
                                    mqttItems["Daily_Irradiance_1"] = "";
                                    mqttItems["Daily_Irradiance_2"] = "";
                                    mqttItems["Daily_Irradiance_3"] = "";
                                    mqttItems["Daily_Irradiance_energy_1"] = "";
                                    mqttItems["Daily_Irradiance_energy_2"] = "";
                                break;

                            }
                            jsonPayload = JsonConvert.SerializeObject(mqttItems);
                            var message = new MqttApplicationMessageBuilder()
                                .WithTopic(topic[i])
                                .WithPayload(jsonPayload)
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                .WithRetainFlag(false)
                                .Build();
                            await mqttClient.PublishAsync(message);
                            //Console.WriteLine("MQTT {0} published json: {1}", topic[i], jsonPayload);
                            Console.WriteLine("MQTT {0} published", topic[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while publishing MQTT: {0}", ex.Message);
            }
        }
        static void PrintErrors(OpcDaItemResult[] results, string gname)
        {
            foreach (var result in results)
            {
                if (result.Error.Failed)
                    Console.WriteLine("Error adding item '{0}': {1}", gname, result.Error);
            }
        }
        static string GetTextAfterLastDot(string input) //TH00P01PVMPIS01\\S-TH-BBO-Modbus.Sub2.SINV34.VoltageA => VoltageA
        {
            int dotIndex = input.LastIndexOf('.');
            return dotIndex != -1 ? input.Substring(dotIndex + 1) : input;
        }
        static string GetTextAfterSecondDot(string input) ////TH00P01PVMPIS01\\S-TH-BBO-Modbus.Sub2.SINV34.VoltageA => SINV34.VoltageA
        {
            int firstDotIndex = input.IndexOf('.');
            if (firstDotIndex != -1)
            {
                int secondDotIndex = input.IndexOf('.', firstDotIndex + 1);
                if (secondDotIndex != -1)
                {
                    return input.Substring(secondDotIndex + 1);
                }
            }
            return input;
        }
    }
}
