using System;

namespace GNS3_UNITY_API {
    class Program {
        static void Main(string[] args)
        {
            GNS3sharp handler = new GNS3sharp("b4a4f44d-0f62-4435-89e0-84c8c7a2b35f");
            // Show every node information
            foreach(Node n in handler.Nodes){
                Console.WriteLine("host: {0}, port: {1}, name: {2}, component: {3}",
                    n.ConsoleHost, n.Port, n.Name, n.GetType().ToString());
                Console.WriteLine("Bytes sent: {0}", n.Send("show"));
                Console.WriteLine("String received: {0}", n.Receive().in_txt);
            }
        }
    }
}
