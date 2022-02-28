using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace K4os.FakeNukeBridge;

/// <summary>
/// Simple helper allowing to parse .md file with changelog.
/// </summary>
public class ReleaseNotes
{
	/// <summary>Version.</summary>
	public Version Version { get; private set; } = new(0, 0, 0);
	
	/// <summary>Version tag.</summary>
	public string? Tag { get; private set; }

	/// <summary>Changes in latest release.</summary>
	public IReadOnlyList<string> Changes { get; private set; } = Array.Empty<string>();

	private static readonly Regex HeaderPattern = new(
		@"^##\s+(?<version>\d+(\.\d+(\.\d+)?)?)(\-(?<tag>[^\s]+))?.*$",
		RegexOptions.Singleline | RegexOptions.ExplicitCapture);

	private static readonly Regex BulletPattern = new(
		@"^(\*+\s+)?(?<entry>.*)$",
		RegexOptions.Singleline | RegexOptions.ExplicitCapture);

	/// <summary>Parses a changelog.</summary>
	/// <param name="lines">Text lines.</param>
	/// <returns>Parsed release notes.</returns>
	public static ReleaseNotes Parse(IEnumerable<string> lines)
	{
		var result = new ReleaseNotes();
		var changes = new List<string>();

		var phase = 0;

		foreach (var line in lines)
		{
			if (phase == 0)
			{
				if (!TryExtractVersion(line, result)) break;

				phase++;
			}
			else if (phase == 1)
			{
				if (TryAppendChange(line, changes)) continue;

				phase++;
			}
			else
			{
				break;
			}
		}

		result.Changes = changes;

		return result;
	}
		
	private static bool TryExtractVersion(string line, ReleaseNotes result)
	{
		if (string.IsNullOrEmpty(line)) return false;

		var m = HeaderPattern.Match(line);
		if (!m.Success) return false;

		result.Version = new Version(m.Groups["version"].Value);
		var tag = m.Groups["tag"].Value;
		result.Tag = string.IsNullOrWhiteSpace(tag) ? null : tag;

		return true;
	}

	private static bool TryAppendChange(string line, List<string> changes)
	{
		if (string.IsNullOrWhiteSpace(line)) return false; // empty line
		if (HeaderPattern.IsMatch(line)) return false; // next header

		var m = BulletPattern.Match(line);
		if (!m.Success) return false;

		changes.Add(m.Groups["entry"].Value);

		return true;
	}

	/// <summary>Parses changelog file content.</summary>
	/// <param name="content">File content.</param>
	/// <returns>Parsed release notes.</returns>
	public static ReleaseNotes Parse(string content) =>
		Parse(content.Split('\n').Select(l => l.Trim()).ToArray());

	/// <summary>Parses changelog file.</summary>
	/// <param name="filename">Change file name.</param>
	/// <returns>Parsed release notes.</returns>
	public static ReleaseNotes ParseFile(string filename) =>
		Parse(File.ReadAllLines(filename));
}