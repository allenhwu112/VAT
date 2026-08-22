namespace Vat.Migrations;

public enum MigrationCommand
{
    Up,
    Check,
    Down,
}

public sealed record RunnerArguments(MigrationCommand Command)
{
    public static RunnerArguments Parse(IReadOnlyList<string> args)
    {
        return args switch
        {
            { Count: 0 } => new(MigrationCommand.Up),
            { Count: 1 } when args[0].Equals("--check", StringComparison.OrdinalIgnoreCase) => new(MigrationCommand.Check),
            { Count: 1 } when args[0].Equals("--down", StringComparison.OrdinalIgnoreCase) => new(MigrationCommand.Down),
            _ => throw new ArgumentException(
                $"Unknown argument(s): {string.Join(' ', args)}. Use no argument, --check, or --down.",
                nameof(args)),
        };
    }
}
