using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GNS3_UNITY_API;

/*
It handles the connections with the different components of
a GNS3 project
 */
public class GNS3sharp {

    // Project ID
    private string projectID; public string ProjectID{ get => projectID; }

    // GNS3 server host
    private string host; public string Host{ get => host; }

    // GNS3 server port
    private ushort port; public ushort Port{ get => port; }

    // List of nodes inside the project with all the info about them.
    // The info is not filtered
    private List<Dictionary<string,object>> nodesJSON;
    public List<Dictionary<string,object>> NodesJSON{
        get {
            ExtractNodesDictionary(
                $"http://{host}:{port.ToString()}/v2/projects/{projectID}/nodes"
            );
            return nodesJSON;
        }
    }

    // Same for the links
    private List<Dictionary<string,object>> linksJSON;
    public List<Dictionary<string,object>> LinksJSON{
        get {
            ExtractLinksDictionary(
                $"http://{host}:{port.ToString()}/v2/projects/{projectID}/links"
            );
            return linksJSON;
        }
    }

    // List of nodes the project has
    private Node[] nodes; public Node[] Nodes{ get => nodes; }

    // List of links the project has. We will use a struct
    private List<Link> links; public List<Link> Links{ get => links; }

    // These dictionaries will help getting the nodes by name and ID
    private Dictionary<string, Node> nodesByName = new Dictionary<string, Node>();
    private Dictionary<string, Node> nodesByID = new Dictionary<string, Node>();

    // HTTP client used to interact with the REST API
    private static readonly HttpClient HTTPclient = new HttpClient();

    ///////////////////////////// Constructors ////////////////////////////////////////////

    // Wrong constructor. It needs an ID for the project
    public GNS3sharp() => Console.Error.WriteLine("You need the project ID");

    // Right constructor. Needs the project ID. Just get the nodes
    // the project has
    public GNS3sharp(string _projectID, string _host = "localhost", ushort _port = 3080) {
        this.projectID = _projectID; this.host = _host; this.port = _port;
        // Defines the URL where the info is
        string baseURL = $"http://{_host}:{_port.ToString()}/v2/projects/{_projectID}";
        // Extract that info
        Console.Write($"Extracting nodes information from URL: {baseURL}/nodes... ");
        ExtractNodesDictionary($"{baseURL}/nodes");
        Console.WriteLine(" ok");
        Console.Write($"Extracting links information from URL: {baseURL}/links... ");
        ExtractLinksDictionary($"{baseURL}/links");
        Console.WriteLine(" ok");
        if (nodesJSON != null){
            // Create the nodes related to that info
            nodes = GetNodes(nodesJSON);
            links = GetLinks(linksJSON);
        }
        try{
            SaveLinksInfoInNodes(links);
        } catch(Exception err){
            Console.Error.WriteLine(
                $"Ups, something went wrong. Unable to save the link info in the nodes: {err}"
            );
        }
    }

    ///////////////////////////////// Methods ////////////////////////////////////////////

    // It returns a dictionary with information about the nodes of the project
    private void ExtractNodesDictionary(string URL){
        nodesJSON = ExtractDictionary(URL, "nodes");    
    }

    // It returns a dictionary with information about the project
    private void ExtractLinksDictionary(string URL){
        linksJSON = ExtractDictionary(URL, "links");    
    }

