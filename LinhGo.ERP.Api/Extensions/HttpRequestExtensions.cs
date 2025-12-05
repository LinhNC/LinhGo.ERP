namespace LinhGo.ERP.Api.Extensions;

public static class HttpRequestExtensions
{
    public const string ForwardedProtoHeader = "X-Forwarded-Proto";
    public const string ForwardedHostHeader = "X-Forwarded-Host";
    public const string OriginalPathPrefixHeader = "X-Original-Path-Prefix";

    public static string GetOriginalUrl(this HttpRequest httpRequest)
    {
        httpRequest.Headers.TryGetValue(ForwardedProtoHeader, out var forwardedProtos);
        httpRequest.Headers.TryGetValue(ForwardedHostHeader, out var forwardedHosts);

        var forwardedProto = forwardedProtos.FirstOrDefault() ?? httpRequest.Scheme;
        var forwardedHost = forwardedHosts.FirstOrDefault() ?? httpRequest.Host.Value;
        var originalPathPrefix = httpRequest.GetOriginalPathPrefix();

        return $"{forwardedProto}://{forwardedHost}/{originalPathPrefix}";
    }

    public static string GetOriginalPathPrefix(this HttpRequest httpRequest)
    {
        httpRequest.Headers.TryGetValue(OriginalPathPrefixHeader, out var originalPathPrefixes);

        var originalPathPrefix = originalPathPrefixes.FirstOrDefault() ?? string.Empty;

        originalPathPrefix = originalPathPrefix.TrimStart('/');

        return originalPathPrefix;
    }

    public static List<KeyValuePair<string, string>> ConvertObToKeyPairNewEngine(object obj)
    {
        var list = new List<KeyValuePair<string, string>>();

        var propInfos = obj.GetType().GetProperties();
        foreach (var item in propInfos)
            if (item.GetValue(obj) != null &&
                item.CustomAttributes.All(a => a.AttributeType.Name != "IgnoreConvertAttribute"))
                list.Add(new KeyValuePair<string, string>(item.Name, (item.GetValue(obj) ?? "").ToString()));

        return list;
    }
}