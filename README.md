# GNS3sharp #
This work was born as a need for my end-of-studies project. Since its purpose was basically integrate a network simulator ([**GNS3**](https://www.gns3.com/)) in a game engine ([**Unity**](https://unity3d.com)), I needed an API between the two parts of the project in order to make the interaction possible. Of course, the API offers much more possibilities and it's up to you to even find new and more imaginative uses!

**Unity** works mostly with **C#** so the API has been made entirely in this language. The way it is made, how to make it work and what can do will be explained later on.

## Installation ##
You can either place the source code on your project or import one of the .dll's you may find at the '[Release](https://github.com/aorestr/GNS3sharp/releases)' space in this repository as a reference. Just keep in mind that the library depends on [Json.NET](https://www.newtonsoft.com/json) in both cases, so you will need to install it in order to make it work.

In case you want to compile the code as a **.NET Standard** library by yourself, besides Json.NET you will need to include [Microsoft.CSharp](https://www.nuget.org/packages/Microsoft.CSharp/) as a dependency for your project.

## First steps ##
Once the library is imported into your project and you have **GNS3** running, you can create a new instance of the handler class. The constructor needs at least the ID of the project you want to handle. You can also add the server and port your **GNS3 server** uses. By default *``host`` = "localhost"* and *``port`` = 3080*:

```csharp
GNS3sharp handler = new GNS3sharp("b4a4f44d-0f62-4435-89e0-84c8c7a2b35f", "localhost", 3080);
```

If you do not know the ID of the project you plan to game with but you know its name, you can use ``ServerProjects.GetProjectIDByName()``.

Ok, so now you have plenty of information and tools within this variable. It contains a list with all the nodes inside your project. You can access this list with the property ``Nodes``:

```csharp
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
```

You can also run/stop your project by using the handler:

```csharp
handler.StartProject();
handler.StopProject();
```

Or even just one of the nodes:
```csharp
handler.StartNode(handler.GetNodeByName(nodeName));
```

## Nodes ##
Every node is related to a class which handles it with the functions you expect this device to have. In order to make this work, the name of your node at your **GNS3** project must start with a label like *[NODETYPE]*. For example, if you plan to have a *VPC* called "My_PC", you must make sure the component is called "[VPC]My_PC" in the project. Every type of node has its own name you can find in the static property called ``label``. For instance, **OpenVSwitch** switches has "OVS" as label and **OpenWRT** routers "OPENWRT".

Every type of node has methods that let you interact with it in a very easy way. For example, once again, if we are dealing with a *VPC* node we can set its IP just like:

```csharp
VPC OurVPC = (VPC)handler.GetNodeByName("[VPC]PC_Lleida");
// dynamic OurVPC = handler.GetNodeByName("[VPC]PC_Lleida");
OurVPC.SetIP("192.168.10.11");
```

Sadly you will need to cast to the type of node you will deal with if you want to use its especific methods. It's also possible to use the ``dynamic`` type.

We also can select the netmask ("255.255.255.0" by default) and the gateway.

If any of the already created methods can't meet your needs you can always use the generic ``Send`` and ``Receive`` methods:

```csharp
OurVPC.Send("show");
string[] messages = OurVPC.Receive();
```

So far, only five type of nodes have been included: *VPC*, *EthernetSwitch*, *MicroCore*, *OpenWRT*, *LEDE* and *OpenvSwitch*. You can create your own classes for the devices you plan to use. Otherwise, you can use the generic *Node* class instead. If you are thinking of creating a new class, keep in mind you will need to add a ``Label`` property in it.

## Links ##
Nodes are connected to each other by links. These links have some parameters that you can interact with such as the latency or the frequency drop. You have plenty control of these links through the API as well.

For example, you can set a new link between two nodes like the following:
```csharp
handler.SetLink(
	handler.GetNodeByName("[OPENWRT]Balaguer"),
	handler.GetNodeByName("[OPENWRT]Mollerusa"),
	latency:45
);
```
This adds also 45ms of latency to the link.

You can modify the properties the link too or just remove it:
```csharp
handler.EditLink(
	handler.GetNodeByName("[OPENWRT]Balaguer"),
	handler.GetNodeByName("[OPENWRT]Mollerusa"),
	latency:9, packetLoss:3, jitter:0, frequencyDrop:44
);
handler.RemoveLink(
	handler.GetNodeByName("[OPENWRT]Mollerusa"),
	handler.GetNodeByName("[OPENWRT]Balaguer")
);
```

## Limitations ##
* As you might already noticed, this tool can not create a GNS3 project from scratch. It's more like a tool for handling already created projects.
* Unlike the links, nodes can not be added or removed from the project. You can configure them as much as you want but always through its terminal.
* This API relies totally on GNS3 server. It can not work without it running, which means you need to open it always first.
