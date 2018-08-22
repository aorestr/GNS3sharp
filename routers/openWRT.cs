using System;
using System.Collections.Generic;

namespace GNS3sharp {

    /// <summary>
    /// Representation of a OpenWRT type of node
    /// <remarks>
    /// Define methods that are only available for this appliance
    /// </remarks>
    /// </summary>
    public class OpenWRT : Router{

        private const string label = "OPENWRT";
        /// <summary>
        /// Label you must set in the name of the node at the GNS3 project
        /// <para>Name of the node must look like "[OPENWRT]Name"</para>
        /// </summary>
        /// <value>Label as a string</value>
        public static string Label { get => label; }

        /// <summary>
        /// Routing table of the node as an object
        /// </summary>
        /// <value>Object of type <c>RoutingTable</c></value>
        public override RoutingTable RoutingTable { 
            get => this.GetRoutingTable(this.GetRoutingTable());
        }

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        internal OpenWRT() : base() {}

        /// <summary>
        /// Constructor for any kind of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        internal OpenWRT(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}

        /// <summary>
        /// Constructor that replicates a router from another node
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public OpenWRT(Node clone) : base(clone){}

        /// <summary>
        /// Force the terminal to allow messages by sending a first \n. You should wait for the router to finish of configuring though
        /// </summary>
        private void ActivateTerminal(){
            Send("");
        }

        /// <summary>
        /// Activate an interface of the router
        /// </summary>
        /// <param name="IP">IPv4 you plan to set</param>
        /// <param name="netmask">Netmask of the address. By default "255.255.255.0"</param>
        /// <param name="interfaceNumber">Interface number (eth#). By default is 0</param>
        /// <returns>Received message as an array of strings</returns>
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
        /// <summary>
        /// Change the status of an interface to 'up' or 'down'
        /// </summary>
        /// <param name="status">"up" or "down</param>
        /// <param name="IP">IPv4 you plan to set</param>
        /// <param name="netmask">Netmask of the address. By default "255.255.255.0"</param>
        /// <param name="interfaceNumber">Interface number (eth#). By default is 0</param>
        /// <returns>Received message as an array of strings</returns>
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

        /// <summary>
        /// Set a route to a certain destination through a gateway
        /// </summary>
        /// <param name="destination">Address where the route is planned to get</param>
        /// <param name="gateway">Gateway where the packets must initially go through to reach the destination</param>
        /// <param name="netmask">Netmask of the IP. By default "255.255.255.0"</param>
        /// <returns>Received message as an array of strings</returns>
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

        /// <summary>
        /// Get the routing table of the router
        /// </summary>
        /// <returns>The routing table as an array of strings</returns>
        protected override string[] GetRoutingTable(){
            // Reception variable as a string
            string[] in_txt = null;

            ActivateTerminal();
            Send($"route -n");
            in_txt = Receive();

            // Return the response
            return in_txt;
        }

        /// <summary>
        /// Get the routing table of the router as a RoutingTable object. It's used to get the RoutingTable property of the class
        /// </summary>
        /// <param name="routingTable">Routing table as a string</param>
        /// <returns>The routing table</returns>
        private RoutingTable GetRoutingTable(string[] routingTable){
            RoutingTable table = new RoutingTable();

            string[][] lines = new string[routingTable.Length][];

            // Position of the "Destination-Gateway-Genmask-Flags-Metric-Ref-Use-Iface" line

            short informationPosition = -1;
            for (short i = 0; i < routingTable.Length; i++){
                lines[i] = routingTable[i].Trim().Split(new char[] {' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (
                    lines[i][0].ToUpper().Equals("DESTINATION") &&
                    lines[i][1].ToUpper().Equals("GATEWAY") &&
                    lines[i][2].ToUpper().Equals("GENMASK")
                    )
                    informationPosition = i;
            }

            if (informationPosition >= 0){
                short i = informationPosition;
                // While the first element of every line keep being an IP means we are
                // scanning the actual routing table
                while (Aux.IsIP(lines[++i][0])){
                    table.AddRoute(
                        lines[i][0], lines[i][1], lines[i][2],
                        lines[i][7], UInt16.Parse(lines[i][5])
                    );
                }
            } else{
                table = null;
                Console.Error.WriteLine("Impossible to analyze the routing table");
            }

            return table;
        }

        /// <summary>
        /// (Re)start the router firewall
        /// </summary>
        /// <returns>Received message as an array of strings</returns>
        public virtual string[] EnableFirewall(){
            return ChangeFirewallStatus("start");
        }

        /// <summary>
        /// Stop the router firewall
        /// </summary>
        /// <returns>Received message as an array of strings</returns>
        public virtual string[] DisableFirewall(){
            return ChangeFirewallStatus("stop");
        }

        /// <summary>
        /// Change router status
        /// </summary>
        /// <param name="newStatus">"start" or "stop"</param>
        /// <returns></returns>
        private string[] ChangeFirewallStatus(string newStatus){
            // Reception variable as a string
            string[] in_txt = null;

            ActivateTerminal();
            Send($"/etc/init.d/firewall {newStatus}");
            in_txt = Receive();

            // Return the response
            return in_txt;
        }

        /// <summary>
        /// Get the IPv4 related to a certain interface. Needs overwriting.
        /// </summary>
        /// <param name="iface">Interface whose IPv4 will be searched</param>
        /// <returns>Array of strings: first corresponds to the IP nad second to the netmask</returns>
        public override string[] GetIPByInterface(string iface){

            string GetParameterIfconfig(string _iface, string type){

                string result = null; string command = null;
                if (type.Equals("IP"))
                    command = $"ifconfig {_iface} | grep 'inet addr' | cut -d: -f2 | awk '{{print $1}}'";
                else if (type.Equals("NETMASK"))
                    command = $"ifconfig {_iface} | grep 'inet addr' | cut -d: -f4 | awk '{{print $1}}'";
                if (command != null){
                    string lineTemp;
                    Send(command);
                    foreach (string line in Receive()) {
                        lineTemp = line.Trim();
                        if (Aux.IsIP(lineTemp)){
                            result = lineTemp;
                            break;
                        }
                    }
                }
                return result;

            }

            return new string[]{ 
                GetParameterIfconfig(iface, "IP"), GetParameterIfconfig(iface, "NETMASK") 
            };
            
        }

    }
}