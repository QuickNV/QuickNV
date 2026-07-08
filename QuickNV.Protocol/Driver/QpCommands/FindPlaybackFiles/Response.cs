using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.FindPlaybackFiles;

public class Response : AbstractQpSerializer<Response>
{
    protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_FindPlaybackFilesCommandSerializerContext.Default.Response;
    public VideoFileInfo[] Files { get; set; }
}