using System;
using System.Linq;
using System.Collections.Generic;

namespace GNS3sharp {

    /// <summary>
    /// Representation of a MicroCore type of node
    /// <remarks>
    /// Define methods that are only available for this appliance
    /// </remarks>
    /// </summary>
    public class MicroCore : Guest{

        private const string label = "MICROCORE";
        /// <summary>
        /// Label you must set in the name of the node at the GNS3 project
        /// <para>Name of the node must look like "[MICROCORE]Name"</para>
        /// </summary>
        /// <value>Label as a string</value>
        public static string Label { get => label; }

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        internal MicroCore() : base() {}

        /// <summary>
        /// Constructor for any kind of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        internal MicroCore(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}

        /// <summary>
        /// Constructor that replicates a guest from another node
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public MicroCore(Node clone) : base(clone){}

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

            if(!Aux.IsIP(IP)) {
                Console.Error.WriteLine($"{IP} is not a valid IP");
            } else if(!Aux.IsNetmask(netmask)){ 
                Console.Error.WriteLine($"{netmask} is not a valid netmask");
            } else{
                Send($"sudo ifconfig eth{adapterNumber.ToString()} {IP} netmask {netmask}");
                in_txt = Receive();
                if (gateway != null) {
                    // If we choose to set a gateway
                    in_txt = in_txt.Concat(SetGateway(gateway)).ToArray();
                }
            }
            // Return the response
            return in_txt;

        }

        /// <summary>
        /// Set a default gateway for sending messages
        /// </summary>
        /// <param name="gateway">Gateway address</param>
        /// <returns>Received message as an array of strings</returns>
        public string[] SetGateway(string gateway){
            // Reception variable as a string
            string[] in_txt = null;

            if (Aux.IsIP(gateway)) {
                Send($"sudo route add default gw {gateway}");
                in_txt = Receive();
            } else{
                Console.Error.WriteLine($"{gateway} is not a valid gateway");
            }
            // Return the response
            return in_txt;

        }

        /// <summary>
        /// Send Ping to a certain IP
        /// </summary>
        /// <param name="IP">IP where ICMP packets will be sent</param>
        /// <param name="count">Number of retries. By default 5</param>
        /// <param name="timeout">Timeout for retrying</param>
        /// <returns>The result messages of the ping as an array of strings</returns>
        public virtual string[] Ping(string IP, ushort count=5, ushort timeout=10){
            return Ping(IP,$"-c {count.ToString()} -W {timeout.ToString()}");
        }

    }
}