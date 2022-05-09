using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace K4os.FakeNukeBridge;

/// <summary>Handler for settings file.</summary>
public class SettingsFile
{
	private static readonly Regex HeaderPattern =
		new(@"^\s*\[\s*(?<name>.*?)\s*\]\s*$");

	private static readonly Regex ValuePattern =
		new(@"^\s*(?<key>.*?)\s*(=\s*(?<value>.*?)\s*)?$");

	private static readonly Regex EmptyPattern =
		new(@"^\s*([#;].*)?$");

	private readonly IDictionary<string, SettingsSection> _sections =
		new Dictionary<string, SettingsSection>();

	private readonly List<string> _keys = new();

	/// <summary>
	/// Get a section. Creates and returns new section if it does not exist.
	/// </summary>
	/// <param name="key">Section name.</param>
	public ISettingsSection this[string key] => TryGetOrCreate(key);

	/// <summary>Checks is given section exists and has some keys in it.</summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public bool HasSection(string key) => TryGetOrNull(key)?.HasKeys ?? false;
	
	private SettingsSection? TryGetOrNull(string key) => 
		_sections.TryGetValue(key, out var section) ? section : null;

	private SettingsSection TryGetOrCreate(string key)
	{
		if (_sections.TryGetValue(key, out var section))
			return section;

		_sections[key] = section = new SettingsSection(key);
		_keys.Add(key);

		return section;
	}

	/// <summary>Enumerates all section which already have keys.
	/// NOTE: Empty sections are ignored.</summary>
	public IEnumerable<ISettingsSection> Sections =>
		_keys.Select(k => _sections[k]).Where(s => s.HasKeys);

	/// <summary>Returns root section (which has <c>string.Empty</c> name).</summary>
	public ISettingsSection Root => TryGetOrCreate("");

	/// <summary>Parses settings from provided text lines.</summary>
	/// <param name="lines">Lines of text.</param>
	/// <returns>Parsed settings.</returns>
	/// <exception cref="ArgumentException">Thrown when settings cannot be parsed.</exception>
	public static SettingsFile Parse(IEnumerable<string> lines)
	{
		var result = new SettingsFile();
		var section = result.Root;

		foreach (var line in lines)
		{
			if (EmptyPattern.IsMatch(line))
				continue;

			var hm = HeaderPattern.Match(line);
			if (hm.Success)
			{
				section = result.TryGetOrCreate(hm.Groups["name"].Value);
				continue;
			}

			var vm = ValuePattern.Match(line);
			if (vm.Success)
			{
				var key = vm.Groups["key"].Value;
				var value = vm.Groups["value"] switch {
					{ Success: true, Value: var v } => v, _ => null,
				};
				section[key] = value;
				continue;
			}

			throw new ArgumentException($"Settings file is invalid: {line}");
		}

		return result;
	}

	/// <summary>Parses settings from provided text lines.</summary>
	/// <param name="content"></param>
	/// <returns>Parsed settings.</returns>
	/// <exception cref="ArgumentException">Thrown when settings cannot be parsed.</exception>
	public static SettingsFile Parse(string content) =>
		Parse(content.Split('\n'));

	/// <summary>Parses settings from provided file.</summary>
	/// <param name="filename">File name.</param>
	/// <returns>Parsed settings.</returns>
	/// <exception cref="ArgumentException">Thrown when settings cannot be parsed.</exception>
	public static SettingsFile ParseFile(string filename) => 
		TryParseFile(filename) ?? 
		throw new FileNotFoundException($"File {filename} does not exist");

	/// <summary>Parses settings from provided file.</summary>
	/// <param name="filename">File name.</param>
	/// <returns>Parsed settings.</returns>
	/// <exception cref="ArgumentException">Thrown when settings cannot be parsed.</exception>
	public static SettingsFile? TryParseFile(string filename) => 
		!File.Exists(filename) ? null : Parse(File.ReadAllLines(filename));
}
