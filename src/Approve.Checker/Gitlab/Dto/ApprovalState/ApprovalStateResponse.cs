// ReSharper disable UnusedMember.Global
namespace ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto.ApprovalState;
#pragma warning disable CS8618

public class ApprovalStateResponse
{
    public bool user_has_approved { get; set; }
    public bool user_can_approve { get; set; }
    public bool approved { get; set; }
    public List<ApprovedBy> approved_by { get; set; }
}