    // It returns a dictionary with information about the nodes of the project
    private static List<Dictionary<string,object>> ExtractDictionary(string URL, string type){
        
        // Extract a JSON from a GET request
        string ExtractJSONString(string local_URL){
            // Variable with a string with all the JSON info
            string local_json;
            try{
                // Get the info from the JSON file you can access from the GNS3 Rest service
                using (System.Net.WebClient GNS3NodesProject = new System.Net.WebClient()){
                    local_json = GNS3NodesProject.DownloadString(local_URL);
                }
            } catch(Exception err){
                // Server not open
                Console.Error.WriteLine("Impossible to connect to URL {0}: {1}", local_URL, err.Message);
                local_json = null;
            }

            return local_json;
        }

        // Raw string
        string json = ExtractJSONString(URL);

        // Depending on the JSON from we have downloaded the data
        string lastElement = null;
        if (type.Equals("nodes"))
            lastElement = "z";
        else
            lastElement = "suspend";

        // Creates a list of dictionaries. It will be used to store the JSON info and
        // get the values from it
        List<Dictionary<string,object>> dictList = new List<Dictionary<string,object>>();
        if(json == "[]"){
            Console.Error.WriteLine("JSON is empty");
            dictList = null;
        } else if (string.IsNullOrEmpty(json) == false){
            dictList = DeserializeJSONList(json, lastElement);
            /*
            // Show content of the list
            foreach(Dictionary<string, object> dict in dictList){
                foreach (KeyValuePair<string, object> kvp in dict){
                    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                }
            }
            */
        } else{
            dictList = null;
        }
        return dictList;

    }

    // Desearialize a certain JSON and store it in a list of dictionaries.
    // lastKey indicates the last key of a dictionary
    private static List<Dictionary<string,object>> DeserializeJSONList(string json, string lastKey){
        
        // Return variable
        List<Dictionary<string,object>> dictList = new List<Dictionary<string,object>>();
        // JSON array object
        JArray jsonArray = JArray.Parse(json);
        Dictionary<string,object> tempDict = new Dictionary<string, object>();

        // Variables in which store the JSON info temporaly
        string name; object value;        
        if (jsonArray.HasValues){
            foreach (JObject jO in jsonArray.Children<JObject>()) {
                foreach (JProperty jP in jO.Properties()) {                
                    name = jP.Name;
                    value = (object)jP.Value;
                    tempDict.Add(name,value);
                    // The last key of every node
                    if (jP.Name.Equals(lastKey)) {
                        // If we do not copy the content of the dictionary into another
                        // we will be copying by reference and erase the content once
                        // we 'clear' the dict
                        Dictionary<string, object> copyDict = new Dictionary<string, object>(tempDict);
                        dictList.Add(copyDict);
                        tempDict.Clear();
                    }
                }
            }
        }
        return dictList;
    }
    
    // Save a list in every node with information about the links that are
    // atrached to them
    private void SaveLinksInfoInNodes(List<Link> listOfLinks){
        foreach (Link link in listOfLinks){
            foreach(Node node in link.Nodes){
                if(node != null)
                    node.LinksAttached.Add(link);
            }
        }
    }

    // Given a certain link and its nodes JSON string, add the link into the
    // correct port of the nodes
    private void MatchLinkWithNodePorts(Link link, string nodesJSON){
        
        List<Dictionary<string,object>> dictList = null;
        try{
            dictList = DeserializeJSONList(nodesJSON, "port_number");
        } catch (Exception err){
            Console.Error.WriteLine(
                "Some problem occured while trying to gather information about the nodes connect to the link: {0}",
                err.Message
            );
        }

        if (dictList.Count > 0){
            // Iterates through the JSON dictionary
            foreach (Dictionary<string, object> nodeTemp in dictList){
                // Iterates through the nodes the link connects
                foreach (Node node in link.Nodes){
                    if (node != null && node.ID.Equals(nodeTemp["node_id"].ToString())){
                        // Search for the port that matches the found one
                        var foundPort = node.Ports.Where(
                            x => (
                                x["adapterNumber"].ToString() == nodeTemp["adapter_number"].ToString() &&
                                x["portNumber"].ToString() == nodeTemp["port_number"].ToString()
                            )
                        );
                        // If exists, add the link into the key "link" of the ports list
                        // of dictionaries of the node
                        if (foundPort.Count() > 0) foundPort.First()["link"] = link;
                    }
                }
            }
        }

    }

