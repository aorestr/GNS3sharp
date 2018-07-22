using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GNS3sharp {
    /*
    Defines some methods that are helpful for other classes
    */
    public static class Aux{

        // This array of dictionaries contains the different classes of nodes and
        // their 'label's. The 'label's are the strings you must place between brackets
        // in the node name in the GNS3 project
        public static Dictionary<string,object>[] nodesAvailables = {
            new Dictionary<string,object>(){
                {"class", typeof(VPC)}, {"label", VPC.label}
            },  new Dictionary<string,object>(){
                {"class", typeof(MicroCore)}, {"label", MicroCore.label}
            }, new Dictionary<string,object>(){
                {"class", typeof(OpenvSwitch)}, {"label", OpenvSwitch.label}
            }, new Dictionary<string,object>(){
                {"class", typeof(EthernetSwitch)}, {"label", EthernetSwitch.label}
            }, new Dictionary<string,object>(){
                {"class", typeof(OpenWRT)}, {"label", OpenWRT.label}
            }, new Dictionary<string,object>(){
                {"class", typeof(LEDE)}, {"label", LEDE.label}
            }
        };

        // It returns the right class type for every node
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

        // Guess whether a string is an IP or not
        internal static bool IsIP(string IP) => 
            Regex.IsMatch(IP, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");

        // Guess whether a string is a netmask or not
        internal static bool IsNetmask(string netmask) =>
            Regex.IsMatch(netmask, @"^(((255\.){3}(255|254|252|248|240|224|192|128|0+))|((255\.){2}(255|254|252|248|240|224|192|128|0+)\.0)|((255\.)(255|254|252|248|240|224|192|128|0+)(\.0+){2})|((255|254|252|248|240|224|192|128|0+)(\.0+){3}))$");
        
        // Convert a mask written by numbers and dots into its CIDR notation
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

    // Structure for gathering routing tables
    public class RoutingTable{

        // Every route within the table
        public struct RoutingTableRow{
            // Properties are the main fields of a routing table
            private string destination; public string Destination{ get => destination;}
            private string gateway; public string Gateway{ get => gateway;}
            private string netmask; public string Netmask{ get => netmask;}
            private string iface; public string Iface{ get => iface;}
            private int metric; public int Metric{ get => metric;}

            // Constructor that initialize every parameter
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

        private List<RoutingTableRow> routes; public RoutingTableRow[] Routes{ get => routes.ToArray();}

        // Initialize the object 
        public RoutingTable(){
            this.routes = new List<RoutingTableRow>();
        }

        // Initialize the object with fixed size
        public RoutingTable(ushort numberOfRoutes){
            this.routes = new List<RoutingTableRow>(numberOfRoutes);
        }

        // Add a new route to the table
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