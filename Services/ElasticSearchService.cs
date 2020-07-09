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

        public Task<StringResponse> SearchTermQueryAsync(int id)
        {
            return _elasticClient.SearchAsync<StringResponse>("ht-index", PostData.Serializable(new
            {
                from = 0,
                size = 10000,
                query = new
                {
                    term = new
                    {
                        Id = id
                    }
                }
            }));

            /*
                POST ht-index/_search
                {
                   "query":{
                      "term":{
                         "id":"2"
                      }
                   }
                }
            */
        }

        public Task<StringResponse> SearchMatchQueryAsync(int id)
        {
            return _elasticClient.SearchAsync<StringResponse>("ht-index", PostData.Serializable(new
            {
                from = 0,
                size = 10000,
                query = new
                {
                    match = new
                    {
                        Id = id
                    }
                }
            }));

            /*
                POST ht-index/_search
                {
                   "query":{
                      "match":{
                         "id":"2"
                      }
                   }
                }
            */
        }

        public Task<StringResponse> GetMatchPhraseAsync(string matchPhrase)
        {
            return _elasticClient.SearchAsync<StringResponse>("ht-index", PostData.Serializable(new
            {
                from = 0,
                size = 10000,
                query = new
                {
                    match_phrase = new
                    {
                        CurrentPosition = new
                        {
                            query = matchPhrase
                        }
                    }
                }
            }));
        }

        public async Task<List<StringResponse>> BulkMatchAsync(string[] matchTerms)
        {
            // we need to use bulk method of the client instead, this is just a sample
            var matchTasks = matchTerms.Select(m => _elasticClient.SearchAsync<StringResponse>("ht-index", PostData.Serializable(new
            {
                from = 0,
                size = 10000,
                query = new
                {
                    match = new
                    {
                        FirstName = m
                    }
                }
            })));

            return (await Task.WhenAll(matchTasks)).ToList();
        }

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

        public Task<StringResponse> FilterRangeQueryAsync()
        {
            return _elasticClient.SearchAsync<StringResponse>("ht-index", PostData.Serializable(new
            {
                from = 0,
                size = 10000,
                query = new
                {
                    @bool = new
                    {
                        filter = new
                        {
                            range = new
                            {
                                Id = new {gte = 2, lte = 3}
                            }
                        }
                    }
                }
            }));

            /*
                POST ht-index/_search
                {
                   "bool" : {
                       "filter": {
                         "range" : {
                             "Id" : { "gte" : 2, "lte" : 3 }
                        }
                      }
                   }
                }
            */
        }

        public Task<StringResponse> IndexAsync(Avatar avatar)
        {
            return _elasticClient.IndexAsync<StringResponse>("ht-index", PostData.Serializable(avatar));

            // To confirm you added data from Avatars, you can type this in: GET /ht-index/_search
        }

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

        public Task<DynamicResponse> DeleteIndexAsync()
        {
            return _elasticClient.Indices.DeleteAsync<DynamicResponse>("ht-index");
        }
    }

}
