using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/*
It handles the connections with the different components of
a GNS3 project
 */
public class GNS3sharp {

    // List of nodes inside the project with all the info about them.
    // The info is not filtered
    private List<Dictionary<string,object>> projectJSON;

    // List of nodes the project has
    private Node[] nodes;

    // Wrong constructor. It needs an ID for the project
    public GNS3sharp() {
        Console.Error.WriteLine("You need the project ID");
    }

    // Right constructor. Needs the project ID
    public GNS3sharp(string projectID) {
        // Defines the URL in which the nodes info is
        string projectURL = $"http://localhost:3080/v2/projects/{projectID}/nodes";
        // Extract that info
        projectJSON = extractProjectJSON(projectURL);
        if (projectJSON != null){
            // Create the nodes related to that info
            nodes = getNodes(projectJSON);
            /*
            // Show every node information
            foreach(Node n in nodes){
                Console.WriteLine("host: {0}, port: {1}, name: {2}, component: {3}",
                    n.getConsoleHost(), n.getPort(), n.getName(), n.getType());
            }
            */
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
        } catch(Exception){
            // Server not open
            Console.Error.WriteLine("Impossible to connect to project {0}",projectURL);
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
            if (symbol.Contains("pc"))
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
                    listOfNodes[i++] = new Node(
                        node["console_host"].ToString(), 
                        Int32.Parse(node["console"].ToString()), 
                        node["name"].ToString(), 
                        extractComponent(node["symbol"].ToString())
                    );
                } catch(Exception){
                    Console.Error.WriteLine($"Impossible to save the configuration for the node {i}");
                }
            }
        } catch(Exception){
            Console.Error.WriteLine("Some problem occured while saving the nodes information");
            listOfNodes = null;
        }

        return listOfNodes;
    }
}