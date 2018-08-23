using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GNS3sharp {

    /// <summary>
    /// Representation of a VPC type of node
    /// <remarks>
    /// Define methods that are only available for this appliance
    /// </remarks>
    /// </summary>
    public class VPC : Guest{

        private const string label = "VPC";
        /// <summary>
        /// Label you must set in the name of the node at the GNS3 project
        /// <para>Name of the node must look like "[VPC]Name"</para>
        /// </summary>
        /// <value>Label as a string</value>
        public static string Label { get => label; }

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        internal VPC() : base() {}

        /// <summary>
        /// Constructor for any kind of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        internal VPC(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}

        /// <summary>
        /// Constructor that replicates a guest from another node
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public VPC(Node clone) : base(clone){}

        /// <summary>
        /// Show arp table
        /// </summary>
        /// <returns>Received message as an array of strings</returns>
        public string[] ShowArp(){

            // Reception variable as a string
            string[] in_txt = null;

            Send($"arp");
            in_txt = Receive();
            // Return the response
            return in_txt;

        }

        /// <summary>
        /// Clear a certain type of parameters
        /// </summary>
        /// <param name="parameter">Valid parameters: "ip","ipv6","arp","neighbor","hist"</param>
        /// <returns>Received message as an array of strings</returns>
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

        /// <summary>
        /// Use DHCP as the method for assigning the IP in a network
        /// </summary>
        /// <returns>Received message as an array of strings</returns>
        public string[] DHCP(){

            // Reception variable as a string
            string[] in_txt = null;

            Send("DHCP");
            in_txt = Receive();
            // Return the response
            return in_txt;

        }

        /// <summary>
        /// Show current config
        /// </summary>
        /// <returns>Received message as an array of strings</returns>
        public string[] ShowConf(){

            // Reception variable as a string
            string[] in_txt = null;

            Send($"show");
            in_txt = Receive();
            // Return the response
            return in_txt;

        }

        /// <summary>
        /// Set an IP for an interface of the device
        /// </summary>
        /// <param name="IP">IPv4 you plan to set</param>
        /// <param name="netmask">Netmask of the address. By default "255.255.255.0"</param>
        /// <param name="adapterNumber">Interface number (eth#). By default is 0</param>
        /// <param name="gateway">Default gateway packets will use</param>
        /// <returns>Received message as an array of strings</returns>
        public override string[] SetIP(
            string IP, string netmask = "255.255.255.0", ushort adapterNumber = 0, string gateway = null
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

        /// <summary>
        /// Send Ping to a certain IP
        /// </summary>
        /// <param name="IP">IP where ICMP packets will be sent</param>
        /// <param name="count">Number of retries. By default 5</param>
        /// <param name="msBetweenPackets">Miliseconds between each package</param>
        /// <param name="protocol">Sending protocol. Valid values: "ICMP", "TCP", "UDP". By default "ICMP"</param>
        /// <returns>Received message as an array of strings</returns>
        public string[] Ping(string IP, ushort count=5, uint msBetweenPackets = 500, string protocol="ICMP"){
            ushort protocolNum = 1;
            if (protocol.ToUpper().Equals("TCP"))
                protocolNum = 6;
            else if(protocol.ToUpper().Equals("UDP"))
                protocolNum = 17;
                
            return Ping(IP, $"-c {count.ToString()} -P {protocolNum.ToString()} -i {msBetweenPackets}");
        }

        /// <summary>
        /// Check whether a ping went right or wrong
        /// </summary>
        /// <param name="pingMessage">Result message of a ping</param>
        /// <returns>True if the ping went right, False otherwise</returns>
        /// <example>
        /// <code>
        /// if (PC.PingResult(PC.Ping("192.168.30.5")))
        ///     Console.WriteLine("The ping went ok");
        /// </code>
        /// </example>
        public override bool PingResult(string[] pingMessage){
            // We assume the result will be negative
            bool result = false;
            string[] lineSplit;
            foreach(string line in pingMessage){
                lineSplit = line.Split(new char[] {' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
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

        /// <summary>
        /// Show the route to a certain IP
        /// </summary>
        /// <param name="IP">IP</param>
        /// <returns>Received message as an array of strings</returns>
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