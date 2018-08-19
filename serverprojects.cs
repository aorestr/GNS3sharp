using System.Collections.Generic;

namespace GNS3sharp {
    /*
    Static class that gathers info about the projects the server contains
     */
    /// <summary>
    /// Static class that gathers info about the projects the server contains. Methods:
    /// <list type="bullet">
    /// <item>
    /// <term>GetProjectIDByName</term>
    /// <description>Return the ID of a project giving its name</description>
    /// </item>
    /// </list>
    /// </summary>
    public static class ServerProjects {

        /// <summary>
        /// Return the ID of a project giving its name. It's case sensitive
        /// </summary>
        /// <param name="projectName">Name of the GNS3 project which ID you plan to get</param>
        /// <param name="host">IP where the GNS3 server is hosted. "localhost" by default</param>
        /// <param name="port">Port where the server is hosted. 3080 by default</param>
        /// <returns></returns>
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
