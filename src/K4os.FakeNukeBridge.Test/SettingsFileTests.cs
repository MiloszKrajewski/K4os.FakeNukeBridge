using Xunit;

namespace K4os.FakeNukeBridge.Test;

public class SettingsFileTests
{
	private const string Content = @"
		in_root = some text

		[section1]
		name1
		name2
		name3

		[section2]
		key1=value1=value1
		key2 = value2
		#comment

		[section3]
	";

	[Fact]
	public void SettingsCanBeLoaded()
	{
		var settings = SettingsFile.Parse(Content);
		Assert.NotNull(settings);
	}
	
	[Fact]
	public void RootSettingsCanBeRead()
	{
		var settings = SettingsFile.Parse(Content);
		Assert.Equal("some text", settings.Root["in_root"]);
		Assert.Equal(new[] { "in_root" }, settings.Root.Keys);
	}
	
	[Fact]
	public void AllSectionsAreRead()
	{
		var settings = SettingsFile.Parse(Content);
		Assert.True(settings.HasSection("section1"));
		Assert.True(settings.HasSection("section2"));
	}
	
	[Fact]
	public void EmptySectionsAreNotReported()
	{
		var settings = SettingsFile.Parse(Content);
		Assert.False(settings.HasSection("section3"));
		Assert.False(settings.HasSection("section4"));
	}

    [Fact]
    public void EmptySectionsAreEmpty()
	{
		var settings = SettingsFile.Parse(Content);
		Assert.True(settings["section1"].HasKeys);
		Assert.True(settings["section2"].HasKeys);
		Assert.False(settings["section3"].HasKeys);
		Assert.False(settings["section4"].HasKeys);
	}
    
    [Fact]
    public void ValuelessEntriesAreReportedAsKeys()
    {
	    var settings = SettingsFile.Parse(Content);
	    Assert.Equal(new[] { "name1", "name2", "name3" }, settings["section1"].Keys);
    }
   
    [Fact]
    public void WhitespaceIsStripped()
    {
	    var settings = SettingsFile.Parse(Content);
	    var section = settings["section2"];
	    Assert.Equal("value2", section["key2"]);
    }
    
    [Fact]
    public void CanUseEqualsInValue()
    {
	    var settings = SettingsFile.Parse(Content);
	    var section = settings["section2"];
	    Assert.Equal("value1=value1", section["key1"]);
    }
    
    [Fact]
    public void CommandLinesAreStripped()
    {
	    var settings = SettingsFile.Parse(Content);
	    var section = settings["section2"];
	    Assert.Equal(new[] { "key1", "key2" }, section.Keys);
    }
}
