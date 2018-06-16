using System;
using System.Linq;
using GNS3_UNITY_API;

public class VPC : Guest{

    // In the name of the node: [label]Name
    public const string label = "VPC";

    // Constructors
    public VPC() : base() {}
    public VPC(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
    public VPC(Node father) : base(father){}

    // Show arp table
    public string[] ShowArp(){

        // Reception varible as a string
        string[] in_txt = null;

        Send($"arp");
        in_txt = Receive();
        // Return the response
        return in_txt;

    }

    // Clear certain parameters
    public string[] Clear(string parameter){

        // Reception varible as a string
        string[] in_txt = null;

        string[] validParamters = {
            "ip","ipv6","arp","neighbor","hist"
        };

        if (validParamters.Any(parameter.ToLowerInvariant().Contains)) {
            Send($"clear {parameter}");
            in_txt = Receive();
        } else{
            Console.Error.WriteLine("Invalid parameter to send");
        }
        // Return the response
        return in_txt;

    }

    // Use DHCP for assigning the IP
    public string[] DHCP(){

        // Reception varible as a string
        string[] in_txt = null;

        Send("DHCP");
        in_txt = Receive();
        // Return the response
        return in_txt;

    }

    // Show current config
    public string[] ShowConf(){

        // Reception varible as a string
        string[] in_txt = null;

        Send($"show");
        in_txt = Receive();
        // Return the response
        return in_txt;

    }

    // Set an IP for the VPC
    public override string[] SetIP(string IP, string netmask = null, int adapter_number = 0){

        // Reception varible as a string
        string[] in_txt = null;

        if(Aux.IsIP(IP)) {
            Send($"ip {IP}");
            in_txt = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }
        // Return the response
        return in_txt;

    }

    // Send ping to a certain IP
    public (string[], bool reached) Ping(string IP){

        // Reception varible as a string
        string[] in_txt = null;

        if(Aux.IsIP(IP)) {
            Send($"ping {IP}");
            in_txt = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }

        /*
        ///////////////
        TO DO
         */
        // Check if the ping went right
        bool reached = false;
        //////////////

        // Return the response
        return (in_txt, reached);

    }

    // Show the route to a certain IP
    public string[] Trace(string IP){

        // Reception varible as a string
        string[] in_txt = null;

        if(Aux.IsIP(IP)) {
            Send($"trace {IP}");
            in_txt = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }
        // Return the response
        return in_txt;

    }


}