using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBlaster.Models
{
    public static class ProxyExtension
    {
        public static string Build(string host, int port, string user, string pass)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxy_ext");
            Directory.CreateDirectory(dir);

            File.WriteAllText(Path.Combine(dir, "manifest.json"), $@"
                {{
                    ""name"": ""BrightData Proxy"",
                    ""version"": ""1.0"",
                    ""manifest_version"": 3,
                    ""permissions"": [""proxy"", ""storage"", ""webRequest"", ""webRequestAuthProvider""],
                    ""host_permissions"": [""<all_urls>""],
                    ""background"": {{ ""service_worker"": ""bg.js"" }}
            }}");

            File.WriteAllText(Path.Combine(dir, "bg.js"), $@"
                chrome.runtime.onInstalled.addListener(() => {{
                  chrome.proxy.settings.set({{
                    value: {{
                      mode: 'fixed_servers',
                      rules: {{
                        singleProxy: {{ scheme: 'http', host: '{host}', port: {port} }},
                        bypassList: ['localhost']
                      }}
                    }},
                    scope: 'regular'
                  }});
                }});
                chrome.webRequest.onAuthRequired.addListener(
                  _ => ({{ authCredentials: {{ username: '{user}', password: '{pass}' }} }}),
                  {{ urls: ['<all_urls>'] }},
                  ['blocking']
            );");
            return dir;
        }
    }
}
