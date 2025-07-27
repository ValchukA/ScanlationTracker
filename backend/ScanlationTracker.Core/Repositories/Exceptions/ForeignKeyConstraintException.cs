namespace ScanlationTracker.Core.Repositories.Exceptions;

public class ForeignKeyConstraintException(Exception innerException)
    : Exception("Foreign key constraint violated", innerException);
