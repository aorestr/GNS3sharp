using System;
using GNS3_UNITY_API;

public abstract class Guest : Node{

    // Constructors
    public Guest() : base() {}
    public Guest(string _consoleHost, ushort _port, string _name, string _id) : 
        base(_consoleHost, _port, _name, _id){}
    public Guest(Node clone) : base(clone){}

    // Methods every guest subclass must have
    abstract public string[] SetIP(string IP, string netmask, ushort adapter_number, string gateway);

    // Send ping to a certain IP
    virtual public (string[], bool reached) Ping(string IP){

        // Reception variable as a string
        string[] in_txt = null;

        if(Aux.IsIP(IP)) {
            Send($"ping {IP}");
            in_txt = Receive();
        } else{
            Console.Error.WriteLine($"{IP} is not a valid IP");
        }

        /*
        ///////////////
        TO DO
         */
        // Check if the ping went right
        bool reached = false;
        //////////////

        // Return the response
        return (in_txt, reached);

    }
}