using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.FindPlaybackFiles;

public class Response : AbstractQpSerializer<Response>
{
    protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_FindPlaybackFilesCommandSerializerContext.Default.Response;
    public VideoFileInfo[] Files { get; set; }
}