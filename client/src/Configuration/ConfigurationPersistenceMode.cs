namespace ModuloFivePadClient.Configuration
{
    public enum ConfigurationPersistenceMode
    {
        /// <summary>
        /// Write no more frequently than <see cref="IModuloFivePadConfigurationProvider.WriteBufferRateLimit"/>
        /// </summary>
        Buffered,

        /// <summary>
        /// Write immediately and cancel any buffered writes.
        /// </summary>
        Immediate,

        // no use case for this
        // /// <summary>
        // /// Update the in-memory but do not write to file.
        // /// </summary>
        // DoNotWrite,
    }
}