using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using GNS3_UNITY_API;

/*
It handles the connections with the different components of
a GNS3 project
 */
public class GNS3sharp {

    // Project ID
    private string projectID; public string ProjectID{ get => projectID; }
    // List of nodes inside the project with all the info about them.
    // The info is not filtered
    private List<Dictionary<string,object>> projectJSON;
    public List<Dictionary<string,object>> ProjectJSON{ get => projectJSON; }

    // List of nodes the project has
    private Node[] nodes; public Node[] Nodes{ get{return nodes;} }

    // Wrong constructor. It needs an ID for the project
    public GNS3sharp() {
        Console.Error.WriteLine("You need the project ID");
    }

    // Right constructor. Needs the project ID. Just get the nodes
    // the project has
    public GNS3sharp(string _projectID, ushort GNS3Port = 3080) {
        this.projectID = _projectID;
        // Defines the URL in which the nodes info is
        string projectURL = $"http://localhost:{GNS3Port.ToString()}/v2/projects/{_projectID}/nodes";
        // Extract that info
        projectJSON = extractProjectJSON(projectURL);
        if (projectJSON != null){
            // Create the nodes related to that info
            nodes = getNodes(projectJSON);
        }

    }

    // It returns a dictionary with information about the nodes of the project
    private static List<Dictionary<string,object>> extractProjectJSON(string projectURL){

        // Variable with a string with all the JSON info
        string json;
        try{
            // Get the info from the JSON file you can access from the GNS3 Rest service
            using (System.Net.WebClient GNS3NodesProject = new System.Net.WebClient()){
                json = GNS3NodesProject.DownloadString(projectURL);
            }
        } catch(Exception err){
            // Server not open
            Console.Error.WriteLine("Impossible to connect to project {0}: {1}",projectURL, err.Message);
            json = null;
        }

        // Creates a list of dictionaries. It will be used to store the JSON info and
        // get the values from it
        List<Dictionary<string,object>> dictList = new List<Dictionary<string,object>>();
        if(json == "[]"){
            Console.Error.WriteLine("JSON is empty");
            dictList = null;
        } else if (string.IsNullOrEmpty(json) == false){
            // JSON array object
            JArray jsonArray = JArray.Parse(json);
            Dictionary<string,object> tempDict = new Dictionary<string, object>();

            // Variables in which store the JSON info temporaly
            string name; object value;
            
            foreach (JObject jO in jsonArray.Children<JObject>()) {
                foreach (JProperty jP in jO.Properties()) {                
                    name = jP.Name;
                    value = (object)jP.Value;
                    tempDict.Add(name,value);
                    // The last key of every node is 'z'
                    if (jP.Name == "z") {
                        // If we do not copy the content of the dictionary into another
                        // we will be copying by reference and erase the content once
                        // we 'clear' the dict
                        Dictionary<string, object> copyDict = new Dictionary<string, object>(tempDict);
                        dictList.Add(copyDict);
                        tempDict.Clear();
                    }
                }
            }
            /*
            // Show content of the list
            foreach(Dictionary<string, object> dict in dictList){
                foreach (KeyValuePair<string, object> kvp in dict){
                    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                }
            }
            */
            return dictList;
        } else{
            dictList = null;
        }
        return dictList;    
    }

    // Create an array with the nodes. Each element is a Node instance
    private static Node[] getNodes(List<Dictionary<string,object>> JSON){
        Node[] listOfNodes = new Node[JSON.Count];

        // Guess what kind of component is the node by its symbol name
        int extractComponent(string symbol){
            int component;
            // Map every component with a number. We need the
            // relation between them for node.cs. This thing must
            // be changed in order to handle more appliances
            if (symbol.Contains("pc") || symbol.Contains("guest"))
                component = 1;
            else if(symbol.Contains("router"))
                component = 2;
            else if(symbol.Contains("switch"))
                component = 3;
            else
                component = 0;
            return component;
        }

        int i = 0;
        try{
            foreach(Dictionary<string, object> node in JSON){
                try{
                    switch(extractComponent(node["symbol"].ToString())){
                        case 1:
                            listOfNodes[i++] = new VPC(
                                node["console_host"].ToString(), 
                                UInt16.Parse(node["console"].ToString()), 
                                node["name"].ToString(),
                                node["node_id"].ToString()
                            ); break;
                        case 2:
                            listOfNodes[i++] = new Router(
                                node["console_host"].ToString(), 
                                UInt16.Parse(node["console"].ToString()), 
                                node["name"].ToString(),
                                node["node_id"].ToString()
                            ); break;
                        case 3:
                            listOfNodes[i++] = new Switch(
                                node["console_host"].ToString(), 
                                UInt16.Parse(node["console"].ToString()), 
                                node["name"].ToString(),
                                node["node_id"].ToString()
                            ); break;
                        default:
                            listOfNodes[i++] = null; break;
                    }
                } catch(Exception err1){
                    Console.Error.WriteLine(
                        "Impossible to save the configuration for the node {0}: {1}", 
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

}