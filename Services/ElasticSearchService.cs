using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Elasticsearch.Net;
using ElasticSearchLowLevelClientDotNetCore3Sample.Models;
using Microsoft.AspNetCore.Http;

namespace ElasticSearchLowLevelClientDotNetCore3Sample.Services
{

    public class ElasticSearchService : IElasticSearchService
    {
        private readonly IElasticLowLevelClient _elasticClient;

        public ElasticSearchService(IElasticLowLevelClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<IElasticsearchResponse> CreateIndex(string indexName)
        {
            // TODO:Hamid: need settings later
            // var indexSettings = new IndexSettings { NumberOfReplicas = 1, NumberOfShards = 1 };
            // c.InitializeUsing(indexSettings)

            if (_elasticClient.Indices.Exists<StringResponse>(indexName).ApiCall.HttpStatusCode == StatusCodes.Status200OK)
            {
                await DeleteIndexAsync();
            }

            var mappings = new
            {
                mappings = new
                {
                    properties = new
                    {
                        Id = new { type = "long" },
                        FirstName = new { type = "text" },
                        LastName = new { type = "text" },
                        Email = new { type = "text" },
                        PhoneNumber = new { type = "text" },
                        Country = new { type = "text" },
                        CurrentPosition = new { type = "text" }
                    }
                }
            };

            var postData = PostData.Serializable(mappings);

            return await _elasticClient.Indices.CreateAsync<DynamicResponse>(indexName, postData);
        }

        //public Task<ISearchResponse<Avatar>> SearchQueryAsync(int id)
        //{
        //    return _elasticClient.SearchAsync<Avatar>(s =>
        //        s.From(0).Size(10000).Query(q => q.Term(t => t.Id, id)));

        //    /*
        //        POST ht-index/_search
        //        {
        //           "query":{
        //              "match":{
        //                 "firstName":"Hamid"
        //              }
        //           }
        //        }
        //    */
        //}

        //public Task<ISearchResponse<Avatar>> GetMatchPhraseAsync(string matchPhrase)
        //{
        //    return _elasticClient.SearchAsync<Avatar>(s => s
        //            .From(0)
        //            .Size(10000)
        //            .Query(q =>
        //                q.MatchPhrase(mq =>
        //                    mq.Field(f => f.CurrentPosition).Query(matchPhrase))));
        //}

        //public async Task<List<ISearchResponse<Avatar>>> BulkMatchAsync(string[] matchTerms)
        //{
        //    // we need to use bulk method of the client instead, this is just a sample
        //    var matchTasks = matchTerms.Select(m => _elasticClient.SearchAsync<Avatar>(term =>
        //        term.From(0)
        //            .Size(10000)
        //            .Query(q => q.Match(mq => mq.Field(f => f.FirstName).Query(m)))));

        //    return (await Task.WhenAll(matchTasks)).ToList();
        //}

        //public Task<MultiSearchResponse> MultiSearchAsync(string[] matchTerms)
        //{
        //    var searchRequests = matchTerms.ToDictionary(a => a, a => new SearchRequest<Avatar>
        //    {
        //        Query = new MatchQuery
        //        {
        //            Field = new Field("FirstName"),
        //            Query = a
        //        },
        //        //new TermQuery
        //        //{
        //        //    Field = "FirstName",
        //        //    Value = value
        //        //},
        //    } as ISearchRequest);

        //    var multiSearchRequest = new MultiSearchRequest
        //    {
        //        Operations = searchRequests
        //    };

        //    // Chain dynamically
        //    var accumulatedQueries = AccumulatedQueries(matchTerms);

        //    return _elasticClient.MultiSearchAsync(index: null, accumulatedQueries);

        //    // Chain statically
        //    //return _elasticClient.MultiSearchAsync(null, ms => ms
        //    //    .Search<Avatar>("Hamid", s => s.MatchAll())
        //    //    .Search<Avatar>("Jimmy", s => s.MatchAll())
        //    //);

        //    // Using query dictionary
        //    //return _elasticClient.MultiSearchAsync(multiSearchRequest);
        //}

        //private Func<MultiSearchDescriptor, IMultiSearchRequest> AccumulatedQueries(string[] matchTerms)
        //{
        //    var queries = new List<Func<MultiSearchDescriptor, IMultiSearchRequest>>();

        //    foreach (var matchTerm in matchTerms)
        //    {
        //        queries.Add(des => des.Search<Avatar>(matchTerm,
        //            s => s.Query(q
        //                => q.Match(mq => mq.Field(f => f.FirstName).Query(matchTerm)))));
        //    }

        //    Func<MultiSearchDescriptor, IMultiSearchRequest> accumulatedQueries = null;

        //    foreach (var query in queries)
        //    {
        //        accumulatedQueries += query;
        //    }

        //    return accumulatedQueries;
        //}

        //public Task<ISearchResponse<Avatar>> FilterAsync()
        //{
        //    return _elasticClient.SearchAsync<Avatar>(s =>
        //        s
        //            .From(0)
        //            .Size(10000)
        //            .Query(q => q
        //                .Bool(b => b
        //                    .Filter(filter =>
        //                        filter.Range(m => m.Field(fld => fld.Id).GreaterThanOrEquals(3)))
        //                )
        //            ));
        //}

        //public Task<IndexResponse> IndexAsync(Avatar avatar)
        //{
        //    return _elasticClient.IndexAsync(avatar, i => i.Index("ht-index"));

        //    // To confirm you added data from Avatars, you can type this in: GET /ht-index/_search
        //}

        public Task<StringResponse> BulkIndexAsync(IReadOnlyCollection<Avatar> avatars)
        {
            var avatarObjects = new List<object>();

            foreach (var avatar in avatars)
            {
                avatarObjects.Add(new { index = new { } });
                avatarObjects.Add(new
                {
                    avatar.Id,
                    avatar.FirstName,
                    avatar.LastName,
                    avatar.Email,
                    avatar.PhoneNumber,
                    avatar.Country,
                    avatar.CurrentPosition
                });
            }

            return _elasticClient.BulkAsync<StringResponse>("ht-index", PostData.MultiJson(avatarObjects));

            // To confirm you added data from Avatars, you can type this in: GET /ht-index/_search
        }

        //public async Task<IReadOnlyCollection<IndexResponse>> BulkIndexExperimentalAsync(IReadOnlyCollection<Avatar> contents)
        //{
        //    var tasks = contents.Select((value, index) => _elasticClient.IndexAsync(value, i => i.Index("ht-index"))).ToList();

        //    return await Task.WhenAll(tasks);

        //    // To confirm you added data from Avatars, you can type this in: GET /ht-index/_search
        //}

        public Task<DynamicResponse> DeleteIndexAsync()
        {
            return _elasticClient.Indices.DeleteAsync<DynamicResponse>("ht-index");
        }
    }

}
