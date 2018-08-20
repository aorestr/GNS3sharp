using System.Collections.Generic;

namespace GNS3sharp {
    
    /// <summary>
    /// Abstract class that serves as a skeleton for the routers that are defined
    /// </summary>
    public abstract class Router : Node{

        /// <summary>
        /// Routing table of the node as an object
        /// </summary>
        /// <value>Object of type <c>RoutingTable</c></value>
        public abstract RoutingTable RoutingTable { get; }

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        public Router() : base(){}

        /// <summary>
        /// Constructor for any kind of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        public Router(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}

        /// <summary>
        /// Constructor that replicates a router from another node
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public Router(Node clone) : base(clone){}

        /// <summary>
        /// Send Ping to a certain IP
        /// </summary>
        /// <param name="IP">IP where ICMP packets will be sent</param>
        /// <param name="count">Number of retries. By default 5</param>
        /// <param name="timeout">Seconds until it stops retrying</param>
        /// <returns>The result of the ping as an array of strings</returns>
        public virtual string[] Ping(string IP, ushort count=5, ushort timeout=10){
            return Ping(IP,$"-c {count.ToString()} -W {timeout.ToString()}");
        }

        /// <summary>
        /// Get the IPv4 related to a certain interface. Needs overwriting.
        /// </summary>
        /// <param name="iface">Interface whose IPv4 will be searched</param>
        /// <returns>Array of strings: first corresponds to the IP nad second to the netmask</returns>
        public abstract string[] GetIPByInterface(string iface);

        /// <summary>
        /// Activate an interface of the appliance. Needs overwriting
        /// </summary>
        /// <param name="IP">IPv4 that will be set for the interface</param>
        /// <param name="netmask">Netmask that will be set for the interface</param>
        /// <param name="interfaceNumber">Number of the interface (0 for eth0...)</param>
        /// <returns>The result of the operation as an array of strings</returns>
        public abstract string[] ActivateInterface(string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0);

        /// <summary>
        /// Dectivate an interface of the appliance. Needs overwriting
        /// </summary>
        /// <param name="IP">IPv4 of the interface</param>
        /// <param name="netmask">Netmask of the interface</param>
        /// <param name="interfaceNumber">Number of the interface (0 for eth0...)</param>
        /// <returns>The result of the operation as an array of strings</returns>
        public abstract string[] DeactivateInterface(string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0);

        /// <summary>
        /// Set a route for a certain network
        /// </summary>
        /// <param name="destination">Destination of the route</param>
        /// <param name="gateway">Address where the packets must go in order to reach the destination</param>
        /// <param name="netmask">Netmask of the destination</param>
        /// <returns>The result of the operation as an array of strings</returns>
        public abstract string[] SetRoute(string destination, string gateway, string netmask = "255.255.255.0");

        /// <summary>
        /// Gets the routing table of the router
        /// </summary>
        /// <returns>The routing table as an array of strings</returns>
        protected abstract string[] GetRoutingTable();
    }
}