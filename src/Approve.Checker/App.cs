using System.Text;
using Approve.Checker.Gitlab;
using GitLabApiClient;
using GitLabApiClient.Models.Commits.Responses;
using GitLabApiClient.Models.MergeRequests.Responses;
using GitLabApiClient.Models.Notes.Requests;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Settings = Approve.Checker.Gitlab.Settings;

namespace Approve.Checker;

public class App : IApp
{
    private readonly GitLabClient _client;
    private readonly Settings _settings;
    private readonly GitlabCustomClient _customClient;

    public App()
    {
        var httpHandler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        _settings = new Settings();
        _client = new GitLabClient(_settings.GitlabUrl, _settings.GitlabToken, httpHandler);
        _customClient = new GitlabCustomClient(_settings.GitlabUrl, _settings.GitlabToken, httpHandler);
    }

    public async Task<int> Run()
    {
        if (string.IsNullOrEmpty(_settings.Approvers))
        {
            Console.WriteLine("Approvers in empty, please define APPROVERS_FILEPATH.");
            return 0;
        }
        if (string.IsNullOrEmpty(_settings.MrPath))
        {
            Console.WriteLine("Not in MergeRequest. Exiting.");
            return 0;
        }
        var approversDict = ParseApprovers();
        if (approversDict is null)
        {
            return -1;
        }

        var mrId = int.Parse(_settings.MrPath.Split('!')[1]);
        
        var mergeRequest = await _client.MergeRequests.GetAsync(_settings.ProjectId, mrId);
        var commit = await _client.Commits.GetAsync(_settings.ProjectId, mergeRequest.Sha);

        var mrsToSameRelease = await _customClient.GetMergeRequestsByTargetBranch(mergeRequest.TargetBranch);
        var sameMrs = mrsToSameRelease.Where(f => f.SourceBranch == mergeRequest.SourceBranch && 
                                                  f.TargetBranch == mergeRequest.TargetBranch &&
                                                  f.State == MergeRequestState.Merged).ToArray();
        if (sameMrs.Any())
        {
            var mrUrls = string.Join("\r\n", sameMrs.Select(f=>f.WebUrl));
            var message = $"Аналогичный MR ({sameMrs.First().WebUrl}) уже был одобрен и влит, автоматически одобряю текущий:\r\n{mrUrls}";

            await _client.MergeRequests.CreateNoteAsync(mergeRequest.ProjectId, mergeRequest.Iid,
                new CreateMergeRequestNoteRequest(message));
            return 0;
        }

        var isCodeFreezePeriod = _settings.ReleaseCodeFreeze == "true";
        var periodDescription = isCodeFreezePeriod ? "Code-Freeze. Restrictions may apply" : "Normal";
        Console.WriteLine($"Current period is {periodDescription}.");

        var filtered = FilterApprovers(approversDict, isCodeFreezePeriod);
        if (!filtered.Any() && isCodeFreezePeriod)
        {
            filtered = FilterApprovers(approversDict, false);
        }
        if (!await IsApproved(mergeRequest, commit, filtered))
        {
            return -1;
        }

        return 0;
    }

    private async Task<bool> IsApproved(MergeRequest mergeRequest, Commit commit, Dictionary<string, ApproversDto> approversDict)
    {
        var emojis = await _client.MergeRequests.GetAwardEmojisAsync(_settings.ProjectId, mergeRequest.Iid);

        var sb = new StringBuilder();
        foreach (var emoji in emojis)
        {
            if (emoji.UpdatedAt <= commit.CommittedDate)
            {
                sb.AppendLine($" - Удален {emoji.Name} от {emoji.User.Username} ({emoji.UpdatedAt}), т.к. после него были коммиты ({commit.CommittedDate}).");
                await _customClient.DeleteEmoji(mergeRequest.ProjectId, mergeRequest.Iid, emoji.Id);
            }
        }
        if (sb.Length > 0)
        {
            await _client.MergeRequests.CreateNoteAsync(mergeRequest.ProjectId, mergeRequest.Iid,
                new CreateMergeRequestNoteRequest(sb.ToString()));
        }
        emojis = emojis.Where(f => f.UpdatedAt > commit.CommittedDate).ToArray();

        foreach (var approversGroup in approversDict)
        {
            var needCount = approversGroup.Value.CountApprovers;
            var groupApproves = 0;
            var approvedInGroup = new List<string>();
            foreach (var approveUser in approversGroup.Value.Users)
            {
                if (emojis.Any(f => f.User.Username == approveUser))
                {
                    approvedInGroup.Add(approveUser);
                    groupApproves++;
                    if (groupApproves >= needCount)
                    {
                        var approvers = string.Join(",", approvedInGroup);
                        Console.WriteLine($"[+] Approved by group of {approversGroup.Key} ({approvers}).");
                        break;
                    }
                }
            }

            if (groupApproves < needCount)
            {
                Console.WriteLine($"[-] Need to be approved by group of {approversGroup.Key} (current approves: {groupApproves}, need: {needCount}):");
                foreach (var approveUser in approversGroup.Value.Users)
                {
                    Console.WriteLine($"  - {approveUser}");
                }
                return false;
            }
        }

        return true;
    }

    private Dictionary<string, ApproversDto>? ParseApprovers()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        try
        {
            return deserializer.Deserialize<Dictionary<string, ApproversDto>>(_settings.Approvers);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while deserializing approvers: {ex}");
            return null;
        }
    }

    private Dictionary<string, ApproversDto> FilterApprovers(Dictionary<string, ApproversDto> approvers, bool isCodeFreezePeriod)
    {
        var result = new Dictionary<string, ApproversDto>();
        foreach (var approversDto in approvers)
        {
            if (approversDto.Value.ForCodeFreeze == isCodeFreezePeriod)
            {
                result.Add(approversDto.Key, approversDto.Value);
            }
        }
        return result;
    }
}