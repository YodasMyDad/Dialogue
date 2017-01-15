namespace Dialogue.Logic
{
    using System;
    using System.Linq;
    using Application;
    using Interfaces;
    using Umbraco.Core;

    /// <summary>
    /// A resolver to ensure services are instantiated only once per request.
    /// </summary>
    internal class ServiceResolver
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static ServiceResolver _current;

        /// <summary>
        /// The _cache.
        /// </summary>
        private readonly CacheHelper _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResolver"/> class.
        /// </summary>
        /// <param name="cache">
        /// The <see cref="CacheHelper"/>.
        /// </param>
        /// <remarks>
        /// Used for testing
        /// </remarks>
        internal ServiceResolver(CacheHelper cache)
        {
            Mandate.ParameterNotNull(cache, "cache");

            this._cache = cache;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ServiceResolver"/> class from being created.
        /// </summary>
        private ServiceResolver()
            : this(ApplicationContext.Current.ApplicationCache)
        {
        }

        /// <summary>
        /// Gets or sets the current singleton instance.
        /// </summary>
        public static ServiceResolver Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new ServiceResolver();
                }

                return _current;
            }

            internal set
            {
                _current = value;
            }
        }

        /// <summary>
        /// Creates an instance of a service.
        /// </summary>
        /// <typeparam name="TService">
        /// The type of the service to be returned
        /// </typeparam>
        /// <returns>
        /// The <see cref="TService"/>.
        /// </returns>
        public TService Instance<TService>()
            where TService : IRequestCachedService, new()
        {
            return
                (TService)
                this._cache.RequestCache.GetCacheItem(ServiceCacheKey(
                        typeof(TService)),
                    () =>
                    {
                        try
                        {
                            var service = (TService)Activator.CreateInstance(typeof(TService));
                            return service;
                        }
                        catch (Exception ex)
                        {
                            AppHelpers.LogError("Failed to instantiate service", ex);
                            throw;
                        }
                    });
        }

        /// <summary>
        /// Creates an instance of a service with arguments
        /// </summary>
        /// <param name="constructorArgumentValues">
        /// The constructor argument values.
        /// </param>
        /// <param name="cacheSuffix">
        /// Optional additional caching suffix
        /// </param>
        /// <typeparam name="TService">
        /// The type of the service to resolve
        /// </typeparam>
        /// <returns>
        /// The <see cref="TService"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws an exception if service cannot be resolved
        /// </exception>
        public TService Instance<TService>(object[] constructorArgumentValues, string cacheSuffix = "")
            where TService : class, IRequestCachedService
        {
            //// We may need to manage separate instances of these services if the constructor arguments are different
            //// so that we can assert the values returned are what we expect.
            var suffix = string.Join(".", constructorArgumentValues.Select(x => x.ToString()));
            var cacheKey = $"{ServiceCacheKey(typeof(TService))}{suffix}{cacheSuffix}".GetHashCode().ToString();

            return (TService)this._cache.RequestCache.GetCacheItem(
                cacheKey,
                () =>
                ActivatorHelper.Instance<TService>(constructorArgumentValues));
        }

        /// <summary>
        ///  Creates the cache key
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public string ServiceCacheKey(Type service)
        {
            return $"dialogue.serviceresolver.{service.Name}";
        }
    }
}