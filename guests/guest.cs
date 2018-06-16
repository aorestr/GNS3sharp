using GNS3_UNITY_API;

public abstract class Guest : Node{

    public Guest() : base() {}
    public Guest(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
    public Guest(Node clone) : base(clone){}

}