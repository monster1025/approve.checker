using GitLabApiClient.Models.Job.Responses;
using GitLabApiClient.Models.MergeRequests.Responses;
using ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto;
using ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto.ApprovalState;

namespace Approve.Checker.Gitlab;

public interface IGitlabCustomClient
{
    Task<Reviewers[]> GetReviewers(string projectId, int Iid);
    Task<ApprovalStateResponse> GetApprovals(string projectId, int Iid);
    Task<ApprovalStateResponse> GetDiscussions(string projectId, int Iid);
    Task<MergeRequest[]> GetMergeRequestsByTargetBranch(string targetBranch);
    Task<Job[]> GetPipelineTriggers(string projectId, int pipelineId);
    Task RestartJob(string projectId, int failedJobId);
    Task<MergeRequest[]> GetMergeRequestsBySourceBranch(string targetBranch);
    Task SetMrReviewer(string mrProjectId, int mergeRequestId, int reviewerId);
    Task DeleteEmoji(string projectId, int Iid, int emojiId);
}