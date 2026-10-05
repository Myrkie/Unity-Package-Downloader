using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class RetrieveBearerToken
{
    [MenuItem("Debug/Give BearerCookie")]
    private static void BearerCookie()
    {
        ReflectReturnsAfterPackageManagerReady("GetConnectAccessToken");
    }

    [MenuItem("Debug/Give PackageKey")]
    private static void PackageKey()
    {
        ReflectReturnsAfterPackageManagerReady("GetPackagesKey");
    }

    private static void ReflectReturnsAfterPackageManagerReady(string methodName)
    {
        ListRequest request;

        try
        {
            request = Client.List();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not start Package Manager request: {ex}");
            return;
        }

        EditorApplication.update += WaitForPackageManager;
        void WaitForPackageManager()
        {
            if (!request.IsCompleted)
                return;

            EditorApplication.update -= WaitForPackageManager;

            if (request.Status != StatusCode.Success)
            {
                Debug.LogError($"Package Manager session/request is not valid: {request.Error?.message}");
                return;
            }

            try
            {
                CopyReflectedValue(methodName);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    private static void CopyReflectedValue(string methodName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var targetType = assemblies.SelectMany(a => {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }).FirstOrDefault(t => t.FullName == "UnityEditor.Search.Utils");

        if (targetType == null)
        {
            throw new Exception("Could not find UnityEditor.Search.Utils class.");
        }

        var method = targetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null)
        {
            throw new Exception($"Could not find {methodName} method.");
        }

        var value = method.Invoke(null, null) as string;

        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"{methodName} returned an empty value. " +
                                "The Unity Package Manager/Unity account session may not be ready.");
        }
        GUIUtility.systemCopyBuffer = value;
        Debug.Log($"Successfully copied {methodName} result to clipboard.");
    }
}