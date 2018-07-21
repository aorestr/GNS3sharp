using System;
using System.Collections.Generic;

namespace GNS3sharp {
    public abstract class Guest : Node{

        // Constructors
        public Guest() : base() {}
        public Guest(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public Guest(Node clone) : base(clone){}

        // Methods every guest subclass must have
<<<<<<< HEAD
        abstract public string[] SetIP(string IP, string netmask, ushort adapter_number, string gateway);

        // Send ping to a certain IP
        virtual public string[] Ping(string IP){

            // Reception variable as a string
            string[] in_txt = null;

            if(Aux.IsIP(IP)) {
                Send($"ping {IP}");
                in_txt = Receive();
            } else{
                Console.Error.WriteLine($"{IP} is not a valid IP");
            }

            // Return the response
            return in_txt;

        }
=======
        public abstract string[] SetIP(string IP, string netmask, ushort adapter_number, string gateway);
>>>>>>> master
    }
}