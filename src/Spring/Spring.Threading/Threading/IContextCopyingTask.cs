namespace Spring.Threading
{
    /// <summary>
    /// A marker interface to indicate the task copies the context.
    /// </summary>
    public interface IContextCopyingTask
    {
        /// <summary>
        /// Gets and sets the <see cref="IContextCarrier"/> that captures
        /// and restores the context.
        /// </summary>
        IContextCarrier ContextCarrier { get; set; }
    }
}
