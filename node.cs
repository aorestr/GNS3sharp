using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace GNS3sharp {

    /// <summary>
    /// This class represents a node from a GNS3 project. Its main methods are:
    /// <list type="bullet">
    /// <item>
    /// <term>Send</term>
    /// <description>Allows you to send some message to a node</description>
    /// </item>
    /// <item>
    /// <term>Receive</term>
    /// <description>Allows you to receive messages from a node</description>
    /// </item>
    /// </list>
    /// <remarks>
    /// This class can interact directly by telnet to a node
    /// </remarks>
    /// </summary>
    public class Node{

        protected string consoleHost;
        /// <summary>
        /// IP of the machine where the node is hosted
        /// </summary>
        /// <value>String IP</value>
        public string ConsoleHost { get => consoleHost; }

        protected ushort port;
        /// <summary>
        /// Port of the machine where the node is hosted
        /// </summary>
        /// <value>Port number</value>
        public ushort Port { get => port; }

        protected string name;
        /// <summary>
        /// Name of the node stablished in the project
        /// </summary>
        /// <value>String name</value>
        public string Name { get => name; }

        protected string id;
        /// <summary>
        /// ID the node has implicitly
        /// </summary>
        /// <value>String ID</value>
        public string ID { get => id; }

        protected Dictionary<string,dynamic>[] ports;
        /// <summary>
        /// Array of dictionaries that contains information about every network interface
        /// </summary>
        /// <value>
        /// Keys: adapterNumber, portNumber and link. If the value found in link
        /// is null means that interface is not attached to anything yet
        /// </value>
        public Dictionary<string,dynamic>[] Ports{ get => ports; }

        protected List<Link> linksAttached = new List<Link>();
        /// <summary>
        /// List of the links that the node is attach to
        /// </summary>
        /// <value>List of <c>Link</c></value>
        public List<Link> LinksAttached { get => linksAttached; }

        protected TcpClient tcpConnection;
        /// <summary>
        /// TCP client to stablish connections
        /// </summary>
        internal TcpClient TCPConnection { get => tcpConnection; }
        
        protected NetworkStream netStream;
        /// <summary>
        /// Network stream through which messages are sent
        /// </summary>
        internal NetworkStream NetStream { get => netStream; }

        ///////////////////////////// Constructors ////////////////////////////////////////

        /// <summary>
        /// Constructor by default. Every property is empty
        /// </summary>
        internal Node(){
            this.consoleHost = null; this.port = 0; this.name = null; this.id = null;
            this.tcpConnection = null; this.netStream = null;
        }

        /// <summary>
        /// Constructor of <c>Node</c>. It must be called from a <c>GNS3sharp</c> object
        /// </summary>
        /// <param name="_consoleHost">IP of the machine where the node is hosted</param>
        /// <param name="_port">Port of the machine where the node is hosted</param>
        /// <param name="_name">Name of the node stablished in the project</param>
        /// <param name="_id">ID the node has implicitly</param>
        /// <param name="_ports">Array of dictionaries that contains information about every network interface</param>
        internal Node(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports){

            this.consoleHost = _consoleHost; this.port = _port; this.name = _name; this.id = _id;
            this.ports = _ports;
            (this.tcpConnection, this.netStream) = this.Connect();
        }

        /// <summary>
        /// Constructor that replicates a node from another one
        /// </summary>
        /// <param name="clone">Node you want to make the copy from</param>
        public Node(Node clone){
            this.consoleHost = clone.ConsoleHost; this.port = clone.Port;
            this.name = clone.Name; this.id = clone.ID; this.ports = clone.Ports;
            this.tcpConnection = clone.TCPConnection; this.netStream = clone.NetStream;
        }

        /// <summary>
        /// Close the connection with the node before leaving
        /// </summary>
        ~Node(){
            try{
                // Close the network stream
                if(this.netStream != null)
                    this.netStream.Close();
                // Close the TCP connection
                if(this.tcpConnection != null)
                    this.tcpConnection.Close();
            } catch{}
        }

        ///////////////////////////////// Methods ////////////////////////////////////////////

        /// <summary>
        /// Stablish a TCP connection with the node and makes a network stream out of it
        /// </summary>
        /// <param name="timeout">Timeout (in seconds) before quitting the connection</param>
        /// <returns></returns>
        protected (TcpClient Connection, NetworkStream Stream) Connect(int timeout = 10000){
            // Network endpoint as an IP address and a port number
            IPEndPoint address = new IPEndPoint(IPAddress.Parse(this.consoleHost),this.port);
            // Set the socket for the connection
            TcpClient newConnection = new TcpClient();
            // Stream used to send and receive data
            NetworkStream newStream = null;
            try{
                newConnection.Connect(address);
                newStream = newConnection.GetStream();
                newStream.ReadTimeout = timeout; newStream.WriteTimeout = timeout;
            } catch(Exception err){
                Console.Error.WriteLine("Impossible to connect to the node {0}: {1}", this.name, err.Message);
                newConnection = null;
            }
            return (newConnection, newStream);
        }

        /// <summary>
        /// Send a message to the node through a network stream
        /// </summary>
        /// <param name="message">String with the message that is intended to be sent</param>
        /// <example>
        /// <code>
        /// PC.Send("ifconfig");
        /// </code>
        /// </example>
        public void Send(string message){

            if (this.tcpConnection == null)
                (this.tcpConnection, this.netStream) = this.Connect();
            if (this.tcpConnection == null)
                Console.Error.WriteLine("The connection couldn't be stablished and so the message can not be sent");
            else if (!this.netStream.CanWrite)
                Console.Error.WriteLine("Impossible to send any messages right now");
            else{
                try{
                    // We need to convert the string into a bytes array first
                    byte[] out_txt = Encoding.Default.GetBytes($"{message}\n");
                    this.netStream.Write(buffer: out_txt, offset: 0, size: out_txt.Length);
                    this.netStream.Flush();
                } catch(ObjectDisposedException err1){
                    Console.Error.WriteLine("Impossible to send anything, connection closed: {0}", err1.Message);
                } catch(NullReferenceException){
                    Console.Error.WriteLine("Connection value is null. Probably it was not possible to initialize it");
                } catch(IOException err2){
                    Console.Error.WriteLine("Time to write expired: {0}", err2.Message);
                } catch(Exception err3){
                    Console.Error.WriteLine(
                        "Some error occured while sending '{0}': {1}",
                        message, err3.Message
                    );
                }
            }

        }
        
        /// <summary>
        /// Receive messages from the buffer of the node network stream
        /// </summary>
        /// <returns>Messages as an array of strings</returns>
        /// <example>
        /// <code>
        /// foreach(string line in PC.Receive())
        ///     Console.WriteLine("${line}");
        /// </code>
        /// </example>
        public string[] Receive(){

            // Reception variable as a string split by \n
            string[] in_txt_split = null;

            if (this.tcpConnection != null)
                (this.tcpConnection, this.netStream) = this.Connect();
            if (this.netStream.CanRead){
                // Reception variable as a bytes array
                byte[] in_bytes = new byte[tcpConnection.ReceiveBufferSize];
                // Reception variable as a string
                string in_txt = "";
                // Number of bytes read for every iteration
                int numberOfBytesRead;
                do{
                    do{
                        // We repeat this until there's no more to read
                        try{
                            numberOfBytesRead = this.netStream.Read(buffer: in_bytes, offset: 0, size: in_bytes.Length);
                            in_txt = $"{in_txt}{Encoding.Default.GetString(in_bytes, 0, numberOfBytesRead)}";
                        } catch(NullReferenceException){
                            Console.Error.WriteLine("Connection is null. Probably it was not possible to initialize it");
                        } catch(IOException err1){
                            Console.Error.WriteLine("Time to write expired: {0}", err1.Message);
                        } catch(Exception err2){
                            Console.Error.WriteLine("Some error occured while receiving text: {0}", err2.Message);
                        }
                    } while (this.netStream.DataAvailable);
                    // We need to wait for the server to process our messages
                    Thread.Sleep(2000);
                // We double check the availability of data 
                } while (this.netStream.DataAvailable);
                // Remove all the unnecesary characters contained in the buffer and split the text we have received in \n
                in_txt_split = Regex.Replace(in_txt, @"(\0){2,}", "").Split('\n');
            }
            return in_txt_split;
        }
        
        /// <summary>
        /// Send Ping to a certain IP
        /// </summary>
        /// <param name="IP">IP where ICMP packets will be sent</param>
        /// <param name="count">Number of retries. By default 5</param>
        /// <returns>The result messages of the ping</returns>
        /// <example>
        /// <code>
        /// foreach(string line in PC.Ping("192.168.30.5"))
        ///     Console.WriteLine($"{line}");
        /// </code>
        /// </example>
        public virtual string[] Ping(string IP, ushort count=5){
            return Ping(IP, $"-c {count.ToString()}");
        }

        /// <summary>
        /// Template child classes can used a template for creating 'Ping''s with different parameters
        /// </summary>
        /// <param name="IP">IP where to send the ICMP packets to</param>
        /// <param name="additionalParameters">Additional parameters for the ping</param>
        /// <returns>The result messages of the ping</returns>
        /// <example>
        /// <code>
        /// foreach(string line in PC.Ping("192.168.30.5"))
        ///     Console.WriteLine($"{line}");
        /// </code>
        /// </example>
        protected string[] Ping(
            string IP, string additionalParameters){
            // Reception variable as a string
            string[] in_txt = null;

            if(Aux.IsIP(IP)) {
                Send($"ping {IP} {additionalParameters}");
                in_txt = Receive();
            } else{
                Console.Error.WriteLine($"{IP} is not a valid IP");
            }

            // Return the response
            return in_txt;
        }

        /// <summary>
        /// Check whether a ping went right or wrong
        /// </summary>
        /// <param name="pingMessage">Result of a ping</param>
        /// <returns>True if the ping went right, False otherwise</returns>
        /// <example>
        /// <code>
        /// if (PC.PingResult(PC.Ping("192.168.30.5")))
        ///     Console.WriteLine("The ping went ok");
        /// </code>
        /// </example>
        public virtual bool PingResult(string[] pingMessage){
            // We assume the result will be negative
            bool result = false;
            foreach(string line in pingMessage.Reverse<string>()){
                // Search for the line with the results
                if (Regex.IsMatch(line, @"\d+\spackets\stransmitted,\s\d+\spackets\sreceived,\s(\d+|\d+[.]\d+)%?%\spacket\sloss\s")){
                    string[] resultStr = line.Split(',');
                    // If "%d packets received" is different to zero means the ping went right
                    if (Int32.Parse(resultStr[1].TrimStart().Split(' ')[0]) != 0)
                        result = true;
                    break;
                }
            }
            return result;
        }

    }
}