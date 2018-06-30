using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using GNS3_UNITY_API;

/*
 Structure that handles every link
 */
public class Link{
    // ID
    private string id; public string ID { get => id; }
    // Nodes the link connects
    private Node[] nodes; public Node[] Nodes { get  => nodes; }
    // Parameters of the link
    private int frequencyDrop;              // th
    public int FrequencyDrop { 
        get => frequencyDrop;
        private set {
            if (value < -1) frequencyDrop = 0;
            else frequencyDrop = value;
        }
    }
    private int packetLoss;                 // %
    public int PacketLoss { 
        get  => packetLoss;
        private set {
            if (value < 0) packetLoss = 0;
            else if (value > 100) packetLoss = 100;
            else packetLoss = value;
        }
    }    
    private int latency;                    // ms
    public int Latency {
        get => latency;
        private set {
            if (value < 0) latency = 0;
            else latency = value;
        }
    }
    private int jitter;                     // ms
    public int Jitter {
        get => jitter;
        private set {
            if (value < 0) jitter = 0;
            else jitter = value;
        }
    }
    private int corrupt;                    // %
    public int Corrupt {
        get {return corrupt;}
        private set {
            if (value < 0) corrupt = 0;
            else if (value > 100) corrupt = 100;
            else corrupt = value;
        }
    }
    // Information about the server (host, port and projectID)
    private Dictionary<string,string> serverInfo;
    // HTTP client used to interact with the REST API
    private readonly HttpClient HTTPclient;

    ///////////////////////////// Constructors ////////////////////////////////////////////

    // Constructor with all filters different from 0
    public Link(string _id, Node[] _nodes, Dictionary<string, string> _serverInfo, HttpClient _HTTPclient){
        id = _id; nodes = _nodes; serverInfo = _serverInfo; HTTPclient = _HTTPclient;
        frequencyDrop = 0; packetLoss = 0;
        latency = 0; jitter = 0; corrupt = 0;
    }
    // Constructor with any filter different from 0
    public Link(
        string _id, Node[] _nodes, Dictionary<string, string> _serverInfo, HttpClient _HTTPclient,
        int _frequencyDrop=0, int _packetLoss=0, int _latency=0, int _jitter=0, int _corrupt=0
        ){
        id = _id; nodes = _nodes; serverInfo = _serverInfo; HTTPclient = _HTTPclient;
        frequencyDrop = _frequencyDrop; packetLoss = _packetLoss;
        latency = _latency; jitter = _jitter; corrupt = _corrupt;
    }

    ///////////////////////////////// Methods ////////////////////////////////////////////
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
            // URL where send the data
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