namespace ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto;
#pragma warning disable CS8618

public class Reviewers
{
    public User user { get; set; }
    public string state { get; set; }
    public DateTime created_at { get; set; }
}