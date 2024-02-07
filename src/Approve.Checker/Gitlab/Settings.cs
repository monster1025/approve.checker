namespace Approve.Checker.Gitlab
{
    public class Settings
    {
        public string GitlabUrl = Environment.GetEnvironmentVariable("CI_SERVER_URL") ?? CheckAndThrow("CI_SERVER_URL");
        public string GitlabToken = Environment.GetEnvironmentVariable("GITLAB_TOKEN") ?? CheckAndThrow("GITLAB_TOKEN");
        public string MrPath = Environment.GetEnvironmentVariable("CI_OPEN_MERGE_REQUESTS")!;
        public string ProjectId = Environment.GetEnvironmentVariable("CI_PROJECT_ID") ?? CheckAndThrow("CI_PROJECT_ID");
        public string Approvers = Environment.GetEnvironmentVariable("APPROVERS_FILEPATH") ?? CheckAndThrow("APPROVERS_FILEPATH");
        public string ReleaseCodeFreeze = Environment.GetEnvironmentVariable("RELEASE_CODEFREEZE") ?? "false";

        private static string CheckAndThrow(string name)
        {
            Console.WriteLine($"Please define {name}");
            throw new ArgumentException($"Please define {name}");
        }
    }
}
