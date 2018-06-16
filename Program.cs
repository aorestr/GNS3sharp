using System;
using System.Linq;

namespace GNS3_UNITY_API {
    class Program {
        static void Main(string[] args) {
            GNS3sharp handler = new GNS3sharp("b4a4f44d-0f62-4435-89e0-84c8c7a2b35f");
            Example1(handler);
            Example2(handler);
        }
       
        // Show every node information
        public static void Example1(GNS3sharp handler){
            foreach(Node n in handler.Nodes){
                Console.WriteLine("host: {0}, port: {1}, name: {2}, component: {3}",
                    n.ConsoleHost, n.Port, n.Name, n.GetType().ToString());
            }
        }

        public static void Example2(GNS3sharp handler){
            string[] in_txt = null;
            try{
                VPC PC = (VPC)handler.Nodes.Where(node => node.Name == "[VPC]PC_Lleida").ToList()[0];
                in_txt = PC.ShowConf();
                foreach(string lin in in_txt){
                    Console.WriteLine($"{lin}");
                }
                in_txt = PC.SetIP("192.168.10.11");
                foreach(string lin in in_txt){
                    Console.WriteLine($"{lin}");
                }
                in_txt = PC.Ping("192.168.20.11");
                foreach(string lin in in_txt){
                    Console.WriteLine($"{lin}");
                }
            } catch(Exception err){
                Console.Error.WriteLine("Some error occured: {0}", err.Message);
            } 
        }
    }
}
