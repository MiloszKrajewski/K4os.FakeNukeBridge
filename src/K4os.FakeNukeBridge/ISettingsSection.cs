using System.Collections.Generic;

namespace K4os.FakeNukeBridge;

/// <summary>Settings section.</summary>
public interface ISettingsSection
{
	/// <summary>Section name.</summary>
	string Name { get; }

	/// <summary>Keys in section.</summary>
	IReadOnlyList<string> Keys { get; }

	/// <summary>Return <c>true</c> if section has any keys.</summary>
	bool HasKeys { get; }

	/// <summary>List of all items in section.</summary>
	IEnumerable<KeyValuePair<string, string?>> Items { get; }

	/// <summary>
	/// Gets or sets value assigned to given key.
	/// If values does not exists it will return <c>null</c>
	/// </summary>
	/// <param name="key">Key name.</param>
	string? this[string key] { get; set; }
}
