using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class RetrieveBearerToken
{
    [MenuItem("Debug/Give BearerCookie")] 
    private static void BearerCookie()
    {
        ReflectReturns("GetConnectAccessToken");
    }
    [MenuItem("Debug/Give PackageKey")] 
    private static void PackageKey()
    {
        ReflectReturns("GetPackagesKey");
    }
    private static void ReflectReturns(string methodName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var targetType = assemblies
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == "UnityEditor.Search.Utils");
        if (targetType == null)
        {
            throw new Exception("Could not find UnityEditor.Search.Utils class.");
        }
        var method = targetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null)
        {
            throw new Exception($"Could not find {methodName} method.");
        }
        GUIUtility.systemCopyBuffer = (string)method.Invoke(null, null);
    }
}