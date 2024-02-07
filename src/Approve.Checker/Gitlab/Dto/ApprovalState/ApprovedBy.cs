namespace ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto.ApprovalState;
#pragma warning disable CS8618

public class ApprovedBy
{
    public GitLabApiClient.Models.Users.Responses.User user { get; set; }
}