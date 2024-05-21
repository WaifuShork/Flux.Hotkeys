using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Flux.Hotkeys.Util;

internal static partial class EmbeddedResources
{
    public static string? FindByName(Assembly assembly, string path) 
    {
        path = Regex.Replace(path, "."); // replace slashes with periods
        var pathWithDotPrefix = path.StartsWith('.') ? path : $".{path}";
            
        var names = assembly.GetManifestResourceNames();

        foreach (var name in names) 
        {
            if (name.EndsWith(pathWithDotPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return name;
            }

            if (name.Equals(path, StringComparison.InvariantCultureIgnoreCase))
            {
                return name;
            }
        }

        return null;
    }
        

    public static void ExtractToFile(Assembly assembly, string embeddedResourceName, string outputFilePath) 
    {
        if (File.Exists(outputFilePath))
        {
            return;
        }
            
        var fullResourceName = FindByName(assembly, embeddedResourceName);

        if (fullResourceName is null)
        {
            throw new FileNotFoundException($"Cannot find resource name of '{embeddedResourceName}' in assembly '{assembly.GetName().Name}'", embeddedResourceName);
        }

        EnsureDirectoryExistsForFile(outputFilePath);

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            return;
        }
            
        using var readStream = assembly.GetManifestResourceStream(fullResourceName);
        using var writeStream = File.Open(outputFilePath, FileMode.Create);
        readStream?.CopyTo(writeStream);
        readStream?.Flush();
    }

    public static string? ExtractToText(Assembly assembly, string embeddedResourceName) 
    {
        var fullResourceName = FindByName(assembly, embeddedResourceName);
        if (string.IsNullOrWhiteSpace(fullResourceName))
        {
            throw new FileNotFoundException($"Cannot find resource name of '{embeddedResourceName}' in assembly '{assembly.GetName().Name}'", embeddedResourceName);
        }

        using var readStream = assembly.GetManifestResourceStream(fullResourceName);
        if (readStream is not null)
        {
            using var reader = new StreamReader(readStream);
            return reader.ReadToEnd();
        }

        return null;
    }

    private static void EnsureDirectoryExistsForFile(string targetFileName) 
    {
        var absolutePath = Path.GetFullPath(targetFileName);
        var directoryPath = Path.GetDirectoryName(absolutePath);
            
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    [GeneratedRegex(@"[/\\]")]
    private static partial Regex ResourceRegex();

    private static Regex Regex => ResourceRegex();
}