# Unity Packages Bulk downloader

# Requirements
* .NET Desktop Runtime [10.X.X](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) only for building
* Tested only with unity 2022.3.22f1

# Usage
```
--output-dir <output-dir>  Output Directory [default: ./UnityPackages]
--bearer <bearer>          Bearer Token []
-?, -h, --help             Show help and usage information
```


# Obtaining bearer token
1. Install the Editor Extension from [RetrieveBearerToken.cs](Unity/Editor/RetrieveBearerToken.cs) by placing into a folder called `Editor` or from UnityPackage in [Releases](https://github.com/Myrkie/Unity-Package-Downloader/releases)
2. Open package manager and ensure you are signed in, Window > Package Manager | Set packages to > My Assets
3. Select Debug > Give BearerCookie.
4. Your authorization cookie will be copied to clipboard use this with the `--bearer` argument