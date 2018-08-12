using System.Collections.Generic;

namespace GNS3sharp {
    public abstract class Router : Node{

        // Routing table of the node as an object
        public abstract RoutingTable RoutingTable { get; }

        // Constructors
        public Router() : base(){}
        public Router(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public Router(Node clone) : base(clone){}

        public virtual string[] Ping(string IP, ushort count=5, ushort timeout=10){
            return Ping(IP,$"-c {count.ToString()} -W {timeout.ToString()}");
        }

        // Get the IP related to a certain interface. The first parameter
        // of the list is the IP and the second one is the netmask
        public virtual string[] GetIPByInterface(string iface){

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

        public abstract string[] ActivateInterface(string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0);
        public abstract string[] DeactivateInterface(string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0);
        public abstract string[] SetRoute(string destination, string gateway, string netmask = "255.255.255.0");
        public abstract string[] GetRoutingTable();
    }
}