using System;
using System.IO;
using System.Text;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Flux.Hotkeys.Pipes;

internal abstract class NamedPipeServer(string pipeName)
{
    private volatile NamedPipeServerStream? m_serverStream;

    private bool IsClientConnected => m_serverStream is { IsConnected: true };

    public void Start() 
    {
        m_serverStream = MakeNamedPipeServerStream(pipeName);
        m_serverStream.BeginWaitForConnection(DoConnectionLoop, null);
    }

    private async void DoConnectionLoop(IAsyncResult result) 
    {
        if (!result.IsCompleted || m_serverStream == null)
        {
            return;
        }

        // IOException = pipe is broken
        // ObjectDisposedException = cannot access closed pipe
        // OperationCanceledException - read was canceled
        // ACCEPT CLIENT CONNECTION
        try
        {
            m_serverStream.EndWaitForConnection(result);
        }
        catch (IOException)
        {
            RebuildNamedPipe();
            return;
        }
        catch (ObjectDisposedException)
        {
            RebuildNamedPipe();
            return;
        }
        catch (OperationCanceledException)
        {
            RebuildNamedPipe(); 
            return;
        }

        while (IsClientConnected) 
        {
            // READ FROM CLIENT
            if (m_serverStream == null)
            {
                break;
            }

            var clientMessage = await TryOrRebuildAsync(async () => await ReadClientMessage(m_serverStream));
            var serverResponse = HandleClientMessage(clientMessage);
            
            if (m_serverStream == null)
            {
                break;
            }

            await TryOrRebuildAsync(async () =>
            {
                await SendResponseToClient(m_serverStream, serverResponse);
                return "";
            });
        }

        // client disconnected, re-listen
        m_serverStream?.BeginWaitForConnection(DoConnectionLoop, null);
    }

    private async Task<string> TryOrRebuildAsync(Func<Task<string>> callback)
    {
        try
        {
            return await callback();
        }
        catch (IOException)
        {
            RebuildNamedPipe();
            return "";
        }
        catch (ObjectDisposedException)
        {
            RebuildNamedPipe();
            return "";
        }
        catch (OperationCanceledException)
        {
            RebuildNamedPipe(); 
            return "";
        }
    }

    private void RebuildNamedPipe() 
    {
        Shutdown();
        m_serverStream = MakeNamedPipeServerStream(pipeName);
        m_serverStream.BeginWaitForConnection(DoConnectionLoop, null);
    }

    protected abstract string HandleClientMessage(string clientMessage);

    private static async Task SendResponseToClient(NamedPipeServerStream stream, string serverResponse) 
    {
        var responseData = Encoding.Unicode.GetBytes(serverResponse);
        await stream.WriteAsync(responseData);
        await stream.FlushAsync();
        stream.WaitForPipeDrain();
    }

    private static async Task<string> ReadClientMessage(NamedPipeServerStream stream) 
    {
        var buffer = new byte[65535];
        var read = await stream.ReadAsync(buffer);
        var clientString = Encoding.Unicode.GetString(buffer, 0, read);
        return clientString;
    }

    public void Shutdown() 
    {
        if (m_serverStream != null) 
        {
            try
            {
                m_serverStream.Close(); 
                
            } 
            catch { /* ignored */ }

            try
            {
                m_serverStream.Dispose();
            } 
            catch { /* ignored */ }
            
            m_serverStream = null;
        }
    }


    private static NamedPipeServerStream MakeNamedPipeServerStream(string name) 
    {
        return new NamedPipeServerStream(name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
    }
}