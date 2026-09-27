using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Nibras.BuildingBlocks.Persistence;

/// <summary>
/// Sets <c>app.tenant_id</c> as the first statement of every transaction a tenant-bound context opens, including
/// the one EF Core opens around <c>SaveChanges</c>. <c>set_config(..., true)</c> is <c>SET LOCAL</c> in function form,
/// so the value ends with the transaction and never leaks to the next client of a pooled server connection under
/// PgBouncer transaction mode (REQ-DATA-004). A context without a tenant sets nothing, and the row-level
/// security policy then refuses every tenant-owned row.
/// </summary>
public sealed class TenantTransactionInterceptor : DbTransactionInterceptor
{
    public override DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(eventData);
        if (eventData.Context is NibrasDbContext { HasTenant: true } db)
        {
            using var command = CreateCommand(connection, result, db.CurrentTenantId);
            command.ExecuteNonQuery();
        }

        return result;
    }

    public override async ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(eventData);
        if (eventData.Context is NibrasDbContext { HasTenant: true } db)
        {
            var command = CreateCommand(connection, result, db.CurrentTenantId);
            await using (command.ConfigureAwait(false))
            {
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        return result;
    }

    private static DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, Guid tenant)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT set_config('{RowLevelSecurity.TenantSetting}', @tenant, true)";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "tenant";
        parameter.Value = tenant.ToString("D");
        command.Parameters.Add(parameter);
        return command;
    }
}

public static class TenantTransactions
{
    /// <summary>
    /// Runs work inside a transaction that carries the tenant for row-level security. Read handlers use it too:
    /// a query outside a transaction has no <c>app.tenant_id</c> and PostgreSQL refuses it (document 10, part 2.4).
    /// </summary>
    public static async Task<T> InTenantTransactionAsync<T>(this NibrasDbContext context, Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(work);
        if (!context.HasTenant)
        {
            throw new TenantNotSetException();
        }

        var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await using (transaction.ConfigureAwait(false))
        {
            var result = await work(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
