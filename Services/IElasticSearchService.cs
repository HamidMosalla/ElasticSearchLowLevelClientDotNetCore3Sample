using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using ElasticSearchLowLevelClientDotNetCore3Sample.Models;

namespace ElasticSearchLowLevelClientDotNetCore3Sample.Services
{
    public interface IElasticSearchService
    {
        Task<IElasticsearchResponse> CreateIndex(string indexName);
        Task<StringResponse> SearchQueryAsync(int id);
        //Task<ISearchResponse<Avatar>> GetMatchPhraseAsync(string matchPhrase);
        //Task<MultiSearchResponse> MultiSearchAsync(string[] matchTerms);
        //Task<List<ISearchResponse<Avatar>>> BulkMatchAsync(string[] matchTerms);
        //Task<ISearchResponse<Avatar>> FilterAsync();
        Task<StringResponse> IndexAsync(Avatar avatar);
        Task<StringResponse> BulkIndexAsync(IReadOnlyCollection<Avatar> avatars);
        Task<DynamicResponse> DeleteIndexAsync();
    }
}
