// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace FodyTools
{
    using System.Diagnostics.CodeAnalysis;

    using Fody;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// A generic logger interface to decouple implementation.
    /// </summary>
    internal interface ILogger
    {
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogDebug(string message);
        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogInfo(string message);
        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sequencePoint">The optional sequence point where the problem occurred.</param>
        void LogWarning(string message, SequencePoint? sequencePoint = null);
        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sequencePoint">The optional sequence point where the error occurred.</param>
        void LogError(string message, SequencePoint? sequencePoint = null);
        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="method">The method where the problem occurred.</param>
        void LogWarning(string message, MethodReference? method);
        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="method">The method where the error occurred.</param>
        void LogError(string message, MethodReference? method);
    }

    /// <summary>
    /// A generic type system interface to decouple implementation.
    /// </summary>
    internal interface ITypeSystem
    {
        /// <summary>
        /// Finds the type in the assemblies that the weaver has registered for scanning.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>The type definition</returns>
        TypeDefinition FindType(string typeName);

        /// <summary>
        /// Finds the type in the assemblies that the weaver has registered for scanning.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="value">The return value.</param>
        /// <returns><c>true</c> if the type was found and value contains a valid item.</returns>
        bool TryFindType(string typeName, [NotNullWhen(true)] out TypeDefinition? value);

        /// <summary>
        /// Gets the fody basic type system.
        /// </summary>
        Fody.TypeSystem TypeSystem { get; }

        /// <summary>
        /// Gets the module definition of the target module.
        /// </summary>
        ModuleDefinition ModuleDefinition { get; }
    }

    /// <summary>
    /// A <see cref="Fody.BaseModuleWeaver" /> implementing <see cref="ILogger"/> and <see cref="ITypeSystem"/>, decorated with <see cref="NotNullAttribute"/>.
    /// </summary>
    /// <seealso cref="Fody.BaseModuleWeaver" />
    /// <seealso cref="FodyTools.ILogger" />
    /// <seealso cref="FodyTools.ITypeSystem" />
    [ExcludeFromCodeCoverage]
    public abstract class AbstractModuleWeaver : BaseModuleWeaver, ILogger, ITypeSystem
    {
        void ILogger.LogDebug(string message)
        {
            WriteMessage(message, MessageImportance.Low);
        }

        void ILogger.LogInfo(string message)
        {
            WriteMessage(message, MessageImportance.Normal);
        }

        void ILogger.LogWarning(string message, SequencePoint? sequencePoint)
        {
            WriteWarning(message, sequencePoint);
        }

        void ILogger.LogError(string message, SequencePoint? sequencePoint)
        {
            WriteError(message, sequencePoint);
        }

        void ILogger.LogWarning(string message, MethodReference? method)
        {
            ((ILogger)this).LogWarning(message, method.GetEntryPoint());
        }

        void ILogger.LogError(string message, MethodReference? method)
        {
            ((ILogger)this).LogError(message, method.GetEntryPoint());
        }

        TypeDefinition ITypeSystem.FindType(string typeName)
        {
            return FindTypeDefinition(typeName);
        }

        bool ITypeSystem.TryFindType(string typeName, [NotNullWhen(true)] out TypeDefinition? value)
        {
            return TryFindTypeDefinition(typeName, out value);
        }
    }
}
