using GNS3_UNITY_API;

public class MicroCore : Guest{

    // In the name of the node: [label]Name
    public const string label = "MICROCORE";

    public MicroCore() : base() {}
    public MicroCore(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
    public MicroCore(Node father) : base(father){}

}