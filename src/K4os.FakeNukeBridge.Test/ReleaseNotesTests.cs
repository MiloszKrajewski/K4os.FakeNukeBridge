using System;
using System.Linq;
using Xunit;

namespace K4os.FakeNukeBridge.Test
{
	public class ReleaseNotesTests
	{
		[Fact]
		public void ParserHandlesVersionOnly()
		{
			var notes = ReleaseNotes.Parse(
				new[] {
					"## 1.2.3-beta.7",
					"## 1.2.2",
				});
			Assert.NotNull(notes);
			Assert.Equal(new Version(1, 2, 3), notes.Version);
			Assert.Equal("beta.7", notes.Tag);
			Assert.Equal(0, notes.Changes.Count);
		}

		[Fact]
		public void ParserHandlesVersionOnlyWithEmptyLine()
		{
			var notes = ReleaseNotes.Parse(
				new[] {
					"## 1.2.3-beta.7",
					"",
					"## 1.2.2",
				});
			Assert.NotNull(notes);
			Assert.Equal(new Version(1, 2, 3), notes.Version);
			Assert.Equal("beta.7", notes.Tag);
			Assert.Equal(0, notes.Changes.Count);
		}

		[Fact]
		public void ParserHandlesBulletsWithNotEmptyLineBetween()
		{
			var notes = ReleaseNotes.Parse(
				new[] {
					"## 1.2.3-beta.7",
					"* hello me friend",
					"## 1.2.2",
				});
			Assert.NotNull(notes);
			Assert.Equal(new Version(1, 2, 3), notes.Version);
			Assert.Equal("beta.7", notes.Tag);
			Assert.Equal(1, notes.Changes.Count);
		}

		[Fact]
		public void ParserHandlesBulletsWithEmptyLineBetween()
		{
			var notes = ReleaseNotes.Parse(
				new[] {
					"## 1.2.3-beta.7",
					"* hello me friend",
					"",
					"## 1.2.2",
				});
			Assert.NotNull(notes);
			Assert.Equal(new Version(1, 2, 3), notes.Version);
			Assert.Equal("beta.7", notes.Tag);
			Assert.Equal(1, notes.Changes.Count);
		}

		[Fact]
		public void ParserHandlesDatesAddedToVersionLine()
		{
			var notes = ReleaseNotes.Parse("## 1.2.3-beta.7 (2022/02/22)");
			Assert.NotNull(notes);
			Assert.Equal(new Version(1, 2, 3), notes.Version);
			Assert.Equal("beta.7", notes.Tag);
		}

		[Fact]
		public void TagIsNotNeeded()
		{
			var notes = ReleaseNotes.Parse("## 1.2.3");
			Assert.NotNull(notes);
			Assert.Equal(new Version(1, 2, 3), notes.Version);
			Assert.Null(notes.Tag);
		}
	}
}