    // Create an array with the nodes. Each element is a Node instance
    private Node[] GetNodes(List<Dictionary<string,object>> JSON){
        // Return variable
        Node[] listOfNodes = new Node[JSON.Count];

        // Extract the ports a node has
        Dictionary<string,dynamic>[] GetNodeListOfPorts(Dictionary<string, object> node){
            // Return variable
            List<Dictionary<string,dynamic>> ports = new List<Dictionary<string,dynamic>>();

            try{
                // Extract a dictionary with the ports defined in the project nodes JSON
                Dictionary<string,object>[] portsRaw = DeserializeJSONList(
                    node["ports"].ToString(), "short_name"
                ).ToArray();
                foreach(Dictionary<string,object> nodePort in portsRaw){
                    ports.Add(
                        new Dictionary<string,dynamic>(){
                            {"adapterNumber", UInt16.Parse(nodePort["adapter_number"].ToString())},
                            {"portNumber", UInt16.Parse(nodePort["port_number"].ToString())},
                            {"link", null}
                        } 
                    );
                }
            } catch (Exception err){
                Console.Error.WriteLine($"Something went wrong while extracting the ports info: {err}");
            }

            return ports.ToArray();
        }

        Type classType; int i = 0;
        try{
            foreach(Dictionary<string, object> node in JSON){
                try{
                    Console.Write($"Gathering information for node #{i}... ");
                    // Assign a class or another depending on the node
                    classType = Aux.NodeType(node["name"].ToString());
                    listOfNodes[i] = (Node)Activator.CreateInstance(
                        classType,
                        node["console_host"].ToString(), 
                        ushort.Parse(node["console"].ToString()), 
                        node["name"].ToString(),
                        node["node_id"].ToString(),
                        GetNodeListOfPorts(node)
                    );
                    nodesByName.Add(listOfNodes[i].Name, listOfNodes[i]);
                    nodesByID.Add(listOfNodes[i].ID, listOfNodes[i]);
                    i++;
                    Console.WriteLine(" ok");
                } catch(Exception err1){
                    Console.Error.WriteLine(
                        "Impossible to save the configuration for the node #{0}: {1}", 
                        i.ToString(), err1.Message
                    );
                }
            }
        } catch(Exception err2){
            Console.Error.WriteLine(
                "Some problem occured while saving the nodes information: {0}",
                err2.Message
            );
            listOfNodes = null;
        }

        return listOfNodes;
    }

