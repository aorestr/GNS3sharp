using System;
using GNS3_UNITY_API;

public class VPC : Guest{

    public VPC(Guest father) : base(father){}

    // Show current config
    public (string[] in_txt, int numBytes) ShowConf(){
        // Reception varible as a string
        string[] in_txt = null;
        // Number of bytes received
        int numBytes = 0;

        Send($"show");
        (in_txt, numBytes) = Receive();
        // Return the response
        return (in_txt, numBytes);
    }

    // Set an IP for the VPC
    public (string[] in_txt, int numBytes) SetIP(string IP){
        // Reception varible as a string
        string[] in_txt = null;
        // Number of bytes received
        int numBytes = 0;

        if(Aux.IsIP(IP)) {
            Send($"ip {IP}");
            (in_txt, numBytes) = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }
        // Return the response
        return (in_txt, numBytes);
    }

    // Use DHCP for assigning the IP
    public (string[] in_txt, int numBytes) DHCP(){
        // Reception varible as a string
        string[] in_txt = null;
        // Number of bytes received
        int numBytes = 0;

        Send("DHCP");
        (in_txt, numBytes) = Receive();

        // Return the response
        return (in_txt, numBytes);
    }

    // Send ping to a certain IP
    public (string[] in_txt, int numBytes) Ping(string IP){
        // Reception varible as a string
        string[] in_txt = null;
        // Number of bytes received
        int numBytes = 0;

        if(Aux.IsIP($"ping {IP}")) {
            Send(IP);
            (in_txt, numBytes) = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }
        // Return the response
        return (in_txt, numBytes);
    }

    // Show the route to a certain IP
    public (string[] in_txt, int numBytes) Trace(string IP){
        // Reception varible as a string
        string[] in_txt = null;
        // Number of bytes received
        int numBytes = 0;

        if(Aux.IsIP($"trace {IP}")) {
            Send(IP);
            (in_txt, numBytes) = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }
        // Return the response
        return (in_txt, numBytes);
    }
}