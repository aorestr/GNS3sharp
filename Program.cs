using System;
using System.Linq;

namespace GNS3_UNITY_API {
    class Program {
        static void Main(string[] args) {
            GNS3sharp handler = new GNS3sharp("b4a4f44d-0f62-4435-89e0-84c8c7a2b35f");
            Example2(handler);
        }

        
        // Show every node information
        public static void Example1(GNS3sharp handler){
            foreach(Node n in handler.Nodes){
                Console.WriteLine("host: {0}, port: {1}, name: {2}, component: {3}",
                    n.ConsoleHost, n.Port, n.Name, n.GetType().ToString());
                Console.WriteLine("Bytes sent: {0}", n.Send("show"));
                Console.WriteLine("String received: ");
                try{
                    foreach(string s in n.Receive().in_txt)
                        Console.WriteLine($"\t{s}");
                } catch(NullReferenceException){}
            }
        }

        public static void Example2(GNS3sharp handler){
            string[] in_txt = null;
            
        }
    }
}
