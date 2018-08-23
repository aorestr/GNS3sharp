using System;
using System.Collections.Generic;

namespace GNS3sharp {

    /// <summary>
    /// Abstract class that serves as a skeleton for the guest devices that are defined
    /// </summary>
    public abstract class Guest : Node{

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        internal Guest() : base() {}

        /// <summary>
        /// Constructor for any kind of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        internal Guest(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}

        /// <summary>
        /// Constructor that replicates a guest from another node
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public Guest(Node clone) : base(clone){}

        /// <summary>
        /// Set an IP for an interface of the device
        /// </summary>
        /// <param name="IP">IPv4 you plan to set</param>
        /// <param name="netmask">Netmask of the address. By default "255.255.255.0"</param>
        /// <param name="adapterNumber">Interface number (eth#). By default is 0</param>
        /// <param name="gateway">Default gateway packets will use</param>
        /// <returns>Received message as an array of strings</returns>
        public abstract string[] SetIP(string IP, string netmask, ushort adapterNumber, string gateway);
    }
}