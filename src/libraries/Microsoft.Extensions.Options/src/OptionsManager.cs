// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.Options
{
    /// <summary>
    /// Implementation of <see cref="IOptions{TOptions}"/> and <see cref="IOptionsSnapshot{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type.</typeparam>
    public class OptionsManager<[DynamicallyAccessedMembers(Options.DynamicallyAccessedMembers)] TOptions> :
        IOptions<TOptions>,
        IOptionsSnapshot<TOptions>
        where TOptions : class
    {
        private readonly IOptionsFactory<TOptions> _factory;
        private readonly IOptionsMonitorCache<TOptions> _cache = new OptionsCache<TOptions>(); // Note: this is a private cache

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="factory">The factory to use to create options.</param>
        public OptionsManager(IOptionsFactory<TOptions> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// The default configured <typeparamref name="TOptions"/> instance, equivalent to Get(Options.DefaultName).
        /// </summary>
        public TOptions Value => Get(Options.DefaultName);

        /// <summary>
        /// Returns a configured <typeparamref name="TOptions"/> instance with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the <typeparamref name="TOptions"/> instance, if <see langword="null"/> <see cref="Options.DefaultName"/> is used.</param>
        /// <returns>The <typeparamref name="TOptions"/> instance that matches the given <paramref name="name"/>.</returns>
        /// <exception cref="OptionsValidationException">One or more <see cref="IValidateOptions{TOptions}"/> return failed <see cref="ValidateOptionsResult"/> when validating the <typeparamref name="TOptions"/> instance been created.</exception>
        /// <exception cref="MissingMethodException">The <typeparamref name="TOptions"/> does not have a public parameterless constructor or <typeparamref name="TOptions"/> is <see langword="abstract"/>.</exception>
        public virtual TOptions Get(string? name)
        {
            name ??= Options.DefaultName;

            if (!_cache.TryGetValue(name, out TOptions? options))
            {
                // Store the options in our instance cache. Avoid closure on fast path by storing state into scoped locals.
                IOptionsFactory<TOptions> localFactory = _factory;
                string localName = name;
                options = _cache.GetOrAdd(name, () => localFactory.Create(localName));
            }

            return options;
        }
    }
}