    // Create an array with the links. It contains information about the
    private List<Link> GetLinks(List<Dictionary<string,object>> JSON){
        // Return variable
        List<Link> listOfLinks = new List<Link>();
        // Function that returns the nodes connected by the link
        Node[] GetNodesConnectedByLink(string nodesJSON){
            // Return variable
            Node[] nodesList = new Node[2];
            List<Dictionary<string,object>> dictList = null;
            try{
                dictList = DeserializeJSONList(nodesJSON, "port_number");
            } catch (Exception err){
                Console.Error.WriteLine(
                    "Some problem occured while trying to gather information about the nodes connect to the link: {0}",
                    err.Message
                );
            }
            if (dictList.Count > 0){
                ushort idx = 0;
                foreach (Dictionary<string, object> node in dictList){
                    try{
                        nodesList[idx++] = nodesByID[node["node_id"].ToString()];
                    } catch(Exception){
                        Console.Error.WriteLine(
                            $"Unknown node with ID: {node["node_id"].ToString()}"
                        );
                    }
                }
            }
            return nodesList;
        }
        // Extract a certain filter of the link
        int ExtractFilter(JObject filtJSON, string filter){
            
            int filterValue = 0;

            try{
                if (filter.Equals("latency"))
                    filterValue = filtJSON.Property("delay").First.ToObject<int[]>()[0];
                else if (filter.Equals("jitter"))
                    filterValue = filtJSON.Property("delay").First.ToObject<int[]>()[1];
                else
                    filterValue = filtJSON.Property(filter).First.ToObject<int[]>()[0];
            } catch (Exception){}
            
            return filterValue;
        }

        JObject filtersJSON; int i = 0;
        Dictionary<string, string> serverInfo = new Dictionary<string, string>(){
            {"host", this.host}, {"port", this.port.ToString()},
            {"projectID", this.projectID}
        };
        try{
            string nodesJSON;
            foreach(Dictionary<string, object> link in JSON){
                try{
                    Console.Write($"Gathering information for link #{i}... ");
                    filtersJSON = JObject.Parse(link["filters"].ToString());
                    nodesJSON = link["nodes"].ToString();
                    if (filtersJSON.HasValues){
                        // If the link has some filter activates
                        listOfLinks.Add(new Link(
                            link["link_id"].ToString(),
                            GetNodesConnectedByLink(nodesJSON),
                            serverInfo, HTTPclient,
                            ExtractFilter(filtersJSON, "frequency_drop"),
                            ExtractFilter(filtersJSON, "packet_loss"),
                            ExtractFilter(filtersJSON, "latency"),
                            ExtractFilter(filtersJSON, "jitter"),
                            ExtractFilter(filtersJSON, "corrupt")
                        ));
                    } else{
                        // If don't
                        listOfLinks.Add(new Link(
                            link["link_id"].ToString(),
                            GetNodesConnectedByLink(nodesJSON),
                            serverInfo, HTTPclient
                        ));
                    }
                    MatchLinkWithNodePorts(listOfLinks.Last(), nodesJSON);
                    i++;
                    Console.WriteLine(" ok");
                } catch(Exception err1){
                    Console.Error.WriteLine(
                        $"Impossible to save the configuration for the link #{0}: {1}", 
                        i.ToString(), err1.Message
                    );
                }
            }
        } catch(Exception err2){
            Console.Error.WriteLine(
                "Some problem occured while saving the links information: {0}",
                err2.Message
            );
            listOfLinks = null;
        }

        return listOfLinks;
    }

    // Initialize all the nodes in the project
    public string[] StartProject(){
        return ChangeProjectStatus("start");
    }

    // Stop all the nodes in the project
    public string[] StopProject(){
        return ChangeProjectStatus("stop");
    }

