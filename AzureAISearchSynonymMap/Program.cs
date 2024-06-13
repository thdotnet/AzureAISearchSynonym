using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

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

        foreach(var field in index.Fields)
        {
            if (field.IsSearchable.HasValue && field.IsSearchable.Value == true)
            {
                field.SynonymMapNames.Add("abbreviations");
            }
        }

        indexClient.CreateOrUpdateIndex(index);

        Console.WriteLine("Synonym map added to the index successfully.");
        Console.ReadLine();
    }
}
