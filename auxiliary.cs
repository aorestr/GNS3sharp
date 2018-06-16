using GNS3_UNITY_API;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

/*
Defines some methods that are helpful for other classes
 */
public static class Aux{

    // This array of dictionaries contains the different classes of nodes and
    // their 'label's. The 'label's are the strings you must between brackets
    // in the node name in the GNS3 project
    public static ImmutableArray<Dictionary<string,object>> nodesAvailables = ImmutableArray.Create(
        new Dictionary<string,object>(){
            {"class", typeof(VPC)}, {"label", VPC.label}
        },
        new Dictionary<string,object>(){
            {"class", typeof(MicroCore)}, {"label", MicroCore.label}
        },
        new Dictionary<string,object>(){
            {"class", typeof(OpenvSwitch)}, {"label", OpenvSwitch.label}
        },
        new Dictionary<string,object>(){
            {"class", typeof(EthernetSwitch)}, {"label", EthernetSwitch.label}
        },
        new Dictionary<string,object>(){
            {"class", typeof(OpenWRT)}, {"label", OpenWRT.label}
        }
    );

    // It returns the right class type for every node
    public static Type NodeType(string nodeName){

        // If something goes wrong and the label is not properly set on the
        // name, it returns the generic Node class
        Type newNode = typeof(Node);
        Match match = Regex.Match(nodeName, @"(?<=\[).+?(?=\])");

        if (match.Success) {
            string label = match.Groups[0].Value.ToUpperInvariant();
            foreach(Dictionary<string,object> typeOfNode in nodesAvailables){
                if (label.Equals(typeOfNode["label"].ToString())){
                    newNode = (Type)typeOfNode["class"];
                    break;
                }
            }
        }
        return newNode;
    }

    // Guess whether a string is an IP or not
    public static bool IsIP(string IP) => 
        Regex.IsMatch(IP, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");

    // Guess whether a string is a netmask or not
    public static bool IsNetmask(string netmask) =>
        Regex.IsMatch(netmask, @"^(((255\.){3}(255|254|252|248|240|224|192|128|0+))|((255\.){2}(255|254|252|248|240|224|192|128|0+)\.0)|((255\.)(255|254|252|248|240|224|192|128|0+)(\.0+){2})|((255|254|252|248|240|224|192|128|0+)(\.0+){3}))$");
}