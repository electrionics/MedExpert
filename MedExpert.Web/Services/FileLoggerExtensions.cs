using Microsoft.Extensions.Logging;
using System.IO;

namespace MedExpert.Web.Services
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder,
                                        string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            builder.AddProvider(new FileLoggerProvider(filePath));
            return builder;
        }
    }
}
