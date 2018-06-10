using GNS3_UNITY_API;
using System.Text.RegularExpressions;

/*
Defines some methods that could be helpful for other classes
 */
public static class Aux{
    public static bool IsIP(string IP) => 
        Regex.IsMatch(IP, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
}