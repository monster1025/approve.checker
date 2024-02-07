using System.Net.Http.Json;
using System.Text;
using GitLabApiClient.Models.Job.Responses;
using GitLabApiClient.Models.MergeRequests.Responses;
using Newtonsoft.Json;
using ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto;
using ReleaseScripts.ExternalSystems.Gitlab.CustomClient.Dto.ApprovalState;

namespace Approve.Checker.Gitlab;

public class GitlabCustomClient : IGitlabCustomClient
{
    private readonly string _url;
    private readonly string _token;
    private readonly HttpClient _client;

    public GitlabCustomClient(string url, string token, HttpClientHandler? httpClientHandler = null)
    {
        _url = url;
        _token = token;
        _client = new HttpClient(httpClientHandler ?? new HttpClientHandler());
    }

    public async Task<Reviewers[]> GetReviewers(string projectId, int Iid)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_url}/api/v4/projects/{projectId}/merge_requests/{Iid}/reviewers");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<Reviewers[]>();
        return responseObj!;
    }

    public async Task<ApprovalStateResponse> GetApprovals(string projectId, int Iid)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_url}/api/v4/projects/{projectId}/merge_requests/{Iid}/approvals");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<ApprovalStateResponse>();
        return responseObj!;
    }

    public async Task<ApprovalStateResponse> GetDiscussions(string projectId, int Iid)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_url}/api/v4/projects/{projectId}/merge_requests/{Iid}/discussions");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<ApprovalStateResponse>();
        return responseObj!;
    }

    public async Task<MergeRequest[]> GetMergeRequestsByTargetBranch(string targetBranch)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_url}/api/v4/merge_requests?target_branch={targetBranch}&scope=all&per_page=1000");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseStr = await response.Content.ReadAsStringAsync();
        var responseObj = JsonConvert.DeserializeObject<MergeRequest[]>(responseStr)!;
        return responseObj;
    }

    public async Task<MergeRequest[]> GetMergeRequestsBySourceBranch(string targetBranch)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_url}/api/v4/merge_requests?source_branch={targetBranch}&scope=all&per_page=1000");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseStr = await response.Content.ReadAsStringAsync();
        var responseObj = JsonConvert.DeserializeObject<MergeRequest[]>(responseStr)!;
        return responseObj;
    }

    public async Task SetMrReviewer(string mrProjectId, int mergeRequestId, int reviewerId)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_url}/api/v4/projects/{mrProjectId}/merge_requests/{mergeRequestId}");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var json = "{\"reviewer_ids\": [" + reviewerId + "]}";
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }


    public async Task<Job[]> GetPipelineTriggers(string projectId, int pipelineId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_url}/api/v4/projects/{projectId}/pipelines/{pipelineId}/bridges?per_page=100&page=1&include_retried=true");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseStr = await response.Content.ReadAsStringAsync();
        var responseObj = JsonConvert.DeserializeObject<Job[]>(responseStr)!;
        return responseObj;
    }

    public async Task RestartJob(string projectId, int failedJobId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_url}/api/v4/projects/{projectId}/jobs/{failedJobId}/retry");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEmoji(string projectId, int Iid, int emojiId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_url}/api/v4/projects/{projectId}/merge_requests/{Iid}/award_emoji/{emojiId}");
        request.Headers.Add("PRIVATE-TOKEN", _token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}