using GNS3_UNITY_API;
using System.Text.RegularExpressions;

/*
Defines some methods that could be helpful for other classes
 */
public static class Aux{
    public static bool IsIP(string IP) => 
        Regex.IsMatch(IP, @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
}