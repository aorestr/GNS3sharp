using System.Collections.Generic;
using GNS3_UNITY_API;

public class EthernetSwitch : Switch{

    // In the name of the node: [label]Name
    public const string label = "ETHSW";

    // Constructors
    public EthernetSwitch() : base() {}
    public EthernetSwitch(string _consoleHost, ushort _port, string _name, string _id,
        Dictionary<string,ushort>[] _ports) : 
        base(_consoleHost, _port, _name, _id, _ports){}
    public EthernetSwitch(Node father) : base(father){}

}