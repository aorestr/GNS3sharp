using System;
using System.Net;
using System.Net.Sockets;

public class Node{
    // Node attributes
    protected string consoleHost; public string ConsoleHost { get{return name;} }
    protected ushort port; public ushort Port { get{return port;} }
    protected string name; public string Name { get{return name;} }
    protected Socket socket; public Socket Socket { get{return socket;} }

    // Constructor by default. It's not intended to be used
    public Node(){
        this.consoleHost = null;
        this.port = 0;
        this.name = null;
        this.socket = null;
    }

    // Constructor that sets all the parameters for the node
    public Node(string _consoleHost, ushort _port, string _name){
        this.consoleHost = _consoleHost;
        this.port = _port;
        this.name = _name;
        this.socket = this.Connect();
    }

    // Stablish a TCP connection with the node
    protected Socket Connect(){
        // Set the socket for the connection
        //                            IPv4                        Bytes sequences    TCP
        Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // Network endpoint as an IP address and a port number
        IPEndPoint address = new IPEndPoint(IPAddress.Parse(this.consoleHost),this.port);
        try{
            newSocket.Connect(address);
        } catch(Exception err){
            Console.Error.WriteLine("Impossible to connect to the node: {0}", err.Message);
            newSocket = null;
        }
        return newSocket;
    }
    
}