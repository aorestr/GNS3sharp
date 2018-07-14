using System.Collections.Generic;

namespace GNS3sharp {
    public class OpenvSwitch : Switch{

        // In the name of the node: [label]Name
        public const string label = "OVS";

        // Constructors
        public OpenvSwitch() : base() {}
        public OpenvSwitch(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public OpenvSwitch(Node father) : base(father){}

    }
}