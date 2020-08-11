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
                    dynamic = "strict", // or "false" if you don't want to get errors
                    // type= as for specifying type, only one type per index, this is not required
                    avatar = new
                    {
                        properties = new
                        {
                            Id = new {type = "long"},
                            FirstName = new {type = "text"},
                            LastName = new {type = "text"},
                            Email = new {type = "text"},
                            PhoneNumber = new {type = "text"},
                            Country = new {type = "text"},
                            CurrentPosition = new {type = "text"},
                            Age = new {type = "integer"},
                            Interests = new
                            {
                                type = "keyword",
                                analyzer = "standard"
                            }
                        }
                    }
                }
            };

            var postData = PostData.Serializable(mappings);

            return await _elasticClient.Indices.CreateAsync<DynamicResponse>(indexName, postData);
        }

        public Task<StringResponse> SearchTermQueryAsync(int id)
        {
            return _elasticClient.SearchAsync<StringResponse>(IndexNames.Avatar, PostData.Serializable(new
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
            return _elasticClient.SearchAsync<StringResponse>(IndexNames.Avatar, PostData.Serializable(new
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
            return _elasticClient.SearchAsync<StringResponse>(IndexNames.Avatar, PostData.Serializable(new
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
            var matchTasks = matchTerms.Select(m => _elasticClient.SearchAsync<StringResponse>(IndexNames.Avatar, PostData.Serializable(new
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

        public Task<StringResponse> MultiSearchAsync(string[] matchTerms)
        {
            var payload = new List<object>();

            foreach (var matchTerm in matchTerms)
            {
                payload.Add(new { });
                payload.Add(new
                {
                    query = new
                    {
                        match = new
                        {
                            FirstName = matchTerm
                        }
                    }
                });
            }

            return _elasticClient.MultiSearchAsync<StringResponse>(index: IndexNames.Avatar, PostData.MultiJson(payload));
        }

        public Task<StringResponse> FilterRangeQueryAsync()
        {
            return _elasticClient.SearchAsync<StringResponse>(IndexNames.Avatar, PostData.Serializable(new
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
                                Id = new { gte = 2, lte = 3 }
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
            return _elasticClient.IndexAsync<StringResponse>(IndexNames.Avatar, PostData.Serializable(avatar));

            // To confirm you added data from Avatars, you can type this in: GET /ht-index/_search
        }

        public Task<StringResponse> BulkIndexAsync(IReadOnlyCollection<Avatar> avatars)
        {
            var avatarObjects = new List<object>();

            foreach (var avatar in avatars)
            {
                // This line is needed to specify the bulk operation, which in this case is indexing, it's refer to as metadata section
                avatarObjects.Add(new { index = new { } });
                avatarObjects.Add(new
                {
                    id = avatar.Id,
                    firstName = avatar.FirstName,
                    lastName = avatar.LastName,
                    email = avatar.Email,
                    phoneNumber = avatar.PhoneNumber,
                    country = avatar.Country,
                    currentPosition = avatar.CurrentPosition,
                    about = avatar.About,
                    age = avatar.Age,
                    interests = avatar.Interests
                });
            }

            return _elasticClient.BulkAsync<StringResponse>(IndexNames.Avatar, PostData.MultiJson(avatarObjects));

            // To confirm you added data from Avatars, you can type this in: GET /ht-index/_search
        }

        public Task<DynamicResponse> DeleteIndexAsync()
        {
            return _elasticClient.Indices.DeleteAsync<DynamicResponse>(IndexNames.Avatar);
        }
    }

}
