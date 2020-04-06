using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.NewtonsoftJson;
using Microsoft.Extensions.Configuration.Memory;


namespace Microsoft.Extensions.Configuration
{
    public class WriteableJsonConfigurationProvider : NewtonsoftJsonConfigurationProvider
    {
        public WriteableJsonConfigurationProvider(NewtonsoftJsonConfigurationSource source) : base(source)
        {

        }

        public override void Set(string key, string value)
        {
            base.Set(key, value);

            var keys = key.Split(':');
            var obj = JObject.Parse(File.ReadAllText(Source.Path));
            var prop = keys[..^1].Aggregate(obj, (r, x) =>
             {
                 if (!r.ContainsKey(x)) r.Add(x, new JObject());
                 return (JObject)r[x];
             });
            prop[keys[^1]] = value;
            File.WriteAllText(Source.Path, obj.ToString());
        }
    }
    public class WritableJsonConfigurationSource : NewtonsoftJsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new WriteableJsonConfigurationProvider(this);
        }
    }
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            return AddConfiguration(services, builder => { });
        }
        public static IServiceCollection AddConfiguration(this IServiceCollection services, Action<IConfigurationBuilder> configure)
        {
            var config = new ConfigurationBuilder();
            configure(config);
            services.AddSingleton<IConfiguration>(config.Build());
            return services;
        }

        public static IConfigurationBuilder AddWriteableJsonFile(this IConfigurationBuilder builder, string path, bool optional = false, bool reloadOnChange = false)
        {
            builder.Add<WritableJsonConfigurationSource>(source =>
            {
                source.Path = path;
                source.Optional = optional;
                source.ReloadOnChange = reloadOnChange;
                source.ResolveFileProvider();
            });

            return builder;
        }
        public static IConfigurationBuilder AddDefaultConfiguration(this IConfigurationBuilder builder)
        {
            return AddDefaultConfiguration(builder, source => { });
        }
        public static IConfigurationBuilder AddDefaultConfiguration(this IConfigurationBuilder builder, Action<MemoryConfigurationSource> configure)
        {
            var initialData = new Dictionary<string, string>{
                {"id",System.Net.Dns.GetHostName().ToLower()},
                {"modules:controller:host","https://modulaiot:5000"},
                {"modules:controller:key","modulaiot"}
            };
            var source = new MemoryConfigurationSource { InitialData = initialData };
            configure(source);
            builder.Add(source);
            return builder;
        }

        public static IConfiguration GetModuleConfig(this IConfiguration configuration, string id)
        {
            return configuration.GetSection($"modules:{id}");
        }

        public static void SetValue<T>(this IConfiguration configuration, string key, T value)
        {
            configuration[key] = new JValue(value).ToString();
        }

    }
}