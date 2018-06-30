using System.Collections.Generic;
using GNS3_UNITY_API;

public abstract class Switch : Node{

    // Constructors
    public Switch() : base() {}
    public Switch(string _consoleHost, ushort _port, string _name, string _id,
        Dictionary<string,ushort>[] _ports) : 
        base(_consoleHost, _port, _name, _id, _ports){}
    public Switch(Node clone) : base(clone){}
}