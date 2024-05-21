using System;
using Cysharp.Text;
using Flux.Hotkeys.Util.Exceptions;

namespace Flux.Hotkeys.Pipes;

[PublicAPI]
public delegate string PipeMessageHandler(string message);


[PublicAPI]
public static class AhkPipes
{
    private static readonly object s_lockObj = new object();
    internal static MessageHandlerPipeServer? Server { get; private set; }
    
    // ReSharper disable once InconsistentNaming
    private const string A__PIPECLIENT = "A__PIPECLIENT";

    public static void LoadPipesModule(PipeMessageHandler messageHandler) 
    { 
        lock (s_lockObj) 
        {
            var pipeName = GeneratePipeName();
            InitPipeServer(pipeName, messageHandler);
            InitPipeClient(pipeName);
        }
    }

    private static void InitPipeClient(string pipeName) 
    {
        // only load pipe client once, by checking for pipeclient_getversion function
        var buffer = ZString.CreateStringBuilder();
        if (!Ahk.Func.Exists("PipeClient_GetVersion")) 
        {
            var ahkPipeClientScript = Util.EmbeddedResources.ExtractToText(typeof(AhkPipes).Assembly, "Pipes/PipeClient.ahk");

            if (ahkPipeClientScript is null)
            {
                throw new AhkException("Unable to load PipeClient");
            }

            buffer.AppendLine(ahkPipeClientScript);
        }
        else
        {
            buffer.AppendLine($"{A__PIPECLIENT}.Close()");
        }
        
        buffer.AppendLine($"{A__PIPECLIENT} := new PipeClient({AhkEscape.Quote(pipeName)})");
        var code = buffer.ToString();
        if (!Ahk.AddSnippet(code))
        {
            throw new AhkException($"Unable to execute code\n{code}");
        }
    }

    private static void InitPipeServer(string pipeName, PipeMessageHandler messageHandler) 
    {
        // If one is already running nuke it 
        Server?.Shutdown();

        Server = new MessageHandlerPipeServer(pipeName, messageHandler);
        Server.Start();
    }

    private static string GeneratePipeName()
    {
        return $"AHK-PIPE-{Guid.NewGuid().ToString().Replace("-", "")}";
    }
}