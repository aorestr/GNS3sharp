using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using GNS3_UNITY_API;

/*
 Structure that handles every link
 */
public class Link{
    // ID
    private string id; public string ID { get {return id;} }
    // Nodes the link connects
    private Node[] nodes; public Node[] Nodes { get {return nodes;} }
    // Parameters of the link
    private int frequencyDrop;              // th
    public int FrequencyDrop { 
        get {return frequencyDrop;}
        set {
            if (value < -1) frequencyDrop = 0;
            else frequencyDrop = value;
        }
    }
    private int packetLoss;                 // %
    public int PacketLoss { 
        get {return packetLoss;}
        set {
            if (value < 0) packetLoss = 0;
            else if (value > 100) packetLoss = 100;
            else packetLoss = value;
        }
    }    
    private int latency;                    // ms
    public int Latency {
        get {return latency;}
        set {
            if (value < 0) latency = 0;
            else latency = value;
        }
    }
    private int jitter;                     // ms
    public int Jitter {
        get {return jitter;}
        set {
            if (value < 0) jitter = 0;
            else jitter = value;
        }
    }
    private int corrupt;                    // %
    public int Corrupt {
        get {return corrupt;}
        set {
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

}