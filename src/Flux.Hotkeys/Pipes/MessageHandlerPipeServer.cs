using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Flux.Hotkeys.Pipes;

internal class MessageHandlerPipeServer : NamedPipeServer
{
    private readonly PipeMessageHandler m_messageHandler;

    public MessageHandlerPipeServer(string pipeName, PipeMessageHandler messageHandler) : base(pipeName) 
    {
        ArgumentNullException.ThrowIfNull(pipeName);
        ArgumentNullException.ThrowIfNull(messageHandler);

        m_messageHandler = messageHandler;
    }

    protected override string HandleClientMessage(string clientMessage) 
    {
        return m_messageHandler(clientMessage);
    }
}