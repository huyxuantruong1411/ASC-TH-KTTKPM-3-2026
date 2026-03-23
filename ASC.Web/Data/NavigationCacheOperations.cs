using ASC.Web.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ASC.Web.Data
{
    public class NavigationCacheOperations : INavigationCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly string NavigationCacheName = "NavigationCache";

        public NavigationCacheOperations(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task CreateNavigationCacheAsync()
        {
            await _cache.SetStringAsync(
                NavigationCacheName,
                File.ReadAllText("Navigation/Navigation.json")
            );
        }

        public async Task<NavigationMenu> GetNavigationCacheAsync()
        {
            var json = await _cache.GetStringAsync(NavigationCacheName);

            if (string.IsNullOrEmpty(json))
            {
                return new NavigationMenu
                {
                    MenuItems = new List<NavigationMenuItem>()
                };
            }

            var result = JsonConvert.DeserializeObject<NavigationMenu>(json);

            return result ?? new NavigationMenu
            {
                MenuItems = new List<NavigationMenuItem>()
            };
        }
    }
}