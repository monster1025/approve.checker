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
        var mrId = int.Parse(_settings.MrPath.Split('!')[1]);
        
        var mergeRequest = await _client.MergeRequests.GetAsync(_settings.ProjectId, mrId);
        var commit = await _client.Commits.GetAsync(_settings.ProjectId, mergeRequest.Sha);
        if (!await IsApproved(mergeRequest, commit))
        {
            return -1;
        }

        return 0;
    }

    private async Task<bool> IsApproved(MergeRequest mergeRequest, Commit commit)
    {
        var approversDict = ParseApprovers();
        if (approversDict is null)
        {
            return false;
        }
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
}