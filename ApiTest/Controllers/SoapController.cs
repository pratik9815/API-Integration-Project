using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using SOAPApiTest.Handlers;
using SOAPApiTest.Model;
namespace SOAPApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoapController : ControllerBase
    {
        private readonly string _URL;
        private readonly Serializers _serializers;
        private readonly RequestHandler _requestHandler;
        public SoapController(Serializers serializers, RequestHandler requestHandler)
        {
            _URL = "https://www.dataaccess.com";
            _serializers = serializers;
            _requestHandler = requestHandler;
        }
        [HttpPost("/numberconversion/{num}")]
        public async Task<ActionResult> NumberConversionToWords([FromRoute]string num)
        {
            if (string.IsNullOrWhiteSpace(num))
            {
                return BadRequest("Number is requred");
            }
            if (int.TryParse(num, out int result))
            {
                NumberToWords numberToWords = new NumberToWords();
                numberToWords.UbiNum = result;
                SoapBody body = new SoapBody
                {
                    NumberToWords = numberToWords
                };
                SoapEnvelope soapEnvelope = new SoapEnvelope
                {
                    Body = body
                };
                try
                {
                    string requestBody = _serializers.XMLSerializer<SoapEnvelope>(soapEnvelope);
                        
                    if(string.IsNullOrWhiteSpace(requestBody)) 
                    {
                        return BadRequest("Not a valid request body");
                    }
                    APIDictionary dict = new APIDictionary
                    {
                        RequestBody = requestBody,
                        RequestEndpoint = $"{_URL}/webservicesserver/NumberConversion.wso",
                        Method = "POST",
                    };
                    var response = await _requestHandler.CallSOAP(dict);
                    if(!response.Item2.Equals("0"))
                    {
                        return Ok("Something went wrong while the api call");
                    }
                    response.Item1 = response.Item1.Replace("m:", "").Replace("soap:","");
                    var doc = XDocument.Parse(response.Item1);
                    var res = doc.Descendants(XName.Get("NumberToWordsResult")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(res.Value))
                    {
                        return Ok(res.Value.Trim());
                    }
                    return Ok("No result found");
                }
                catch
                {
                    return BadRequest("Somethig went wrong, please try again");
                }
            }
            else
            {
                return BadRequest("Invalid number format");
            }
        }

        [HttpPost("/numbertodollars/{num}")]
        public async Task<ActionResult> NumberConversionToDollars([FromRoute] string num)
        {
            if (string.IsNullOrWhiteSpace(num))
            {
                return BadRequest("Number is requred");
            }
            if (int.TryParse(num, out int result))
            {
                NumberToDollars numberToWords = new NumberToDollars();
                numberToWords.DNum = result;
                NumberToDollarsBody body = new NumberToDollarsBody
                {
                    NumberToDollars = numberToWords
                };
                NumberToDollarsBodyEnvelope soapEnvelope = new NumberToDollarsBodyEnvelope
                {
                    Body = body
                };
                try
                {
                    string requestBody = _serializers.XMLSerializer<NumberToDollarsBodyEnvelope>(soapEnvelope);

                    if (string.IsNullOrWhiteSpace(requestBody))
                    {
                        return BadRequest("Not a valid request body");
                    }
                    APIDictionary dict = new APIDictionary
                    {
                        RequestBody = requestBody,
                        RequestEndpoint = $"{_URL}/webservicesserver/NumberConversion.wso",
                        Method = "POST",
                    };
                    var response = await _requestHandler.CallSOAP(dict);
                    if (!response.Item2.Equals("0"))
                    {
                        return Ok("Something went wrong while the api call");
                    }
                    response.Item1 = response.Item1.Replace("m:", "").Replace("soap:", "");
                    var xdoc = XDocument.Parse(response.Item1);
                    XElement xres = xdoc.Descendants("NumberToDollarsResult").FirstOrDefault();
                    if(!string.IsNullOrEmpty(xres.Value))
                    {
                        return Ok(xres.Value.Trim());
                    }
                    return Ok("Invalid number format");
                }
                catch
                {
                    return BadRequest("Somethig went wrong, please try again");
                }
            }
            else
            {
                return BadRequest("Invalid number format");
            }
        }

        [HttpPost("/calculator/divide")]
        public async Task<ActionResult> DivideNumbers([FromForm] string numA, [FromForm] string numB)
        {
            if (!int.TryParse(numA, out int numC) || !int.TryParse(numB, out int numD))
                return BadRequest("Invalid number format"); // Exit if parsing fails
            Calculator calculator = new Calculator
            {
                IntA = numC,
                IntB = numD,
            };
            CalculatorBody body = new CalculatorBody
            {
                Divide = calculator
            };
            CalculatorEnvelope envelope = new CalculatorEnvelope
            {
                Body = body
            };
            string requestBody = _serializers.XMLSerializer(envelope);
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return BadRequest("Not a valid request body");
            }
            APIDictionary dict = new APIDictionary
            {
                RequestBody = requestBody,
                RequestEndpoint = $"http://www.dneonline.com/calculator.asmx",
                Method = "POST",
            };
            var response = await _requestHandler.CallSOAP(dict);
            if (!response.Item2.Equals("0"))
            {
                return Ok("Something went wrong while the api call");
            }
            response.Item1 = response.Item1.Replace("soap:", "");
            var xdoc = XDocument.Parse(response.Item1);
            var result = xdoc.Descendants("DivideResult").FirstOrDefault();
            if(!string.IsNullOrEmpty(result.Value))
            {
                return Ok(result.Value.Trim());
            }
            return Ok("No valid response obtained");
        }
    }
}
