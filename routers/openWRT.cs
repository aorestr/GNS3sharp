using GNS3_UNITY_API;

public class OpenWRT : Router{

    // Label to determine what device is
    public const string label = "OPENWRT";

    // Constructors
    public OpenWRT() : base() {}
    public OpenWRT(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
    public OpenWRT(Node father) : base(father){}

}