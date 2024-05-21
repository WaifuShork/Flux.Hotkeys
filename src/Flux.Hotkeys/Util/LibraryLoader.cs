using System;
using System.IO;
using Flux.Hotkeys.Util.Exceptions;

namespace Flux.Hotkeys.Util;

internal static class LibraryLoader
{
    private static readonly Lazy<SafeLibraryHandle> s_dllHandle = new Lazy<SafeLibraryHandle>(LoadDll);

    internal static void EnsureDllIsLoaded() 
    {
        if (s_dllHandle.IsValueCreated)
        {
            return;
        }

        _ = s_dllHandle.Value;
    }

    private static SafeLibraryHandle LoadDll() 
    {
        var processorType = Environment.Is64BitProcess ? "x64" : "x86";
        var relativePath = $"{processorType}/AutoHotkey.dll";

        // If for some reason the file is already on the disk,
        // then we can just load that instead of extracting from resources
        return File.Exists(relativePath) 
            ? SafeLibraryHandle.LoadLibrary(relativePath) 
            : ExtractAndLoadEmbeddedResource(relativePath);
    }

    private static SafeLibraryHandle ExtractAndLoadEmbeddedResource(string relativePath) 
    {
        var assembly = typeof(Ahk).Assembly;
        var resource = EmbeddedResources.FindByName(assembly, relativePath);

        if (resource is not null) 
        {
            var tempFolderPath = GetTempFolderPath();
            var outputFile = Path.Combine(tempFolderPath, relativePath);
            EmbeddedResources.ExtractToFile(assembly, resource, outputFile);
            return SafeLibraryHandle.LoadLibrary(outputFile);
        }

        throw new AhkException("Unable to load AutoHotkey.dll");
    }

    private static string GetTempFolderPath() 
    {
        var temp = Path.GetTempPath();
        const string ahkTempName = "Flux.Hotkeys";
        var version = typeof(Ahk).Assembly.GetName().Version?.ToString() ?? "";
        return Path.Combine(temp, ahkTempName, version);
    }
}