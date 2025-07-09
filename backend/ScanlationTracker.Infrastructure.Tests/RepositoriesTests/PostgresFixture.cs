using Testcontainers.PostgreSql;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace ScanlationTracker.Infrastructure.Tests.RepositoriesTests;

public class PostgresFixture(IMessageSink messageSink)
    : ContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(messageSink)
{
    protected override PostgreSqlBuilder Configure(PostgreSqlBuilder builder)
        => builder.WithImage("postgres:17.4").WithDatabase("ScanlationTrackerDb");
}
