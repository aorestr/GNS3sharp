using System;
using System.Collections.Generic;
using GNS3_UNITY_API;

public class OpenWRT : Router{

    // Label to determine what device is
    public const string label = "OPENWRT";

    // Constructors
    public OpenWRT() : base() {}
    public OpenWRT(string _consoleHost, ushort _port, string _name, string _id,
        Dictionary<string,ushort>[] _ports) : 
        base(_consoleHost, _port, _name, _id, _ports){}
    public OpenWRT(Node father) : base(father){}

    // This routers' terminal needs to be initialize with a \n
    private void ActivateTerminal(){
        Send("");
    }

    // Activate an interface of the router
    public string[] ActivateInterface(
        string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0
        ){
        ActivateTerminal();
        return ChangeInterfaceStatus("up", IP, netmask, interfaceNumber);
    }

    // Dectivate an interface of the router
    public string[] DeactivateInterface(
        string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0
        ){
        ActivateTerminal();
        return ChangeInterfaceStatus("down", IP, netmask, interfaceNumber);
    }

    // Change the status of an interface to 'up' or 'down'
    private string[] ChangeInterfaceStatus(
        string status, string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0
        ){
        // Reception variable as a string
        string[] in_txt = null;

        if(!Aux.IsIP(IP)) {
            Console.Error.WriteLine($"{IP} is not a valid IP");
        } else if(!Aux.IsNetmask(netmask)){ 
            Console.Error.WriteLine($"{netmask} is not a valid netmask");
        } else{
            Send($"ifconfig eth{interfaceNumber.ToString()} {IP} netmask {netmask} {status}");
            in_txt = Receive();
        }

        // Return the response
        return in_txt;
    }

    // Set a route to a cerrtain destination through a gateway
    public string[] SetRoute(string destination, string gateway, string netmask = "255.255.255.0"){
        // Reception variable as a string
        string[] in_txt = null;

        ActivateTerminal();
        if (!Aux.IsIP(destination))
            Console.Error.WriteLine($"{destination} is not a valid IP");
        else if (!Aux.IsIP(gateway))
            Console.Error.WriteLine($"{gateway} is not a valid gateway");
        else{
            Send($"route add -net {destination} netmask {netmask} gw {gateway}");
            in_txt = Receive();
        }

        // Return the response
        return in_txt;
    }

}