using System.Text.Json.Serialization;

namespace Server.WebHook
{
    /// <summary>
    /// WebHook 外送载荷的 System.Text.Json 源生成上下文（AOT/裁剪友好，序列化契约与反射模式保持一致）。
    /// 仅覆盖外送的具体载荷类型；基类属性随派生类型元数据一并生成。
    /// </summary>
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(OnlineCountPeriodicPayload))]
    [JsonSerializable(typeof(ThresholdCrossingPayload))]
    [JsonSerializable(typeof(LoginResultPayload))]
    internal partial class WebHookJsonContext : JsonSerializerContext
    {
    }
}
