using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.History
{
    public interface IHistoryService
    {
        Task<List<ChatMessage>> GetHistoryAsync(string conversationId);
        Task AddMessageAsync(string conversationId, ChatMessage message);
        Task ClearHistoryAsync(string conversationId);
    }
}
