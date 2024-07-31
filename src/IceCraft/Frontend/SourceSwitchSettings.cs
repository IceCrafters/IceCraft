namespace IceCraft.Frontend;

using Spectre.Console.Cli;

public class SourceSwitchSettings : CommandSettings
{
    [CommandArgument(0, "<SOURCE>")]
    public required string SourceName { get; set; }
}

public class SourceEnableSettings : SourceSwitchSettings
{
}

public class SourceDisableSettings : SourceSwitchSettings
{
}
