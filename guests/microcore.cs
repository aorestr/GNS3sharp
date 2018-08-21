using System;
using System.Linq;
using System.Collections.Generic;

namespace GNS3sharp {
    public class MicroCore : Guest{

        private const string label = "MICROCORE";
        /// <summary>
        /// Label you must set in the name of the node at the GNS3 project
        /// <para>Name of the node must look like "[MICROCORE]Name"</para>
        /// </summary>
        /// <value>Label as a string</value>
        public static string Label { get => label; }

        // Constructors
        internal MicroCore() : base() {}
        internal MicroCore(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public MicroCore(Node father) : base(father){}

        // Set an IP for the MicroCore
        public override string[] SetIP(
            string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0, string gateway = null
            ){

            // Reception variable as a string
            string[] in_txt = null;

            if(!Aux.IsIP(IP)) {
                Console.Error.WriteLine($"{IP} is not a valid IP");
            } else if(!Aux.IsNetmask(netmask)){ 
                Console.Error.WriteLine($"{netmask} is not a valid netmask");
            } else{
                Send($"sudo ifconfig eth{interfaceNumber.ToString()} {IP} netmask {netmask}");
                in_txt = Receive();
                if (gateway != null) {
                    // If we choose to set a gateway
                    in_txt = in_txt.Concat(SetGateway(gateway)).ToArray();
                }
            }
            // Return the response
            return in_txt;

        }

        // Set a gateway
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

        // Send a ping to a certain IP
        public virtual string[] Ping(string IP, ushort count=5, ushort timeout=10){
            return Ping(IP,$"-c {count.ToString()} -W {timeout.ToString()}");
        }

    }
}