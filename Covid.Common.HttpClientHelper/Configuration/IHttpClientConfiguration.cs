namespace Covid.Common.HttpClientHelper.Configuration
{
    public interface IHttpClientConfiguration
    {
        string Name { get; set; }
        string RootUri { get; set; }
    }
}