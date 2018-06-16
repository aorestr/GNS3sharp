using System;
using GNS3_UNITY_API;

public class MicroCore : Guest{

    // In the name of the node: [label]Name
    public const string label = "MICROCORE";

    // Constructors
    public MicroCore() : base() {}
    public MicroCore(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
    public MicroCore(Node father) : base(father){}

    // Set an IP for the MicroCore
    public override string[] SetIP(string IP, string netmask = "255.255.255.0", int adapter_number = 0){

        // Reception varible as a string
        string[] in_txt = null;

        if(!Aux.IsIP(IP)) {
            Console.Error.WriteLine($"{IP} is not a valid IP");
        } else if(!Aux.IsNetmask(netmask)){ 
            Console.Error.WriteLine($"{netmask} is not a valid netmask");
        } else{
            Send($"sudo ifconfig eth{adapter_number.ToString()} {IP} netmask {netmask}");
            in_txt = Receive();
        }
        // Return the response
        return in_txt;

    }

}