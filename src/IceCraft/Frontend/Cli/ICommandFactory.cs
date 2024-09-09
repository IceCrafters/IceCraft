namespace IceCraft.Frontend.Cli;

using System.CommandLine;

public interface ICommandFactory
{
    Command CreateCommand();
}