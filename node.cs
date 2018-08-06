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
    public class Node{
        // Node attributes
        protected string consoleHost; public string ConsoleHost { get => consoleHost; }
        protected ushort port; public ushort Port { get => port; }
        protected string name; public string Name { get => name; }
        protected string id; public string ID { get => id; }

        // Ports of the node. It contains information about every network interface
        // (adapterNumber, portNumber, link->(null if free))
        protected Dictionary<string,dynamic>[] ports; public Dictionary<string,dynamic>[] Ports{ get => ports; }

        // List of links connected to the node
        protected List<Link> linksAttached = new List<Link>(); public List<Link> LinksAttached { get => linksAttached; }

        // Connection properties
        protected TcpClient tcpConnection; public TcpClient TCPConnection { get => tcpConnection; }
        protected NetworkStream netStream; public NetworkStream NetStream { get => netStream; }

        ///////////////////////////// Constructors ////////////////////////////////////////////

        // Constructor by default. It's not intended to be used
        public Node(){
            this.consoleHost = null; this.port = 0; this.name = null; this.id = null;
            this.tcpConnection = null; this.netStream = null;
        }

        // Constructor that sets all the parameters for the node
        internal Node(string _consoleHost, ushort _port, string _name, string _id,
            Dictionary<string,dynamic>[] _ports){

            this.consoleHost = _consoleHost; this.port = _port; this.name = _name; this.id = _id;
            this.ports = _ports;
            (this.tcpConnection, this.netStream) = this.Connect();
        }

        // In case we want to clone the instance
        public Node(Node clone){
            this.consoleHost = clone.ConsoleHost; this.port = clone.Port;
            this.name = clone.Name; this.id = clone.ID; this.ports = clone.Ports;
            this.tcpConnection = clone.TCPConnection; this.netStream = clone.NetStream;
        }

        // Close the connection with the server before leaving
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

        // Stablish a TCP connection with the node
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

        // Send a message we choose as a parameter to the node
        public void Send(string message){

            if (this.tcpConnection == null)
                (this.tcpConnection, this.netStream) = this.Connect();
            if (this.tcpConnection != null && this.netStream.CanWrite){
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
            } else{
                Console.Error.WriteLine("Impossible to send any messages right now");
            }

        }

        // Get the info from the buffer terminal of a node
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
        
        // Send ping to a certain IP
        public string[] Ping(string IP, ushort count=5){
            return Ping(IP, $"-c {count.ToString()}");
        }

        // Send ping to a certain IP
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

        // Check whether a ping went right or wrong
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