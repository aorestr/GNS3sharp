using System.Collections.Generic;
using GNS3_UNITY_API;

public class OpenvSwitch : Switch{

    // In the name of the node: [label]Name
    public const string label = "OVS";

    // Constructors
    public OpenvSwitch() : base() {}
    public OpenvSwitch(string _consoleHost, ushort _port, string _name, string _id,
        Dictionary<string,ushort>[] _ports) : 
        base(_consoleHost, _port, _name, _id, _ports){}
    public OpenvSwitch(Node father) : base(father){}

}