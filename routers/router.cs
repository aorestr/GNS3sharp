using GNS3_UNITY_API;

public abstract class Router : Node{

    public Router() : base(){}
    public Router(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
    public Router(Node clone) : base(clone){}

}