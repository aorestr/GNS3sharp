using System;
using System.Collections.Generic;

namespace GNS3sharp {
    public class LEDE : OpenWRT{

        // Label to determine what device is
        new public const string label = "LEDE";

        // Constructors
        public LEDE() : base() {}
        public LEDE(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public LEDE(Node father) : base(father){}

    }
}