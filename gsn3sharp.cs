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
    private static void projectJSON(string projectURL){

        // Variable with a string with all the JSON info
        string json;
        using (System.Net.WebClient GNS3NodesProject = new System.Net.WebClient()){
            json = GNS3NodesProject.DownloadString(projectURL);
        }

        
        //Console.WriteLine("{0}", json);

        JArray a = JArray.Parse(json);
        List<Dictionary<string,object>> dict = new List<Dictionary<string, object>>();
        Dictionary<string,object> ui = new Dictionary<string, object>();
        foreach (JObject o in a.Children<JObject>())
        {
            foreach (JProperty p in o.Properties())
            {
                
                string name = p.Name;
                object value = (object)p.Value;
                Console.WriteLine("{0}:{1}",name, value);
                ui.Add(name,value);
                if (p.Name == "z") {
                    dict.Add(ui);
                    ui.Clear();
                }
            }
        }
            Console.WriteLine("Key = {0}",dict.Count);
    }
}