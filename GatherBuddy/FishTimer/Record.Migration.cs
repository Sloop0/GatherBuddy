using System.Text.RegularExpressions;

namespace GatherBuddy.FishTimer;

public partial class Record
{
    private static readonly Regex V3MigrationRegex = new("(Unknown|Weak|Strong|Legendary) ", RegexOptions.Compiled);

    private static string MigrateToV3(string line)
        => V3MigrationRegex.Replace(line, "");
}
