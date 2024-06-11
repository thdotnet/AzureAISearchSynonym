using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

class Program
{
    private static string searchServiceEndpoint = "https://{}.search.windows.net";
    private static string adminApiKey = "";
    private static string indexName = "";

    static async Task Main(string[] args)
    {
        var credential = new AzureKeyCredential(adminApiKey);
        var indexClient = new SearchIndexClient(new Uri(searchServiceEndpoint), credential);

        indexClient.DeleteSynonymMap("abbreviations");
        indexClient.CreateSynonymMap(new SynonymMap("abbreviations",
            "CDE => Comprehensive Deductible Endorsement\n" +
            "UMC => Uninsured Motorist Coverage\n" +
            "BI => Bodily Injury\n" +
            "PD => Property Damage\n" +
            "PIP => Personal Injury Protection\n" +
            "UM => Uninsured Motorist\n" +
            "UIM => Underinsured Motorist\n" +
            "VIN => Vehicle Identification Number"));

        // Get the existing index
        var indexResponse = await indexClient.GetIndexAsync(indexName);
        var index = indexResponse.Value;

        var searchableField = new SearchableField("content2");
        searchableField.SynonymMapNames.Add("abbreviations");

        index.Fields.Add(searchableField);

        indexClient.CreateOrUpdateIndex(index);

        var results = new List<SearchDocument>();
        var searchClient = indexClient.GetSearchClient(indexName);
        var response = await searchClient.SearchAsync<SearchDocument>("*", new SearchOptions { } );

        await foreach (var result in response.Value.GetResultsAsync())
        {
            result.Document["content2"] = result.Document["content"];
            results.Add(result.Document);
        }

        searchClient.MergeDocuments(results);

        Console.WriteLine("Synonym map added to the index successfully.");
    }
}