using System.Collections.Generic;
using System.Linq;

namespace K4os.FakeNukeBridge;

/// <summary>Settings section.</summary>
internal class SettingsSection: ISettingsSection
{
	/// <summary>Section name.</summary>
	public string Name { get; }

	private readonly Dictionary<string, string?> _items = new();
	private readonly List<string> _keys = new();

	public SettingsSection(string name) { Name = name; }

	/// <summary>
	/// Gets or sets value assigned to given key.
	/// If values does not exists it will return <c>null</c>
	/// </summary>
	/// <param name="key">Key name.</param>
	public string? this[string key] { get => TryGet(key); set => TrySet(key, value); }

	/// <summary>Keys in section.</summary>
	public IReadOnlyList<string> Keys => _keys;

	/// <summary>Return <c>true</c> if section has any keys.</summary>
	public bool HasKeys => _keys.Count > 0;

	/// <summary>List of all items in section.</summary>
	public IEnumerable<KeyValuePair<string, string?>> Items =>
		_keys.Select(k => new KeyValuePair<string, string?>(k, _items[k]));

	private string? TryGet(string key) =>
		_items.TryGetValue(key, out var value) ? value : null;

	private void TrySet(string key, string? value)
	{
		if (!_items.ContainsKey(key))
		{
			_keys.Add(key);
		}

		_items[key] = value;
	}
}
