using System.Collections.Generic;

namespace GNS3sharp {
    /// <summary>
    /// Abstract class that serves as a skeleton for the switches that are defined
    /// </summary>
    public abstract class Switch : Node{

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        internal Switch() : base() {}

        /// <summary>
        /// Constructor for any kind of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        internal Switch(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}

        /// <summary>
        /// Constructor that replicates a switch from another node
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public Switch(Node clone) : base(clone){}
    }
}