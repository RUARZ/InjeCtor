namespace InjeCtor.Core.Builder
{
    /// <summary>
    /// Interface for providing a method to build / get a <see cref="IInjeCtor"/> instance.
    /// </summary>
    public interface IInjeCtorBuilder
    {
        /// <summary>
        /// Set's up and gets a implementation for <see cref="IInjeCtor"/>.
        /// </summary>
        /// <returns>The created <see cref="IInjeCtor"/> implementation.</returns>
        IInjeCtor Build();
    }
}
