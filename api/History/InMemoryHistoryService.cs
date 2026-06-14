using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.History
{
    public class InMemoryHistoryService : IHistoryService
    {
        private readonly ConcurrentDictionary<string, List<ChatMessage>> store = new();

        public Task<List<ChatMessage>> GetHistoryAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return Task.FromResult(new List<ChatMessage>());
            }

            var list = store.GetOrAdd(conversationId, _ => new List<ChatMessage>());
            lock (list)
            {
                return Task.FromResult(new List<ChatMessage>(list));
            }
        }

        public Task AddMessageAsync(string conversationId, ChatMessage message)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return Task.CompletedTask;
            }

            var list = store.GetOrAdd(conversationId, _ => new List<ChatMessage>());
            lock (list)
            {
                list.Add(message);
            }
            return Task.CompletedTask;
        }

        public Task ClearHistoryAsync(string conversationId)
        {
            if (!string.IsNullOrEmpty(conversationId))
            {
                store.TryRemove(conversationId, out _);
            }
            return Task.CompletedTask;
        }
    }
}
