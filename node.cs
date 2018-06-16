using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using GNS3_UNITY_API;

public class Node{
    // Node attributes
    protected string consoleHost; public string ConsoleHost { get => consoleHost; }
    protected ushort port; public ushort Port { get => port; }
    protected string name; public string Name { get => name; }
    protected string id; public string ID { get => id; }
    protected TcpClient tcpConnection; public TcpClient TCPConnection { get => tcpConnection; }
    protected NetworkStream netStream; public NetworkStream NetStream { get => netStream; }

    // Constructor by default. It's not intended to be used
    public Node(){
        this.consoleHost = null; this.port = 0; this.name = null; this.id = null;
        this.tcpConnection = null; this.netStream = null;
    }

    // Constructor that sets all the parameters for the node
    public Node(string _consoleHost, ushort _port, string _name, string _id){
        this.consoleHost = _consoleHost; this.port = _port; this.name = _name; this.id = _id;
        (this.tcpConnection, this.netStream) = this.Connect();
    }

    // In case we want to clone the instance
    protected Node(Node clone){
        this.consoleHost = clone.ConsoleHost; this.port = clone.Port;
        this.name = clone.Name; this.id = clone.ID;
        this.tcpConnection = clone.TCPConnection; this.netStream = clone.NetStream;
    }

    // Close the connection with the server before leaving
    ~Node(){
        try{
            // Close the nextwork stream
            if(this.netStream != null)
                this.netStream.Close();
            // Close the TCP connection
            if(this.tcpConnection != null)
                this.tcpConnection.Close();
        } catch{}
    }

    // Stablish a TCP connection with the node
    protected (TcpClient Connection, NetworkStream Stream) Connect(){
        // Network endpoint as an IP address and a port number
        IPEndPoint address = new IPEndPoint(IPAddress.Parse(this.consoleHost),this.port);
        // Set the socket for the connection
        TcpClient newConnection = new TcpClient();
        // Stream used to send and receive data
        NetworkStream newStream = null;
        try{
            newConnection.Connect(address);
            newStream = newConnection.GetStream();
        } catch(Exception err){
            Console.Error.WriteLine("Impossible to connect to the node: {0}", err.Message);
            newConnection = null;
        }
        return (newConnection, newStream);
    }

    // Send a message we choose as a parameter to the node
    public void Send(string message){

        if (this.netStream.CanWrite){
            try{
                // We need to convert the string into a bytes array first
                byte[] out_txt = Encoding.Default.GetBytes($"{message}\n");
                this.netStream.Write(buffer: out_txt, offset: 0, size: out_txt.Length);
                this.netStream.Flush();
            } catch(ObjectDisposedException err1){
                Console.Error.WriteLine("Impossible to send anything, connection closed: {0}", err1.Message);
            } catch(NullReferenceException){
                Console.Error.WriteLine("Connection value is null. Probably it was not possible to initialize it");
            }  catch(Exception err3){
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
        if (this.netStream.CanRead){
            // Reception variable as a bytes array
            byte[] in_bytes = new byte[tcpConnection.ReceiveBufferSize];
            // Reception variable as a string
            string in_txt = "";
            do{
                // We repeat this until there's no more to read
                try{
                    this.netStream.Read(buffer: in_bytes, offset: 0, size: in_bytes.Length);
                    in_txt = $"{in_txt}{Encoding.Default.GetString(in_bytes)}";
                } catch(NullReferenceException){
                    Console.Error.WriteLine("Connection is null. Probably it was not possible to initialize it");
                } catch(Exception err2){
                    Console.Error.WriteLine("Some error occured while receiving text: {0}", err2.Message);
                }
                // We need to wait for the server to process our messages
                Thread.Sleep(2000);
            } while (this.netStream.DataAvailable);
            // Remove all the unnecesary characters contained in the buffer and split the text we have received in \n
            in_txt_split = Regex.Replace(in_txt, @"(\0){2,}", "").Split("\n");
        }
        return in_txt_split;
    }
    
}