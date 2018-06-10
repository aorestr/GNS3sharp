using System;
using GNS3_UNITY_API;

public class VPC : Guest{

    public VPC(Guest father) : base(father){}

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
    public string[] SetIP(string IP){

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

    // Use DHCP for assigning the IP
    public string[] DHCP(){

        // Reception varible as a string
        string[] in_txt = null;

        Send("DHCP");
        in_txt = Receive();
        // Return the response
        return in_txt;

    }

    // Send ping to a certain IP
    public string[] Ping(string IP){

        // Reception varible as a string
        string[] in_txt = null;

        if(Aux.IsIP(IP)) {
            Send($"ping {IP}");
            in_txt = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }
        // Return the response
        return in_txt;

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