#pragma warning disable CS8618
namespace ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto;

public class User
{
    public int id { get; set; }
    public string username { get; set; }
    public string name { get; set; }
    public string state { get; set; }
    public string avatar_url { get; set; }
    public string web_url { get; set; }
}