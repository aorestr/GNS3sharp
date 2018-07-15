using System.Collections.Generic;

namespace GNS3sharp {
    /*
    Static class that gathers info about the projects the server contains
     */
    public static class ServerProjects {

        // Get the ID of a certain project through its name
        public static string GetProjectIDByName(string projectName, string host = "localhost", ushort port = 3080) {
            string id = null;
            List<Dictionary<string,object>> projects = null;
            try {
                projects = GNS3sharp.ExtractDictionary($"http://{host}:{port.ToString()}/v2/projects","zoom");
            } catch {}
            if (projects != null){
                foreach(Dictionary<string, object> project in projects){
                    if ( projectName.Equals(project["name"].ToString()) ){
                        id = project["project_id"].ToString();
                        break;
                    }
                }
            }
            return id;
        }
    }
}
