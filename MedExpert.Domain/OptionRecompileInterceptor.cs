using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Diagnostics;

using DbCommandInterceptor = Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor;

namespace MedExpert.Domain
{
    public class OptionRecompileInterceptor:DbCommandInterceptor
    {
        static void AddOptionToNonInsertCommand(DbCommand command)
        {
            string optionRecompileString = "\r\nOPTION (RECOMPILE)";
            if (!command.CommandText.Contains(optionRecompileString) && //Check the option is not added already
                !command.CommandText.Contains("INSERT INTO", StringComparison.OrdinalIgnoreCase) && // Check non insert command statement
                !(command.CommandText.Contains("UPDATE", StringComparison.OrdinalIgnoreCase) && command.CommandText.Contains("SET", StringComparison.OrdinalIgnoreCase))) // CHeck non-update command statement
            {
                //command.CommandText += optionRecompileString;
            }
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = new())
        {
            AddOptionToNonInsertCommand(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            AddOptionToNonInsertCommand(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = new())
        {
            AddOptionToNonInsertCommand(command);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            AddOptionToNonInsertCommand(command);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
            CancellationToken cancellationToken = new())
        {
            AddOptionToNonInsertCommand(command);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            AddOptionToNonInsertCommand(command);
            return base.ScalarExecuting(command, eventData, result);
        }
    }
}