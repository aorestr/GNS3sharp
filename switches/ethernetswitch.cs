using System.Collections.Generic;

namespace GNS3sharp {

    /// <summary>
    /// Representation of a Ethernet Switch type of node
    /// <remarks>
    /// Define methods that are only available for this appliance
    /// </remarks>
    /// </summary>
    public class EthernetSwitch : Switch{

        private const string label = "ETHSW";
        /// <summary>
        /// Label you must set in the name of the node at the GNS3 project
        /// <para>Name of the node must look like "[ETHSW]Name"</para>
        /// </summary>
        /// <value>Label as a string</value>
        public static string Label { get => label; }

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        internal EthernetSwitch() : base() {}

        /// <summary>
        /// Constructor for any kind of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        internal EthernetSwitch(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports) : 
            base(_consoleHost, _port, _name, _id, _ports){}

        /// <summary>
        /// Constructor that replicates a switch from another node
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public EthernetSwitch(Node father) : base(father){}

    }
}