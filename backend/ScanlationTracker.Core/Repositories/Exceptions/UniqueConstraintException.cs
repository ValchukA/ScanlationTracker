namespace ScanlationTracker.Core.Repositories.Exceptions;

public class UniqueConstraintException(Exception innerException)
    : Exception("Unique constraint violated", innerException);
