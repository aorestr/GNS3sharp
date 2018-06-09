public class Node{
    // Node attributes
    protected string consoleHost;
    public string ConsoleHost { get{return name;} }
    protected ushort port;
    public ushort Port { get{return port;} }
    protected string name;
    public string Name { get{return name;} }

    // Constructor by default. It's not intended to be used
    public Node(){
        this.consoleHost = null;
        this.port = 0;
        this.name = null;
    }

    // Constructor that sets all the parameters for the node
    public Node(string _consoleHost, ushort _port, string _name){
        this.consoleHost = _consoleHost;
        this.port = _port;
        this.name = _name;
    }
    
}