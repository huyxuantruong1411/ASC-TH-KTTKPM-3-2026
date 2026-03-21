using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.Tests.TestUtilities
{
    public class FakeSession : ISession
    {
        // Sử dụng Dictionary để lưu trữ dữ liệu session giả mạo
        private Dictionary<string, byte[]> sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;

        public string Id => Guid.NewGuid().ToString();

        public IEnumerable<string> Keys => sessionStorage.Keys;

        public void Clear()
        {
            sessionStorage.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            sessionStorage.Remove(key);
        }

        // Ghi đè phương thức Set
        public void Set(string key, byte[] value)
        {
            sessionStorage[key] = value;
        }

        // Ghi đè phương thức TryGetValue
        public bool TryGetValue(string key, out byte[] value)
        {
            if (sessionStorage.ContainsKey(key))
            {
                value = sessionStorage[key];
                return true;
            }
            value = null!;
            return false;
        }
    }
}