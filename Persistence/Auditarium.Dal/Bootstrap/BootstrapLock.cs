// SPDX-License-Identifier: MIT
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Dal.Bootstrap;

/// <summary>Holds the installation-wide bootstrap lock on its database session.</summary>
internal sealed class BootstrapLock(DbConnection connection, string provider) : IAsyncDisposable
{
    private const string Resource = "Auditarium.DatabaseBootstrap";

    public static async Task<BootstrapLock> AcquireAsync(AuditariumDbContext db, string provider, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        try
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)) await AcquirePostgreSqlAsync(connection, timeout, cancellationToken);
            else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)) await AcquireSqlServerAsync(connection, timeout, cancellationToken);
            else throw new InvalidOperationException("Auditarium:Database:Provider must be PostgreSQL or SqlServer.");
            return new BootstrapLock(connection, provider);
        }
        catch (Exception exception)
        {
            await connection.CloseAsync();
            if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException("Waiting for the Auditarium bootstrap lock was cancelled.", exception, cancellationToken);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)) await ExecuteAsync(connection, "EXEC sp_releaseapplock @Resource = @resource, @LockOwner = 'Session';", CancellationToken.None);
            else await ExecuteAsync(connection, "SELECT pg_advisory_unlock(hashtext(@resource));", CancellationToken.None);
        }
        finally { await connection.CloseAsync(); }
    }

    private static async Task AcquirePostgreSqlAsync(DbConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!await ExecuteScalarAsync<bool>(connection, "SELECT pg_try_advisory_lock(hashtext(@resource));", cancellationToken))
        {
            if (DateTime.UtcNow >= deadline) throw new TimeoutException("Timed out waiting for the Auditarium bootstrap lock.");
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }
    }

    private static async Task AcquireSqlServerAsync(DbConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "DECLARE @result int; EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = @timeout; SELECT @result;";
        AddParameter(command, "@resource", Resource);
        AddParameter(command, "@timeout", checked((int)timeout.TotalMilliseconds));
        if (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) < 0) throw new TimeoutException("Timed out waiting for the Auditarium bootstrap lock.");
    }

    private static async Task ExecuteAsync(DbConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@resource", Resource);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<T> ExecuteScalarAsync<T>(DbConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@resource", Resource);
        return (T)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
