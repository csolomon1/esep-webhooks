using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(object input, ILambdaContext context)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
                string htmlUrl = json?.issue?.html_url;
                
                if (htmlUrl != null)
                {
                    string payload = $"{{'text':'Issue Created: {htmlUrl}'}}";
                    var client = new HttpClient();
                    var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };

                    var response = client.Send(webRequest);
                    using var reader = new StreamReader(response.Content.ReadAsStream());
                    return reader.ReadToEnd();
                }
                else
                {
                    return "Error: Input does not contain the expected 'issue.html_url' property.";
                }
            }
            catch (JsonReaderException ex)
            {
                // Log the JSON parsing error
                context.Logger.LogLine($"Error parsing JSON input: {ex.Message}");
                return $"Error parsing JSON input: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                context.Logger.LogLine($"Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}
