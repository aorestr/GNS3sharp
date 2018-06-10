using GNS3_UNITY_API;

public class Guest : Node{
    public Guest(){}

    public Guest(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}

    // In case we want to clone the instance
    protected Guest(Guest clone){
        this.consoleHost = clone.ConsoleHost; this.port = clone.Port;
        this.name = clone.Name; this.id = clone.ID;
        this.tcpConnection = clone.TCPConnection; this.netStream = clone.NetStream;
    }

}