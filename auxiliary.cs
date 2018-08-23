using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GNS3sharp {

    /// <summary>
    /// Class that defines some methods and propertiesthat are helpful for
    /// the other classes of the namespace
    /// </summary>
    internal static class Aux{

        private static Dictionary<string,object>[] nodesAvailables = CreateNodesAvailable();
        /// <summary>
        /// Array of dictionaries. Every dictionary contains two keys: "class" and "label". If you create
        /// a new appliance class, you must add its label and type here
        /// </summary>
        /// <value>Values of the dictionaries are the 'type' related to the chosen label</value>
        internal static Dictionary<string,object>[] NodesAvailables { get => nodesAvailables; }

        /// <summary>
        /// Generate the dictionary for NodesAvailable property
        /// </summary>
        /// <returns>A map between the type of a node and its label</returns>
        private static Dictionary<string,object>[] CreateNodesAvailable(){
            
            /// <summary>
            /// List of all nodes defined in the API
            /// </summary>
            /// <returns>IEnumerable with the <c>Type</c> of the nodes</returns>
            IEnumerable<Type> GetNodesTypes(){
                
                /// <summary>
                /// Find the children classes of a class
                /// </summary>
                /// <typeparam name="TBaseType">Type whose children types must be found</typeparam>
                /// <returns>IEnumerable with the <c>Type</c> of the nodes</returns>
                IEnumerable<Type> FindSubClassesOf<TBaseType>() {   

                    var baseType = typeof(TBaseType);
                    var assembly = baseType.Assembly;

                    return assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
                }

                var routers = FindSubClassesOf<Router>();
                var guests = FindSubClassesOf<Switch>();
                var switches = FindSubClassesOf<Guest>();

                return routers.Concat(guests).Concat(switches);
            }

            List<Dictionary<string,object>> typesOfNodes = new List<Dictionary<string,object>>();

            foreach (var nodeType in GetNodesTypes()){
                
                typesOfNodes.Add(
                    new Dictionary<string,object>(){
                        {"class", nodeType},
                        {"label", nodeType.GetProperty("Label", BindingFlags.Static | BindingFlags.Public).GetValue(null).ToString()}
                    }
                );

            }

            return typesOfNodes.ToArray();
        }

        /// <summary>
        /// Return the right class type for a certain node. Try to match the label of the node
        /// with once of those defined in <c>nodesAvailable</c>
        /// </summary>
        /// <param name="nodeName">Name set to a node in GNS3</param>
        /// <returns>The type of the node. If it can not find the certain type
        /// of node, returns <c>typeof(Node)</c></returns>
        internal static Type NodeType(string nodeName){

            // If something goes wrong and the label is not properly set on the
            // name, it returns the generic Node class
            Type newNode = typeof(Node);
            Match match = Regex.Match(nodeName, @"(?<=\[).+?(?=\])");

            if (match.Success) {
                string label = match.Groups[0].Value.ToUpperInvariant();
                foreach(Dictionary<string,object> typeOfNode in nodesAvailables){
                    if (label.Equals(typeOfNode["label"].ToString())){
                        newNode = (Type)typeOfNode["class"];
                        break;
                    }
                }
            }
            return newNode;
        }

        /// <summary>
        /// Guess whether a string is an IP or not
        /// </summary>
        /// <param name="IP">String to check</param>
        /// <returns>True if the string is an IP, False otherwise</returns>
        internal static bool IsIP(string IP) => 
            Regex.IsMatch(IP, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");

        /// <summary>
        /// Guess whether a string is a netmask or not
        /// </summary>
        /// <param name="netmask">String to check</param>
        /// <returns>True if the string is a netmask, False otherwise</returns>
        internal static bool IsNetmask(string netmask) =>
            Regex.IsMatch(netmask, @"^(((255\.){3}(255|254|252|248|240|224|192|128|0+))|((255\.){2}(255|254|252|248|240|224|192|128|0+)\.0)|((255\.)(255|254|252|248|240|224|192|128|0+)(\.0+){2})|((255|254|252|248|240|224|192|128|0+)(\.0+){3}))$");
        
        /// <summary>
        /// Convert a mask written in numbers and dots into its CIDR notation
        /// </summary>
        /// <param name="netmaskDecimals">Mask written in numbers and dots</param>
        /// <returns>Mask in CIDR notation</returns>
        internal static short NetmaskCIDR(string netmaskDecimals){
            short result = 0;

            if (Aux.IsNetmask(netmaskDecimals)){
                // Split the mask by its dots
                string[] netmaskSplit = netmaskDecimals.Split('.');
                BitArray bits;
                foreach (string numberStr in netmaskSplit){
                    // Turn every number into a bit array
                    bits = new BitArray(BitConverter.GetBytes(ushort.Parse(numberStr)));
                    foreach (bool bit in bits){
                        // Run over the array and add 1 to the value for every true found
                        if (bit == true) result++;
                    }
                }
            } else{
                result = -1;
            }

            return result;
        }
    }

    /// <summary>
    /// Structure for gathering routing tables
    /// <remarks>
    /// It is just a structure for better handling routing tables
    /// </remarks>
    /// </summary>
    public class RoutingTable{

        /// <summary>
        /// Class that represents a row (a route) of a routing table
        /// </summary>
        public struct RoutingTableRow{
            private string destination;
            /// <summary>
            /// Destination of the route
            /// </summary>
            /// <value>Address as a string</value>
            public string Destination{ get => destination;}

            private string gateway;
            /// <summary>
            /// Gateway of the route
            /// </summary>
            /// <value>Address as a string</value>
            public string Gateway{ get => gateway;}
            
            private string netmask;
            /// <summary>
            /// Netmask of the route
            /// </summary>
            /// <value>Netmask as a string</value>
            public string Netmask{ get => netmask;}

            private string iface;
            /// <summary>
            /// Interface related to the route
            /// </summary>
            /// <value>Interface as a string</value>
            public string Iface{ get => iface;}

            private int metric;
            /// <summary>
            /// Metric of the route
            /// </summary>
            /// <value>Metric as an integer</value>
            public int Metric{ get => metric;}

            /// <summary>
            /// Constructor that initializes every parameter
            /// </summary>
            /// <param name="_destination">Destination of the route</param>
            /// <param name="_gateway">Gateway of the route</param>
            /// <param name="_netmask">Netmask of the route</param>
            /// <param name="_iface">Interface related to the route</param>
            /// <param name="_metric">Metric of the route</param>
            public RoutingTableRow(
                string _destination, string _gateway, string _netmask, 
                string _iface, int _metric
                ){
                if (_gateway.Equals('*'))
                    _gateway = "0.0.0.0";
                this.destination = _destination;
                this.gateway = _gateway;
                this.netmask = _netmask;
                this.iface = _iface;
                this.metric = _metric;
            }
        }

        private List<RoutingTableRow> routes;
        /// <summary>
        /// List of the routes the table contains
        /// </summary>
        /// <returns>List of <c>RoutingTableRow</c></returns>
        public RoutingTableRow[] Routes{ get => routes.ToArray();}

        /// <summary>
        /// Initialize the object
        /// </summary>
        public RoutingTable(){
            this.routes = new List<RoutingTableRow>();
        }

        /// <summary>
        /// Initialize the object with a fixed size
        /// </summary>
        /// <param name="numberOfRoutes">Number of routes the table contains</param>
        public RoutingTable(ushort numberOfRoutes){
            this.routes = new List<RoutingTableRow>(numberOfRoutes);
        }

        /// <summary>
        /// Add a new route to the table
        /// </summary>
        /// <param name="_destination">Destination of the route</param>
        /// <param name="_gateway">Gateway of the route</param>
        /// <param name="_netmask">Netmask of the route</param>
        /// <param name="_iface">Interface related to the route</param>
        /// <param name="_metric">Metric of the route</param>
        public void AddRoute(
            string destination, string gateway, string netmask, 
            string iface, int metric
            ){
            if (Aux.IsIP(destination) && Aux.IsIP(gateway) && Aux.IsNetmask(netmask)){
                routes.Add(new RoutingTableRow(
                    destination, gateway, netmask, iface, metric
                ));
            } else{
                Console.Error.WriteLine("Impossible to add the new row: some of the parameters were not valid");
            }
        }
    }
}