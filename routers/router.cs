using GNS3_UNITY_API;

public class Router : Node{
    public Router() : base() {}

    public Router(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}

    protected Router(Router clone){
        this.consoleHost = clone.ConsoleHost; this.port = clone.Port;
        this.name = clone.Name; this.id = clone.ID;
        this.tcpConnection = clone.TCPConnection; this.netStream = clone.NetStream;
    }
}