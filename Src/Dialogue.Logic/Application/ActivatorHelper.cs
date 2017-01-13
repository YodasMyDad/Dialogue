namespace Dialogue.Logic.Application
{
    using CallingConventions = System.Reflection.CallingConventions;
    using System;
    using System.Linq;

    internal static class ActivatorHelper
    {
        /// <summary>
        /// Activates an instance of type T.
        /// </summary>
        /// <param name="constructorArgs">
        /// The constructor args.
        /// </param>
        /// <typeparam name="T">
        /// The type to activate
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws an exception if the activation was not successful
        /// </exception>
        public static T Instance<T>(object[] constructorArgs)
            where T : class
        {
            var type = typeof(T);

            const System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            var constructorArgumentTypes =
                constructorArgs.Select(value => value.GetType()).ToList();

            var constructor = type.GetConstructor(
                bindingFlags,
                null,
                CallingConventions.Any,
                constructorArgumentTypes.ToArray(),
                null);

            try
            {
                var obj = constructor.Invoke(constructorArgs);
                if (obj is T)
                {
                    return obj as T;
                }

                throw new Exception($"ServiceResolver failed to instantiate Service of type {type.Name}");
            }
            catch (Exception ex)
            {
                AppHelpers.LogError("Failed to instantiate service", ex);
                throw;
            }
        }

    }
}