public class Node{
    // Node attributes
    private string consoleHost;
    private int port;
    private string name;
    private int type;

    // Constructor that sets all the parameters for the node
    public Node(string _consoleHost, int _port, string _name, int _type){
        consoleHost = _consoleHost;
        port = _port;
        name = _name;
        type = _type;
    }

    //////////////////////////// GETs \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    public string getConsoleHost(){return consoleHost;}
    public int getPort(){return port;}
    public string getName(){return name;}
    public int getType(){return type;}
    
}