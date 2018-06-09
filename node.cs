using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GNS3_UNITY_API;

public class Node{
    // Node attributes
    protected string consoleHost; public string ConsoleHost { get => consoleHost; }
    protected ushort port; public ushort Port { get => port; }
    protected string name; public string Name { get => name; }
    protected string id; public string ID { get => id; }
    protected Socket socket; public Socket Socket { get => socket; }

    // Constructor by default. It's not intended to be used
    public Node(){
        this.consoleHost = null;
        this.port = 0;
        this.name = null;
        this.socket = null;
    }

    // Constructor that sets all the parameters for the node
    public Node(string _consoleHost, ushort _port, string _name, string _id){
        this.consoleHost = _consoleHost;
        this.port = _port;
        this.name = _name;
        this.id = _id;
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

    // Send a message we choose as a parameter to the node
    public int Send(string message){
        // Return from the method
        int numBytesSent = 0;
        // We need to convert the string into a bytes array first
        byte[] out_txt = Encoding.Default.GetBytes($"{message}\n");
        try{
            numBytesSent = this.socket.Send(buffer: out_txt);
        } catch(ObjectDisposedException err1){
            Console.Error.WriteLine("Impossible to send anything, socket closed: {0}", err1.Message);
        } catch(Exception err2){
            Console.Error.WriteLine(
                "Some error occured while sending '{0}': {1}",
                message, err2.Message
            );
        }
        return numBytesSent;
    }

    // Get the info from the buffer terminal of a node
    public (string in_txt, int numBytes) Receive(){
        // Reception varible as a string
        string in_txt = null;
        // Reception varible as a bytes array
        byte[] in_bytes = new byte[255];
        // Number of bytes received
        int numBytes = 0;

        try{
            numBytes = this.socket.Receive(in_bytes, in_bytes.Length, 0);
            in_txt = Encoding.UTF8.GetString(in_bytes);
        } catch(NullReferenceException){
            Console.Error.WriteLine("Socket value is null. Probably it was not possible to initiaze it");
        } catch(Exception err2){
            Console.Error.WriteLine("Some error occured while receiving text: {0}", err2.Message);
        } 
        return (in_txt, numBytes);
    }
    
}