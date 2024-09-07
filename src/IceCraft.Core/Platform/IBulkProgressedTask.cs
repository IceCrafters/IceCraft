namespace IceCraft.Core.Platform;

public interface IBulkProgressedTask
{
    /// <summary>
    /// Creates a new sub-task.
    /// </summary>
    /// <param name="description">The description of the task created.</param>
    /// <remarks>
    /// <para>
    /// Implementors should escape the <paramref name="description"/> argument value to forbid any markup it could
    /// insert.
    /// </para>
    /// </remarks>
    /// <returns>The progressed sub-task.</returns>
    IProgressedTask CreateTask(string description);
}
