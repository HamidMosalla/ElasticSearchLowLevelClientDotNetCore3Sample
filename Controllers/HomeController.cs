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
            var createIndexResult = await _elasticSearchService.CreateIndex("ht-index");

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
                    PhoneNumber = "3234234234"
                },
                new Avatar
                {
                    Id = 2,
                    FirstName = "Jimmy",
                    LastName = "Late",
                    Email = "jlate@gmail.com",
                    CurrentPosition = "architect",
                    Country = "France",
                    PhoneNumber = "5557657645"
                },
                new Avatar
                {
                    Id = 3,
                    FirstName = "Malfo",
                    LastName = "Kiev",
                    Email = "mkiev@gmail.com",
                    CurrentPosition = "manager",
                    Country = "France",
                    PhoneNumber = "987767685"
                }
            };

            //var indexAsyncResult = await _elasticSearchService.IndexAsync(avatars.First());
            var bulkInsertResult = await _elasticSearchService.BulkIndexAsync(avatars);

            //var searchQueryResult = await _elasticSearchService.SearchQueryAsync(2);

            //string[] matchTerms =
            //{
            //    "Hamid",
            //    "Jimmy"
            //};

            //var multiSearchResult = await _elasticSearchService.MultiSearchAsync(matchTerms);

            //var bulkMatchResult = await _elasticSearchService.BulkMatchAsync(matchTerms);

            //string matchPhrase = "developer";

            //var matchPhraseResult = await _elasticSearchService.GetMatchPhraseAsync(matchPhrase);

            //var filterResult = await _elasticSearchService.FilterAsync();

            return View();
        }
    }
}
