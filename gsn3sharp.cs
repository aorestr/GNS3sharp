using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
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
    public List<Dictionary<string,object>> NodesJSON{ get => nodesJSON; }

    // Same for the links
    private List<Dictionary<string,object>> linksJSON;
    public List<Dictionary<string,object>> LinksJSON{ get => linksJSON; }

    // List of nodes the project has
    private Node[] nodes; public Node[] Nodes{ get{return nodes;} }

    // List of links the project has. We will use a struct
    private Link[] links; public Link[] Links{ get{return links;} }

    // These dictionaries will help getting the nodes by name and ID
    private Dictionary<string, Node> nodesByName = new Dictionary<string, Node>();
    private Dictionary<string, Node> nodesByID = new Dictionary<string, Node>();

    // HTTP client used to make POST in order to start or stop the nodes
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
        nodesJSON = ExtractNodesDictionary($"{baseURL}/nodes");
        Console.WriteLine(" ok");
        Console.Write($"Extracting links information from URL: {baseURL}/links... ");
        linksJSON = ExtractLinksDictionary($"{baseURL}/links");
        Console.WriteLine(" ok");
        if (nodesJSON != null){
            // Create the nodes related to that info
            nodes = GetNodes(nodesJSON);
            links = GetLinks(linksJSON);
        }
    }

    ///////////////////////////////// Methods ////////////////////////////////////////////

    // It returns a dictionary with information about the nodes of the project
    private static List<Dictionary<string,object>> ExtractNodesDictionary(string URL){
        return ExtractDictionary(URL, "nodes");    
    }

    // It returns a dictionary with information about the project
    private static List<Dictionary<string,object>> ExtractLinksDictionary(string URL){
        return ExtractDictionary(URL, "links");    
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
            dictList = DeserializeJSON(json, lastElement);
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
    private static List<Dictionary<string,object>> DeserializeJSON(string json, string lastKey){
        
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

    // Create an array with the nodes. Each element is a Node instance
    private Node[] GetNodes(List<Dictionary<string,object>> JSON){
        // Return variable
        Node[] listOfNodes = new Node[JSON.Count];

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
                        node["node_id"].ToString()
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
    private Link[] GetLinks(List<Dictionary<string,object>> JSON){
        // Return variable
        Link[] listOfLinks = new Link[JSON.Count];
        // Function that returns the nodes connected by the link
        Node[] NodesConnected(string nodesJSON){
            // Return variable
            Node[] nodesList = new Node[2];
            List<Dictionary<string,object>> dictList = null;
            try{
                dictList = DeserializeJSON(nodesJSON, "port_number");
            } catch (Exception err){
                Console.Error.WriteLine(
                    "Some problem occured while trying to gather information about the nodes connect to the link: {0}",
                    err.Message
                );
            }
            if (dictList.Count != 0){
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
        try{
            foreach(Dictionary<string, object> link in JSON){
                try{
                    Console.Write($"Gathering information for link #{i}... ");
                    filtersJSON = JObject.Parse(link["filters"].ToString());
                    if (filtersJSON.HasValues){
                        // If the link has some filter activates
                        listOfLinks[i++] = new Link(
                            link["link_id"].ToString(),
                            NodesConnected(link["nodes"].ToString()),
                            ExtractFilter(filtersJSON, "frequency_drop"),
                            ExtractFilter(filtersJSON, "packet_loss"),
                            ExtractFilter(filtersJSON, "latency"),
                            ExtractFilter(filtersJSON, "jitter"),
                            ExtractFilter(filtersJSON, "corrupt")
                        );
                    } else{
                        // If don't
                        listOfLinks[i++] = new Link(
                            link["link_id"].ToString(),
                            NodesConnected(link["nodes"].ToString())
                        );
                    }
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
        string content = JsonConvert.SerializeObject(new Dictionary<string, string> { { "-d", "{}" } });
        ByteArrayContent byteContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content));
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        if (status.Equals("start"))
            Console.WriteLine("Activating all the nodes in the project...");
        else
            Console.WriteLine("Deactivating all the nodes in the project...");
        for (ushort i = 0; i < numNodes; i++){
            try{
                var res = HTTPclient.PostAsync(
                    $"{URLHeader}/{nodes[i].ID}/{status}", byteContent
                ).Result.Content.ReadAsStringAsync();
                totalMessages[i] = res.Result.ToString();
            } catch(Exception err){
                Console.Error.WriteLine("Impossible to {2} node {0}: {1}", nodes[i].Name, err.Message, status);
            }
        }
        Console.WriteLine("...ok");

        // If everything goes right, it returns info about every node
        return totalMessages;
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