using System;
using System.Collections.Generic;
using System.Linq;
using GNS3_UNITY_API;

namespace GNS3_UNITY_API
{
    class Program {
        static void Main(string[] args) {
            GNS3sharp handler = new GNS3sharp("61261064-a2a4-4666-8f26-d2dbfbbe26a4");
            Example1(handler);
            //Example1_5(handler);
            //Example2(handler);
            //Example3(handler);
            //Example4();
            Example5(handler);
            Example1(handler);
        }
       
        // Show every node information
        public static void Example1(GNS3sharp handler){
            foreach(Node n in handler.Nodes){
                Console.Write("host: {0}, port: {1}, name: {2}, component: {3}",
                    n.ConsoleHost, n.Port, n.Name, n.GetType().ToString());
                foreach(Link link in n.LinksAttached){
                    Console.Write($", link: {link.ID}");
                }
                foreach(Dictionary<string,dynamic> port in n.Ports){
                    Console.Write($",\n\tport, adapter number: {port["adapterNumber"]}");
                    Console.Write($",\n\tport, port number: {port["portNumber"]}");
                    Console.Write($",\n\tport, link: {port["link"]}");
                }
                Console.WriteLine();
            }
        }

        // Show every node information
        public static void Example1_5(GNS3sharp handler){
            foreach(Link l in handler.Links){
                Console.WriteLine("id: {0}, nodes: {1}, packet_loss: {2}, frequency_drop: {3},",
                    l.ID, l.Nodes, l.PacketLoss, l.FrequencyDrop);
                Console.WriteLine("latency: {0}, jitter: {1}, corrupt: {2}", l.Latency, l.Jitter, l.Corrupt);
            }
        }

        // Send some basic commands to a specific VPC I already created
        public static void Example2(GNS3sharp handler){
            string[] in_txt = null;
            try{
                VPC PC = (VPC)handler.GetNodeByName("[VPC]PC_Lleida");
                in_txt = PC.ShowConf();
                foreach(string lin in in_txt){
                    Console.WriteLine($"{lin}");
                }
                in_txt = PC.SetIP("192.168.10.11");
                foreach(string lin in in_txt){
                    Console.WriteLine($"{lin}");
                }
                bool reached;
                (in_txt, reached) = PC.Ping("192.168.20.11");
                foreach(string lin in in_txt){
                    Console.WriteLine($"{lin}");
                }
                Console.WriteLine($"The ping went {(reached ? "right" : "wrong")}");
            } catch(Exception err){
                Console.Error.WriteLine("Some error occured: {0}", err.Message);
            } 
        }

        // Set an IP to a Microcore
        public static void Example3(GNS3sharp handler){
            string[] in_txt = null;
            try{
                MicroCore PC = (MicroCore)handler.Nodes.Where(node => node.Name == "[MICROCORE]PC_Balaguer").ToList()[0];
                in_txt = PC.SetIP("192.168.30.11","255.255.255.0",0);
                foreach(string lin in in_txt){
                    Console.WriteLine($"{lin}");
                }
            } catch(Exception err){
                Console.Error.WriteLine("Some error occured: {0}", err.Message);
            } 
        }

        // Example https://www.youtube.com/watch?v=rMrPJlKXsJ8
        public static void Example4(){
            GNS3sharp handler = new GNS3sharp("61261064-a2a4-4666-8f26-d2dbfbbe26a4");
            MicroCore PC1 = (MicroCore)handler.GetNodeByName("[MICROCORE]PC_Lleida");
            MicroCore PC2 = (MicroCore)handler.GetNodeByName("[MICROCORE]PC_Mollerusa");
            MicroCore PC3 = (MicroCore)handler.GetNodeByName("[MICROCORE]PC_Balaguer");
            OpenWRT Router1 = (OpenWRT)handler.GetNodeByName("[OPENWRT]Lleida");
            OpenWRT Router2 = (OpenWRT)handler.GetNodeByName("[OPENWRT]Mollerusa");
            OpenWRT Router3 = (OpenWRT)handler.GetNodeByName("[OPENWRT]Balaguer");
            PC1.SetIP(IP:"192.168.10.11",gateway:"192.168.10.1");
            PC2.SetIP(IP:"192.168.20.11",gateway:"192.168.20.1");
            PC3.SetIP(IP:"192.168.30.11",gateway:"192.168.30.1");
            Router1.ActivateInterface(IP:"192.168.10.1",interfaceNumber:0);
            Router1.ActivateInterface(IP:"200.10.10.1",interfaceNumber:1);
            Router2.ActivateInterface(IP:"192.168.20.1",interfaceNumber:0);
            Router2.ActivateInterface(IP:"200.10.10.2",interfaceNumber:2);
            Router2.ActivateInterface(IP:"200.20.20.1",interfaceNumber:1);
            Router3.ActivateInterface(IP:"192.168.30.1",interfaceNumber:0);
            Router3.ActivateInterface(IP:"200.20.20.2",interfaceNumber:2);
            Router1.SetRoute(destination:"192.168.20.0",gateway:"200.10.10.2");
            Router1.SetRoute(destination:"192.168.30.0",gateway:"200.10.10.2");
            Router2.SetRoute(destination:"192.168.10.0",gateway:"200.10.10.1");
            Router2.SetRoute(destination:"192.168.30.0",gateway:"200.20.20.2");
            Router3.SetRoute(destination:"192.168.10.0",gateway:"200.20.20.1");
            Router3.SetRoute(destination:"192.168.20.0",gateway:"200.20.20.1");
            string[] msgs; (msgs, _) = PC1.Ping("192.168.20.11");
            foreach(string msg in msgs)
                Console.WriteLine(msg);
        }

        public static void Example5(GNS3sharp handler){
            Console.WriteLine(handler.SetLink(handler.GetNodeByName("[OPENWRT]Balaguer"), handler.GetNodeByName("[OPENWRT]Mollerusa"), latency:45));
            Console.WriteLine(
                handler.EditLink(
                    handler.GetNodeByName("[ETHSW]SW2"), handler.GetNodeByName("[OPENWRT]Mollerusa"),
                    latency:9, packetLoss:3, jitter:0, frequencyDrop:44
                )
            );
            Console.WriteLine(handler.RemoveLink(handler.GetNodeByName("[OPENWRT]Mollerusa"), handler.GetNodeByName("[ETHSW]SW2")));
            Console.WriteLine(handler.RemoveLink(handler.GetNodeByName("[OPENWRT]Mollerusa"), handler.GetNodeByName("[OPENWRT]Balaguer")));
        }

    }
}