    // Change the status of the project (start or stop)
    private string[] ChangeProjectStatus(string status){
        
        // First part of the URL
        string URLHeader = $"http://{host}:{port}/v2/projects/{projectID}/nodes";

        // String with all the messages received
        int numNodes = nodes.Length;
        string[] totalMessages = new string[numNodes];

        // Pack the content we will send
        ByteArrayContent byteContent = null;
        try{
            string content = JsonConvert.SerializeObject(new Dictionary<string, string> { { "", "" } });
            byteContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content));
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        } catch(JsonSerializationException err){
            Console.Error.WriteLine("Impossible to serialize the JSON to send it to the API: {0}", err.Message);
        } 

        if (status.Equals("start"))
            Console.WriteLine("Activating all the nodes in the project...");
        else
            Console.WriteLine("Deactivating all the nodes in the project...");

        if (byteContent != null){
            for (ushort i = 0; i < numNodes; i++){
                try{
                    var res = HTTPclient.PostAsync(
                        $"{URLHeader}/{nodes[i].ID}/{status}", byteContent
                    ).Result.Content.ReadAsStringAsync();
                    totalMessages[i] = res.Result.ToString();
                } catch(HttpRequestException err){
                    Console.Error.WriteLine("Some problem occured with the HTTP connection: {0}", err.Message);
                } catch(Exception err){
                    Console.Error.WriteLine("Impossible to {2} node {0}: {1}", nodes[i].Name, err.Message, status);
                }
            }
            Console.WriteLine("...ok");
        }

        // If everything goes right, it returns info about every node
        return totalMessages;
    }

    // Create a new link
    public bool SetLink(Node node1, Node node2, 
        int frequencyDrop=0, int packetLoss=0,
        int latency=0, int jitter=0, int corrupt=0){
        
        bool linkCreated;
        if (node1 != null && node2 != null){
            // URL where send the data
            string URL = ($"http://{host}:{port}/v2/projects/{projectID}/links");

            // Get a certain key of the new link
            object ExtractKeyNewLink(string JSONLink, string key){
                // Return variable
                object newID = null;

                // Parse the JSON string into an object
                JObject jO = JObject.Parse(JSONLink);      
                if (jO.HasValues){
                    foreach (JProperty jP in jO.Properties()) {                
                        if (jP.Name.ToString().Equals(key)){
                            newID = (object)jP.Value;
                            break;
                        }
                    }
                }

                return newID;
            }

            Dictionary<string, dynamic>[] nodesInfo = new Dictionary<string, dynamic>[]{
                new Dictionary<string, dynamic>(){
                    {"adapter_number", 2}, {"node_id", $"{node1.ID}"}, {"port_number", 0}
                },
                new Dictionary<string, dynamic>(){
                    {"adapter_number", 1}, {"node_id", $"{node2.ID}"}, {"port_number", 0}
                }
            };
            Dictionary<string, int[]> filtersInfo = new Dictionary<string, int[]>{
                {"frequency_drop", new int[1]{frequencyDrop}}, {"packet_loss", new int[1]{packetLoss}},
                {"delay", new int[2]{latency,jitter}}, {"corrupt", new int[1]{corrupt}}
            };

            try{

                // Pack the content we will send
                string content = JsonConvert.SerializeObject(new Dictionary<string, dynamic> { 
                    { "nodes", nodesInfo }, { "filters" , filtersInfo }
                });
                ByteArrayContent byteContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content));
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                // Send it. It returns a string with the info about the link
                var res = HTTPclient.PostAsync(
                    $"{URL}", byteContent
                ).Result;
                if (!res.IsSuccessStatusCode){
                    Console.Error.WriteLine("Impossible to create the link: the status of the response is not success");
                    linkCreated = false;
                } else{
                    string responseString = res.Content.ReadAsStringAsync().Result.ToString();
                    // Extract the ID from the new link created
                    string newID = ExtractKeyNewLink(responseString, "link_id").ToString();

                    if (newID == null){
                        Console.Error.WriteLine("Impossible to create the link: impossible to get its ID");
                        linkCreated = false;
                    } else{
                        Dictionary<string, string> serverInfo = new Dictionary<string, string>(){
                            {"host", this.host}, {"port", this.port.ToString()},
                            {"projectID", this.projectID},
                        };
                        // Adds the new link to our list
                        Link newLink = new Link(
                            newID,
                            new Node[2]{node1, node2},
                            serverInfo, HTTPclient,
                            frequencyDrop,
                            packetLoss,
                            latency,
                            jitter,
                            corrupt
                        );
                        // Add the new link to the project list of links
                        links.Add(newLink);
                        // Add the new link to each node list of links
                        SaveLinksInfoInNodes(new List<Link>(){newLink});
                        // And then match the nodes ports
                        MatchLinkWithNodePorts(newLink, ExtractKeyNewLink(responseString, "nodes").ToString());
                        linkCreated = true;
                    }
                }
            } catch(JsonSerializationException err){
                Console.Error.WriteLine("Impossible to serialize the JSON to send it to the API: {0}", err.Message);
                linkCreated = false;
            } catch(HttpRequestException err){
                Console.Error.WriteLine("Some problem occured with the HTTP connection: {0}", err.Message);
                linkCreated = false;
            } catch(Exception err){
                Console.Error.WriteLine("Impossible to create the link: {0}", err.Message);
                linkCreated = false;
            }
        } else{
            Console.Error.WriteLine("Some of the chosen nodes doesn't exist");
            linkCreated = false;
        }        

        return linkCreated;
    }

    // Find the link two nodes share
    public Link GetLinkByNodes(Node node1, Node node2){
        // Return variable
        Link link;

        if (node1 != null && node2 != null){
            Link[] linkNode1 = node1.LinksAttached.ToArray();
            Link[] linkNode2 = node2.LinksAttached.ToArray();
            if (linkNode1 != null && linkNode2 != null){
                var sharedLink = linkNode1.Intersect(linkNode2);
                if (sharedLink.Count() > 0){
                    // In case the nodes share at least one common 
                    // link, it gets the first one
                    link = sharedLink.First();
                } else{
                    link = null;
                }
            } else{
                link = null;
            }
        } else{
            Console.Error.WriteLine("Some of the chosen nodes doesn't exist");
            link = null;
        }

        return link;
    }

    // Edit a link according to the parameters introduced
    public bool EditLink(Link link,
        int frequencyDrop=-10, int packetLoss=-10,
        int latency=-10, int jitter=-10, int corrupt=-10
        ){
        // Return variable
        bool linkEdited;
        if (link != null)
            linkEdited = link.EditLink(
                frequencyDrop, packetLoss, latency,
                jitter, corrupt
            );
        else 
            linkEdited = false;

        return linkEdited;
    }

    // Edit a link according to the parameters introduced
    public bool EditLink(Node node1, Node node2,
        int frequencyDrop=-10, int packetLoss=-10,
        int latency=-10, int jitter=-10, int corrupt=-10
        ){
        // Return variable
        bool linkEdited;
        if (node1 != null && node2 != null){
            linkEdited = this.EditLink(
                GetLinkByNodes(node1, node2),
                frequencyDrop, packetLoss,
                latency, jitter, corrupt
            );
        }
        else{
            Console.Error.WriteLine("Some of the chosen nodes doesn't exist");
            linkEdited = false;
        }

        return linkEdited;

    }

    // Remove a link of the project
    public bool RemoveLink(Link link){
        // Return variable
        bool linkRemoved;
        if (link != null){
            string URLHeader = $"http://{host}:{port}/v2/projects/{projectID}/links";

            try{

                if (HTTPclient.DeleteAsync($"{URLHeader}/{link.ID}").Result.IsSuccessStatusCode){
                    foreach(Node node in link.Nodes){
                        node.LinksAttached.Remove(link);
                        node.Ports.Single( x => x["link"] == link )["link"] = null;
                    }
                    this.links.Remove(link); link = null;
                    linkRemoved = true;
                } else{
                    linkRemoved = false;
                }

            } catch(HttpRequestException err){
                Console.Error.WriteLine("Some problem occured with the HTTP connection: {0}", err.Message);
                linkRemoved = false;
            } catch(Exception err){
                Console.Error.WriteLine("Impossible to remove the link: {0}", err.Message);
                linkRemoved = false;
            }
        } else{
            linkRemoved = false;
        }

        return linkRemoved;
    }

    // Remove a link of the project
    public bool RemoveLink(Node node1, Node node2){
        // Return variable
        bool linkRemoved;

        if (node1 != null && node2 != null)
            linkRemoved = this.RemoveLink(GetLinkByNodes(node1, node2));
        else{
            Console.Error.WriteLine("Some of the chosen nodes doesn't exist");
            linkRemoved = false;
        }

        return linkRemoved;
    }

    // Find the element that corresponds to a certain name.
    // A casting is compulsory in order to use the Node submethods.
    // Returns null if it can't be matched
    public Node GetNodeByName(string name){
        
        Node foundNode = null;
        try{
            foundNode = nodesByName[name];
        } catch{}
        return foundNode;

    }

    // Find the element that corresponds to a certain ID.
    // A casting is compulsory in order to use the Node submethods.
    // Returns null if it can't be matched
    public Node GetNodeByID(string ID){
        
        Node foundNode = null;
        try{
            foundNode = nodesByID[ID];
        } catch{}
        return foundNode;

    }

}