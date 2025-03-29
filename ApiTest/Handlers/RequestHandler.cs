using Newtonsoft.Json;
using SOAPApiTest.Model;
using System.Net.Http;
using System.Text;

namespace SOAPApiTest.Handlers
{
    public class RequestHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _restUrl;

        public RequestHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _restUrl = "https://restful-booker.herokuapp.com/";
        }

        public string GetToken(out string statusCode)
        {
            var login = new
            {
                username = "admin",
                password = "password123"
            };
            string fullUri = new Uri(new Uri(_restUrl), "auth").ToString();
            string content = JsonConvert.SerializeObject(login);
            APIDictionary dict = new APIDictionary
            {
                Method = "POST",
                RequestEndpoint = fullUri,
                RequestBody = content,
            };
            var resStr = CallRest(dict).Result;
            if (!resStr.Item2.Equals("0"))
            {
                statusCode = "999";
                return "Something went wrong while the api call";
            }
            dynamic resObj = JsonConvert.DeserializeObject<dynamic>(resStr.Item1);
            string token = resObj.token;
            if(string.IsNullOrEmpty(token)) 
            {
                statusCode = "1001";
                return "No valid token obtained";
            }
            statusCode = "0";
            return token;
        }

        public async Task<(string, string)> CallSOAP(APIDictionary dict)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Uri fullUri = new Uri(dict.RequestEndpoint);
                    //we can change the get method any where from our code
                    using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, fullUri))
                    {
                        if (dict.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                        {
                            httpRequestMessage.Method = HttpMethod.Get;
                        }
                        else
                        {
                            httpRequestMessage.Content = new StringContent(dict.RequestBody, Encoding.UTF8, "text/xml");
                            // SOAP specific headers
                            httpRequestMessage.Headers.Add("SOAPAction", dict.SoapAction ?? string.Empty);
                        }
                        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
                        //process is complete
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            //statusCode = "0";
                            return (responseContent, "0");
                        }
                        else
                        {

                            return ("Request Failed", "999");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"Unexpected error: {ex.Message}", "999");
            }
        }

        public async Task<(string, string)> CallSoapV2(APIDictionary dict)
        {
            try
            {
                // Use the factory to create a client
                var client = _httpClientFactory.CreateClient("SOAP");
                Uri fullUri = new Uri(dict.RequestEndpoint);
                using(HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, fullUri))
                {
                    // Set method and content based on request type
                    if (dict.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        httpRequestMessage.Method = HttpMethod.Get;
                    }
                    else
                    {
                        httpRequestMessage.Content = new StringContent(dict.RequestBody, Encoding.UTF8, "text/xml");
                        // SOAP specific headers
                        httpRequestMessage.Headers.Add("SOAPAction", dict.SoapAction ?? string.Empty);
                    }
                    // Add timeout handling
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // SOAP may need longer timeout
                    HttpResponseMessage response = await client.SendAsync(httpRequestMessage, cts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return (responseContent, "0");
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        return ($"Request Failed: {response.StatusCode} - {errorContent}", response.StatusCode.ToString("D"));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return ("SOAP request timed out", "408");
            }
            catch (HttpRequestException ex)
            {
                return ($"Network error: {ex.Message}", "500");
            }
            catch (Exception ex)
            {
                return ($"Unexpected error: {ex.Message}", "999");
            }
        }
        
        public async Task<(string,string)> CallRest(APIDictionary dict)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("REST");
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage
                {
                    Method = dict.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) ? HttpMethod.Get : HttpMethod.Post,
                    RequestUri = new Uri(dict.RequestEndpoint),
                };
                httpRequestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                if (!dict.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    httpRequestMessage.Content = new StringContent(dict.RequestBody, Encoding.UTF8, "application/json");
                }

                if (!string.IsNullOrEmpty(dict.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",dict.Token);
                }
                // Add timeout handling
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var response = await client.SendAsync(httpRequestMessage, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return (content, "0");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ($"Request Failed: {response.StatusCode} - {errorContent}", response.StatusCode.ToString("D"));
                }
            }
            catch(OperationCanceledException)
            {
                return ("Request timed out", "408");
            }
            catch (HttpRequestException ex)
            {
                return ($"Network error: {ex.Message}", "500");
            }
            catch (Exception ex)
            {
                return ($"Unexpected error: {ex.Message}", "999");
            }
        }
    }
}
