using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/*
It handles the connections with the different components of
a GNS3 project
 */
public class GNS3sharp {

    // Wrong constructor. It needs an ID for the project
    public GNS3sharp() {
        Console.Error.WriteLine("You need the project ID");
    }

    // Right constructor. Needs the project ID
    public GNS3sharp(string projectID) {
        string projectURL = $"http://localhost:3080/v2/projects/{projectID}/nodes";
        projectJSON(projectURL);
    }

    // It returns a dictionary with information about the nodes of the project
    private static List<Dictionary<string,object>> projectJSON(string projectURL){

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

        // Creates a list of dictionaris. It will be used to store the JSON info and
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
            /* // Show content of the list
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
}