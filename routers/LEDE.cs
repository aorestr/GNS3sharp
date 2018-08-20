using System;
using System.Collections.Generic;

namespace GNS3sharp {
    public class LEDE : OpenWRT{

        private const string label = "LEDE";
        /// <summary>
        /// Label you must set in the name of the node at the GNS3 project
        /// <para>Name of the node must look like "[LEDE]Name"</para>
        /// </summary>
        /// <value>Label as a string</value>
        new public static string Label { get => label; }

        // Constructors
        public LEDE() : base() {}
        public LEDE(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}
        public LEDE(Node father) : base(father){}

    }
}