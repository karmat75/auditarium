// SPDX-License-Identifier: MIT
namespace Auditarium.DemoData;

public sealed record DemoDataGateResult(bool IsAllowed, string Message);

public static class DemoDataInvocationGate
{
    public static DemoDataGateResult Evaluate(string[] args, string? environment, string? enabledValue)
    {
        if (!string.Equals(environment, "Development", StringComparison.Ordinal))
            return new(false, "DemoData can only run when the host environment is exactly Development.");
        if (!bool.TryParse(enabledValue, out var enabled) || !enabled)
            return new(false, "DemoData is disabled. Set Auditarium:DemoData:Enabled to true explicitly.");
        if (args.Length != 2 || !string.Equals(args[0], "apply", StringComparison.Ordinal) || !string.Equals(args[1], "--confirm", StringComparison.Ordinal))
            return new(false, "DemoData requires the explicit apply --confirm invocation.");
        return new(true, "DemoData invocation is permitted.");
    }
}
