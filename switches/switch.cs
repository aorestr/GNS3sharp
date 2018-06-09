using GNS3_UNITY_API;

public class Switch : Node{
    public Switch(){}

    public Switch(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
}