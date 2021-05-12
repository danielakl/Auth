namespace Auth.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;

    public static class IConfigurationExtensions
    {
        public static T GetSection<T>(this IConfiguration config, string key)
        {
            var configSection = config.GetSection(key);
            if (configSection == null)
            {
                throw new ArgumentNullException(nameof(configSection));
            }

            object result = configSection.Get(typeof(T));
            if (result == null)
            {
                return default(T);
            }

            return (T)result;
        }
    }
}
