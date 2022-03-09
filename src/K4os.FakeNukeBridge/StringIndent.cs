using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace K4os.FakeNukeBridge;

/// <summary>
/// Class for manage string indentation.
/// </summary>
public class StringIndent
{
	#region method IndentBlock

	/// <summary>
	/// Indents the block of text.
	/// </summary>
	/// <param name="indent">The indent.</param>
	/// <param name="block">The block.</param>
	/// <returns>Indented block.</returns>
	public static string IndentBlock(string indent, string block)
	{
		if (string.IsNullOrEmpty(indent))
			return block;

		var result = new StringBuilder();
		foreach (var line in ExtractLines(block))
		{
			if (IsEmptyLine(line))
			{
				// it is the last line (does not end with \n) and it is empty
				// do not add \n to result
				if (line.EndsWith("\n"))
					result.AppendLine();
			}
			else
			{
				result.Append(indent).Append(line);
			}
		}

		return result.ToString();
	}

	/// <summary>
	/// Reindents the block. Effectively, it deindents the block completely then indents it back.
	/// </summary>
	/// <param name="indent">The indent.</param>
	/// <param name="block">The block.</param>
	/// <param name="raiseError">if set to <c>true</c> error is raised when deindentation fails.</param>
	/// <returns></returns>
	public static string ReindentBlock(string indent, string block, bool raiseError) =>
		IndentBlock(indent, DeindentBlock(FindIndent(block), block, raiseError));

	#endregion

	#region tabs to spaces

	/// <summary>
	/// Converts tabs to spaces in a single line.
	/// </summary>
	/// <param name="line">The line.</param>
	/// <param name="tabSize">Size of the tab.</param>
	/// <returns>Same string with tabs converted to spaces.</returns>
	public static string TabsToSpacesLine(string line, int tabSize)
	{
		var result = new StringBuilder();
		var length = line.Length;
		var column = 0;

		for (var i = 0; i < length; i++)
		{
			var c = line[i];

			if (c != '\t')
			{
				result.Append(c);
				column++;
			}
			else
			{
				var target = (column + tabSize) / tabSize * tabSize;
				result.Append(' ', target - column);
				column = target;
			}
		}

		return result.ToString();
	}

	/// <summary>
	/// Converts tabs to spaces in a block of text.
	/// </summary>
	/// <param name="block">The block of text.</param>
	/// <param name="tabSize">Size of the tab.</param>
	/// <returns>Same string with tabs converted to spaces.</returns>
	public static string TabsToSpaces(string block, int tabSize)
	{
		var result = new StringBuilder();
		foreach (var line in ExtractLines(block))
		{
			result.Append(TabsToSpacesLine(line, tabSize));
		}

		return result.ToString();
	}

	#endregion

	#region method DeindentBlock

	/// <summary>
	/// Deindents the block by a given indent.
	/// </summary>
	/// <param name="indent">The indent.</param>
	/// <param name="block">The block.</param>
	/// <param name="raiseError">if set to <c>true</c> raises error if deindent cannot be applied.</param>
	/// <returns>Deindented text block.</returns>
	public static string DeindentBlock(string indent, string block, bool raiseError)
	{
		var result = new StringBuilder();
		foreach (var line in ExtractLines(block))
		{
			result.Append(DeindentLine(indent, line, raiseError));
		}

		return result.ToString();
	}

	/// <summary>
	/// Deindents the block.
	/// </summary>
	/// <param name="block">The block.</param>
	/// <returns>Deindented text block.</returns>
	public static string DeindentBlock(string block) =>
		DeindentBlock(FindIndent(block), block, false);

	#endregion

	#region method FindIndent

	private static readonly Regex IndentRx = new(@"^\s*", RegexOptions.Compiled);

	/// <summary>
	/// Finds the indent in given block of text.
	/// </summary>
	/// <param name="block">The block.</param>
	/// <returns>Indentation of text.</returns>
	public static string FindIndent(string block)
	{
		string? result = null;

		foreach (var line in ExtractLines(block))
		{
			if (IsEmptyLine(line)) continue;

			var m = IndentRx.Match(line);
			if (!m.Success) continue;

			var found = m.Value;
			if (result == null || found.Length < result.Length)
			{
				result = found;
			}
		}

		return result ?? "";
	}

	#endregion

	#region method DeindentLine

