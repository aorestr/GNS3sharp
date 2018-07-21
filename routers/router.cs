using System.Collections.Generic;

namespace GNS3sharp {
    public abstract class Router : Node{

        // Constructors
        public Router() : base(){}
        public Router(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public Router(Node clone) : base(clone){}

        public virtual string[] Ping(string IP, ushort count=5, ushort timeout=10){
            return Ping(IP,$"-c {count.ToString()} -W {timeout.ToString()}");
        }

        public abstract string[] ActivateInterface(string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0);
        public abstract string[] DeactivateInterface(string IP, string netmask = "255.255.255.0", ushort interfaceNumber = 0);
        public abstract string[] SetRoute(string destination, string gateway, string netmask = "255.255.255.0");
    }
}