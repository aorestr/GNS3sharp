using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GNS3sharp {
    public class VPC : Guest{

        // In the name of the node: [label]Name
        public const string label = "VPC";

        // Constructors
        public VPC() : base() {}
        public VPC(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public VPC(Node father) : base(father){}

        // Show arp table
        public string[] ShowArp(){

            // Reception variable as a string
            string[] in_txt = null;

            Send($"arp");
            in_txt = Receive();
            // Return the response
            return in_txt;

        }

        // Clear certain parameters
        public string[] Clear(string parameter){

            // Reception variable as a string
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

            // Reception variable as a string
            string[] in_txt = null;

            Send("DHCP");
            in_txt = Receive();
            // Return the response
            return in_txt;

        }

        // Show current config
        public string[] ShowConf(){

            // Reception variable as a string
            string[] in_txt = null;

            Send($"show");
            in_txt = Receive();
            // Return the response
            return in_txt;

        }

        // Set an IP for the VPC
        public override string[] SetIP(
            string IP, string netmask = "255.255.255.0", ushort adapter_number = 0, string gateway = null
            ){

            // Reception variable as a string
            string[] in_txt = null;
            // Netmask in CIDR notation
            short netmaskInt = Aux.NetmaskCIDR(netmask);

            if (netmaskInt == -1){
                Console.Error.WriteLine($"{netmask} is not a valid netmask");
            } else if(Aux.IsIP(IP) && gateway == null) {
                Send($"ip {IP}/{netmaskInt}");
                in_txt = Receive();
            } else if (Aux.IsIP(IP) && gateway != null) {
                if (Aux.IsIP(gateway)){
                    Send($"ip {IP}/{netmaskInt} {gateway}");
                } else{
                    Console.Error.WriteLine($"{gateway} is not a valid gateway");
                }
            } else {
                Console.Error.WriteLine($"{IP} is not a valid IP");
            }
            // Return the response
            return in_txt;

        }

        public string[] Ping(string IP, ushort count=5, uint msBetweenPackets = 500, string protocol="ICMP"){
            ushort protocolNum = 1;
            if (protocol.ToUpper().Equals("TCP"))
                protocolNum = 6;
            else if(protocol.ToUpper().Equals("UDP"))
                protocolNum = 17;
                
            return Ping(IP, $"-c {count.ToString()} -P {protocolNum.ToString()} -i {msBetweenPackets}");
        }

        // Check whether a ping went right or wrong. The results are showed different from
        // the average linux based node
        public override bool PingResult(string[] pingMessage){
            // We assume the result will be negative
            bool result = false;
            string[] lineSplit;
            foreach(string line in pingMessage){
                lineSplit = line.Split(' ');
                // Check if any line matches with "%d bytes from ..."
                if (
                    Regex.IsMatch(lineSplit[0].Trim(), @"\d+") &&
                    Regex.IsMatch(lineSplit[1].Trim(), @"bytes") && 
                    Regex.IsMatch(lineSplit[2].Trim(), @"from")
                    ){
                    result = true;
                    break;
                }
            }
            return result;
        }

        // Show the route to a certain IP
        public string[] Trace(string IP){

            // Reception variable as a string
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
}