	/// <summary>
	/// Deindents the line.
	/// </summary>
	/// <param name="indent">The indent.</param>
	/// <param name="line">The line.</param>
	/// <param name="raiseError">if set to <c>true</c> [raise error].</param>
	/// <returns></returns>
	private static string DeindentLine(string indent, string line, bool raiseError)
	{
		if (line.StartsWith(indent))
			return line.Remove(0, indent.Length);

		if (raiseError && !IsEmptyLine(line))
			throw new ArgumentException("Given line is not properly indented");

		return line;
	}

	#endregion

	#region method ExtractLine & ExtractLines

	private static readonly Regex RawLineRx = new Regex(
		@"((.*?)((\r\n)|(\n)))|((.+?)$)",
		RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

	/// <summary>
	/// Extracts single line starting at given index.
	/// </summary>
	/// <param name="block">The block of text.</param>
	/// <param name="startIndex">The start index.</param>
	/// <returns>Text up to EoL or EoF</returns>
	public static string? ExtractLine(string block, int startIndex) =>
		RawLineRx.Match(block, startIndex) switch { var m when m.Success => m.Value, _ => null, };

	/// <summary>
	/// Enumerates lines in given block of text.
	/// </summary>
	/// <param name="block">The block of text.</param>
	/// <returns>Collection of text lines.</returns>
	public static IEnumerable<string> ExtractLines(string block)
	{
		var index = 0;

		while (true)
		{
			var line = ExtractLine(block, index);
			if (line == null) break;

			index += line.Length;
			yield return line;
		}
	}

	#endregion

	#region method EndsWithNewLine

	private static readonly Regex StartsWithNewLineRx = new Regex(
		@"^[ \t]*\r?\n",
		RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

	private static readonly Regex EndsWithNewLineRx = new Regex(
		@"\r?\n[ \t]*$",
		RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

	/// <summary>
	/// Checks if line starts with new line characters (<c>\n</c> or <c>\r\n</c>).
	/// </summary>
	/// <param name="line">The line.</param>
	/// <param name="trimSpaces">if set to <c>true</c> spaces are trimmed.</param>
	/// <returns><c>true</c> is line starts with new line character.</returns>
	public static bool StartsWithNewLine(string line, bool trimSpaces)
	{
		return trimSpaces
			? StartsWithNewLineRx.Match(line).Success
			: line.StartsWith("\r\n") || line.StartsWith("\n");
	}

	/// <summary>
	/// Checks if line ends with new line characters (<code>\n</code> or <code>\r\n</code>).
	/// </summary>
	/// <param name="line">The line.</param>
	/// <param name="trimSpaces">if set to <c>true</c> spaces are trimmed.</param>
	/// <returns><c>true</c> is line ends with new line character.</returns>
	public static bool EndsWithNewLine(string line, bool trimSpaces)
	{
		return trimSpaces
			? EndsWithNewLineRx.Match(line).Success
			: line.EndsWith("\n");
	}

	#endregion

	#region method IsEmptyLine & IsWhiteLine

	private static readonly Regex EmptyLineRx = new Regex(
		@"^\s*$",
		RegexOptions.Compiled | RegexOptions.Singleline);

	/// <summary>
	/// Determines whether given line is empty line. Note: Whitespaces are considered as empty.
	/// </summary>
	/// <param name="line">The line.</param>
	/// <returns><c>true</c> if given line is empty line; otherwise, <c>false</c>.</returns>
	public static bool IsEmptyLine(string line) =>
		string.IsNullOrEmpty(line) || EmptyLineRx.Match(line).Success;

	#endregion

	#region method ForceNewLine

	/// <summary>
	/// Forces the new line on the end. It is not added if it is already there.
	/// </summary>
	/// <param name="text">The text.</param>
	/// <returns>Given text with new line on the end.</returns>
	public static string ForceNewLine(string text) =>
		ForceNewLine(text, false, true, Environment.NewLine);

	/// <summary>
	/// Forces the new line on the end. It is not added if it is already there.
	/// </summary>
	/// <param name="text">The text.</param>
	/// <param name="newLine">The new line combination.</param>
	/// <returns>Given text with new line on the end.</returns>
	public static string ForceNewLine(string text, string newLine) =>
		ForceNewLine(text, false, true, newLine);

	/// <summary>
	/// Forces the new line.
	/// </summary>
	/// <param name="text">The text.</param>
	/// <param name="head">if set to <c>true</c> new line will be enforced before first line.</param>
	/// <param name="tail">if set to <c>true</c> new line will be enforced after last line.</param>
	/// <returns>Text with new line added.</returns>
	public static string ForceNewLine(string text, bool head, bool tail) =>
		ForceNewLine(text, head, tail, Environment.NewLine);

	/// <summary>
	/// Forces the new line.
	/// </summary>
	/// <param name="text">The text.</param>
	/// <param name="head">if set to <c>true</c> new line will be enforced before first line.</param>
	/// <param name="tail">if set to <c>true</c> new line will be enforced after last line.</param>
	/// <param name="newLine">The new line.</param>
	/// <returns>Text with new line added.</returns>
	private static string ForceNewLine(string text, bool head, bool tail, string newLine)
	{
		if (string.IsNullOrEmpty(text))
			return head || tail ? newLine : text;

		head = head && !StartsWithNewLine(text, true);
		tail = tail && !EndsWithNewLine(text, true);

		return string.Format(
			"{0}{2}{1}",
			head ? newLine : string.Empty,
			tail ? newLine : string.Empty,
			text);
	}

	#endregion

	#region method MakeIndent

	/// <summary>
	/// Makes the indent.
	/// </summary>
	/// <param name="length">The length.</param>
	/// <param name="indentChar">The indent char.</param>
	/// <returns><paramref name="indentChar"/> repeated <paramref name="length"/> times.</returns>
	public static string MakeIndent(int length, char indentChar) =>
		new string(indentChar, length);

	#endregion

	#region method NormalizeNewLine

	private static readonly Regex TrimmedLineRx = new Regex(
		@"^(?<head>([ \t]*\r?\n)+)?(?<body>.*?)[ \t]*(?<tail>(\r?\n[ \t]*)+)?$",
		RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

	private static string ConditionalNewLine(bool? force, Group group) =>
		force.GetValueOrDefault(group.Success) ? "\n" : string.Empty;

	/// <summary>
	/// Normalizes line by adding new line at the beginning and/or at the end.
	/// </summary>
	/// <param name="text">The text.</param>
	/// <param name="head">Set to <c>true</c> if you need new line, set to <c>false</c> if you don't, set to <c>null</c> if you don't care (leave as is).</param>
	/// <param name="tail">Set to <c>true</c> if you need new line, set to <c>false</c> if you don't, set to <c>null</c> if you don't care (leave as is).</param>
	/// <returns>Line with normalized new lines.</returns>
	public static string NormalizeNewLine(string text, bool? head, bool? tail)
	{
		var m = TrimmedLineRx.Match(text);
		if (!m.Success)
			throw new ArgumentException($"Text '{text}' cannot be trimmed.");

		var headGroups = m.Groups["head"];
		var bodyGroups = m.Groups["body"];
		var tailGroups = m.Groups["tail"];

		// ReSharper disable once UseStringInterpolation
		return string.Format(
			"{0}{1}{2}",
			ConditionalNewLine(head, headGroups),
			bodyGroups.Value,
			ConditionalNewLine(tail, tailGroups));
	}

	#endregion

	#region NormalizeText

	/// <summary>
	/// Normalizes the text. Removes \n from the beginning and end of text 
	/// block (kind of trim) and deindents as much as possible.
	/// </summary>
	/// <param name="text">The text.</param>
	/// <returns>Trimmed and unindented text.</returns>
	public static string NormalizeText(string text) =>
		DeindentBlock(NormalizeNewLine(text, false, false));

	#endregion

	#region ScanForLineStart & ScanForLineEnd

	/// <summary>
	/// Scans text for line start. It scans backwards starting at given position.
	/// </summary>
	/// <param name="input">The input.</param>
	/// <param name="index">The starting index.</param>
	/// <returns>First character in a line.</returns>
	public static int ScanForLineStart(string input, int index)
	{
		while (true)
		{
			var c = index < 0 ? '\n' : input[index];

			if (c == '\n')
			{
				index++;
				break;
			}

			index--;
		}

		return index;
	}

	/// <summary>
	/// Scans text for line end. Scans forward from given position.
	/// </summary>
	/// <param name="input">The input.</param>
	/// <param name="index">The index.</param>
	/// <returns></returns>
	public static int ScanForLineEnd(string input, int index)
	{
		var inputLength = input.Length;

		while (true)
		{
			var c = index >= inputLength ? '\n' : input[index];

			if (c == '\n')
			{
				index--;
				break;
			}

			index++;
		}

		return index;
	}

	#endregion
}
