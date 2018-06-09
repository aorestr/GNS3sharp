using GNS3_UNITY_API;

public class VPC : Node{
    public VPC(){}

    public VPC(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
}