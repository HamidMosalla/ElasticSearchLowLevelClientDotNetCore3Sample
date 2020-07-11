using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticSearchLowLevelClientDotNetCore3Sample.Models;
using ElasticSearchLowLevelClientDotNetCore3Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearchLowLevelClientDotNetCore3Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IElasticSearchService _elasticSearchService;

        public HomeController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public async Task<IActionResult> Index()
        {
            var createIndexResult = await _elasticSearchService.CreateIndex(IndexNames.Avatar);

            var avatars = new List<Avatar>
            {
                new Avatar
                {
                    Id = 1,
                    FirstName = "Hamid",
                    LastName = "Mosalla",
                    Email = "Xellarix@gmail.com",
                    CurrentPosition = "developer",
                    Country = "France",
                    PhoneNumber = "3234234234",
                    About = "I love to go rock climbing.",
                    Age = 14,
                    Interests = new List<string>{"music"}
                },
                new Avatar
                {
                    Id = 2,
                    FirstName = "Jimmy",
                    LastName = "Late",
                    Email = "jlate@gmail.com",
                    CurrentPosition = "architect",
                    Country = "France",
                    PhoneNumber = "5557657645",
                    About = "I love to go mountain climbing.",
                    Age = 32,
                    Interests = new List<string>{ "jamaharon" }
                },
                new Avatar
                {
                    Id = 3,
                    FirstName = "Malfo",
                    LastName = "Kiev",
                    Email = "mkiev@gmail.com",
                    CurrentPosition = "manager",
                    Country = "France",
                    PhoneNumber = "987767685",
                    About = "I love to go fishing.",
                    Age = 25,
                    Interests = new List<string>{ "forestry", "sports" }
                },
                new Avatar
                {
                    Id = 4,
                    FirstName = "Tammy",
                    LastName = "Kiev",
                    Email = "tkiev@gmail.com",
                    CurrentPosition = "intern",
                    Country = "France",
                    PhoneNumber = "994785989",
                    About = "I love to sleep.",
                    Age = 30,
                    Interests = new List<string>{ "jamaharon", "sleeping",  "music"}
                }
            };

            //var indexAsyncResult = await _elasticSearchService.IndexAsync(avatars.First());
            var bulkInsertResult = await _elasticSearchService.BulkIndexAsync(avatars);

            var searchQueryResult = await _elasticSearchService.SearchTermQueryAsync(2);

            string[] matchTerms =
            {
                "Hamid",
                "Jimmy"
            };

            var multiSearchResult = await _elasticSearchService.MultiSearchAsync(matchTerms);

            var bulkMatchResult = await _elasticSearchService.BulkMatchAsync(matchTerms);

            string matchPhrase = "developer";

            var matchPhraseResult = await _elasticSearchService.GetMatchPhraseAsync(matchPhrase);

            var filterResult = await _elasticSearchService.FilterRangeQueryAsync();

            return View();
        }
    }
}
