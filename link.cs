using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GNS3sharp {
    /// <summary>
    /// This class represents a link of a GNS3 project
    /// <remarks>
    /// This class can give you information about a link that is attached
    /// to a node as well as edit it and change its parameters
    /// </remarks>
    /// </summary>
    public class Link{

        private string id;
        /// <summary>
        /// ID the link has implicitly
        /// </summary>
        /// <value>String ID</value>
        public string ID { get => id; }

        private Node[] nodes;
        /// <summary>
        /// List of nodes that the link is connecting
        /// </summary>
        /// <value>List of <c>Node</value>
        public Node[] Nodes { get  => nodes; }

        private int frequencyDrop;
        /// <summary>
        /// Parameter that measure the frequency drop of the link
        /// </summary>
        /// <value>Throughput as an integer</value>
        public int FrequencyDrop { 
            get => frequencyDrop;
            private set {
                if (value < -1) frequencyDrop = 0;
                else frequencyDrop = value;
            }
        }

        private int packetLoss;
        /// <summary>
        /// Parameter that measure the packet loss of the link
        /// </summary>
        /// <value>Percentage as an integer</value>
        public int PacketLoss { 
            get  => packetLoss;
            private set {
                if (value < 0) packetLoss = 0;
                else if (value > 100) packetLoss = 100;
                else packetLoss = value;
            }
        }
        
        private int latency;
        /// <summary>
        /// Parameter that measure the latency of the link
        /// </summary>
        /// <value>ms as an integer</value>
        public int Latency {
            get => latency;
            private set {
                if (value < 0) latency = 0;
                else latency = value;
            }
        }

        private int jitter;
        /// <summary>
        /// Parameter that measure the jitter of the link
        /// </summary>
        /// <value>ms as an integer</value>
        public int Jitter {
            get => jitter;
            private set {
                if (value < 0) jitter = 0;
                else jitter = value;
            }
        }

        private int corrupt;
        /// <summary>
        /// Parameter that measure the corruption of the link
        /// </summary>
        /// <value>Percentage as an integer</value>
        public int Corrupt {
            get {return corrupt;}
            private set {
                if (value < 0) corrupt = 0;
                else if (value > 100) corrupt = 100;
                else corrupt = value;
            }
        }

        /// <summary>
        /// Information about the server
        /// </summary>
        /// <value>Keys: host, port and projectID</value>
        private Dictionary<string,string> serverInfo;

        /// <summary>
        /// HTTP client used to interact with the REST API
        /// </summary>
        private readonly HttpClient HTTPclient;

        ///////////////////////////// Constructors ////////////////////////////////////////////

        /// <summary>
        /// Constructs the object with all filters equal to 0
        /// </summary>
        /// <param name="_id">ID the link has implicitly</param>
        /// <param name="_nodes">Array of nodes that the link is connecting</param>
        /// <param name="_serverInfo">Information about the server (host, port and projectID)</param>
        /// <param name="_HTTPclient">HTTP client used by the GNS3sharp object which creates this link</param>
        internal Link(string _id, Node[] _nodes, Dictionary<string, string> _serverInfo, HttpClient _HTTPclient){
            id = _id; nodes = _nodes; serverInfo = _serverInfo; HTTPclient = _HTTPclient;
            frequencyDrop = 0; packetLoss = 0;
            latency = 0; jitter = 0; corrupt = 0;
        }

        /// <summary>
        /// Constructs the object with some filter different from 0
        /// </summary>
        /// <param name="_id">ID the link has implicitly</param>
        /// <param name="_nodes">Array of nodes that the link is connecting</param>
        /// <param name="_serverInfo">Information about the server (host, port and projectID)</param>
        /// <param name="_HTTPclient">HTTP client used by the GNS3sharp object which creates this link</param>
        /// <param name="_frequencyDrop">Parameter that measure the frequency drop of the link</param>
        /// <param name="_packetLoss">Parameter that measure the packet loss of the link</param>
        /// <param name="_latency">Parameter that measure the latency of the link</param>
        /// <param name="_jitter">Parameter that measure the jitter of the link</param>
        /// <param name="_corrupt">Parameter that measure the corruption of the link</param>
        internal Link(
            string _id, Node[] _nodes, Dictionary<string, string> _serverInfo, HttpClient _HTTPclient,
            int _frequencyDrop=0, int _packetLoss=0, int _latency=0, int _jitter=0, int _corrupt=0
            ){
            id = _id; nodes = _nodes; serverInfo = _serverInfo; HTTPclient = _HTTPclient;
            frequencyDrop = _frequencyDrop; packetLoss = _packetLoss;
            latency = _latency; jitter = _jitter; corrupt = _corrupt;
        }

        ///////////////////////////////// Methods ////////////////////////////////////////////
        
        /// <summary>
        /// Edit some filter of a link
        /// </summary>
        /// <param name="_frequencyDrop">Parameter that measure the frequency drop of the link</param>
        /// <param name="_packetLoss">Parameter that measure the packet loss of the link</param>
        /// <param name="_latency">Parameter that measure the latency of the link</param>
        /// <param name="_jitter">Parameter that measure the jitter of the link</param>
        /// <param name="_corrupt">Parameter that measure the corruption of the link</param>
        /// <returns>True if everything went right, False otherwise</returns>
        public bool EditLink(
            int _frequencyDrop=-10, int _packetLoss=-10,
            int _latency=-10, int _jitter=-10, int _corrupt=-10
            ){

            // Return variable
            bool linkEdited;

            // Check the filters that is going to modify
            bool filterChanges = true;
            if (_frequencyDrop != -10){ filterChanges = true; FrequencyDrop = _frequencyDrop; }
            if (_packetLoss != -10){ filterChanges = true; PacketLoss = _packetLoss; }
            if (_latency != -10){ filterChanges = true; Latency = _latency; }
            if (_jitter != -10){ filterChanges = true; Jitter = _jitter; }
            if (_corrupt != -10){ filterChanges = true; Corrupt = _corrupt; }

            if (!filterChanges){
                linkEdited = false;
            } else{
                // Content to send
                Dictionary<string, int[]> filtersInfo = new Dictionary<string, int[]>{
                    {"frequency_drop", new int[1]{this.frequencyDrop}},
                    {"packet_loss", new int[1]{this.packetLoss}},
                    {"delay", new int[2]{ this.latency, this.jitter }},
                    {"corrupt", new int[1]{this.corrupt}}
                };
                // URI where the data is going to be sent
                string URL = (
                    $"http://{serverInfo["host"]}:{serverInfo["port"]}/v2/projects/{serverInfo["projectID"]}/links/{id}"
                );

                try {

                    // Pack the content we will send
                    string content = JsonConvert.SerializeObject(new Dictionary<string, Dictionary<string, int[]>> { 
                        { "filters" , filtersInfo }
                    });
                    ByteArrayContent byteContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content));
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // Send the content and check if everything is alright
                    linkEdited = HTTPclient.PutAsync($"{URL}", byteContent).Result.IsSuccessStatusCode;

                } catch(JsonSerializationException err){
                    Console.Error.WriteLine("Impossible to serialize the JSON to send it to the API: {0}", err.Message);
                    linkEdited = false;
                } catch(HttpRequestException err){
                    Console.Error.WriteLine("Some problem occured with the HTTP connection: {0}", err.Message);
                    linkEdited = false;
                } catch(Exception err){
                    Console.Error.WriteLine("Impossible to edit the link: {0}", err.Message);
                    linkEdited = false;
                }
            }
            return linkEdited;
        }
    }
}