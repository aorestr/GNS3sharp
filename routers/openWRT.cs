using System;
using System.Collections.Generic;

namespace GNS3sharp {
    public class OpenWRT : Router{

        // Label to determine what device is
        public const string label = "OPENWRT";

        // Constructors
        public OpenWRT() : base() {}
        public OpenWRT(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public OpenWRT(Node father) : base(father){}

        // This routers' terminal needs to be initialize with a \n
        private void ActivateTerminal(){
            Send("");
        }

        // Activate an interface of the router
        public override string[] ActivateInterface(
            string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0
            ){
            return ChangeInterfaceStatus("up", IP, netmask, interfaceNumber);
        }

        // Dectivate an interface of the router
        public override string[] DeactivateInterface(
            string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0
            ){
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
                ActivateTerminal();
                Send($"ifconfig eth{interfaceNumber.ToString()} {IP} netmask {netmask} {status}");
                in_txt = Receive();
            }

            // Return the response
            return in_txt;
        }

        // Set a route to a cerrtain destination through a gateway
        public override string[] SetRoute(string destination, string gateway, string netmask = "255.255.255.0"){
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

        // (Re)Start the firewall
        public string[] EnableFirewall(){
            return ChangeFirewallStatus("start");
        }

        // Stop the firewall
        public string[] DisableFirewall(){
            return ChangeFirewallStatus("stop");
        }

        // Change firewall status to start or stop
        private string[] ChangeFirewallStatus(string newStatus){
            // Reception variable as a string
            string[] in_txt = null;

            ActivateTerminal();
            Send($"/etc/init.d/firewall {newStatus}");
            in_txt = Receive();

            // Return the response
            return in_txt;
        }

    }
}