using System;
using System.IO;

namespace K4os.FakeNukeBridge;

/// <summary>
/// Helper methods to find files inside repository.
/// </summary>
public class PathFinder
{
	/// <summary>
	/// Find folder which passes given test walking up the tree.
	/// Can be used to find the root folder of a project or settings file in repository.
	/// </summary>
	/// <param name="root">Starting folder, usually set to "."</param>
	/// <param name="test">Test callback.</param>
	/// <returns>Matching folder, or <c>null</c></returns>
	public string? TryFind(string root, Func<string, bool> test)
	{
		bool IsRoot(string p) =>
			Path.GetFullPath(p) == Path.GetFullPath(Path.Combine(p, ".."));

		var path = root;
		var counter = 1024;

		while (true)
		{
			if (test(path)) return path;
			if (IsRoot(path) || counter-- <= 0) return null;

			path = Path.Combine(path, "..");
		}
	}

	/// <summary>
	/// Find file in any parent folder.
	/// </summary>
	/// <param name="root">Starting folder, </param>
	/// <param name="fileName">File to find.</param>
	/// <returns>Path to file.</returns>
	public string? TryFindFile(string root, string fileName) =>
		TryFind(root, p => File.Exists(Path.Combine(p, fileName))) switch {
			null => null, var p => Path.Combine(p, fileName)
		};

	/// <summary>
	/// Find folder in any parent folder.
	/// </summary>
	/// <param name="root">Starting folder, </param>
	/// <param name="directoryName">Directory to find.</param>
	/// <returns>Path to file.</returns>
	public string? TryFindDirectory(string root, string directoryName) =>
		TryFind(root, p => Directory.Exists(Path.Combine(p, directoryName))) switch {
			null => null, var p => Path.Combine(p, directoryName)
		};
}
