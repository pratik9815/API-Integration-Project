using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOAPApiTest.Handlers;
using SOAPApiTest.Model;

namespace SOAPApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestController : ControllerBase
    {
        private readonly string _URL;
        private readonly RequestHandler _requestHandler;
        public RestController(RequestHandler requestHandler)
        {
            _URL = "https://restful-booker.herokuapp.com/";
            _requestHandler = requestHandler;
        }
        [HttpPost("/login")]
        public async Task<ActionResult> Login(Login login)
        {
            //"username" : "admin",
            //"password" : "password123"
            string fullUri = new Uri(new Uri(_URL), "auth").ToString();
            string content = JsonConvert.SerializeObject(login);
            APIDictionary dict = new APIDictionary
            {
                Method = "POST",
                RequestEndpoint = fullUri,
                RequestBody = content,
            };
            var resStr = await _requestHandler.CallRest(dict);
            if(!resStr.Item2.Equals("0"))
            {
                return Ok("Something went wrong while the api call");
            }    
            dynamic resObj = JsonConvert.DeserializeObject<dynamic>(resStr.Item1);
            string token = resObj.token;
            return Ok(token);
        }

        [HttpPost("/create")]
        public async Task<ActionResult> CreateBooking(CreateBooking booking)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string token = _requestHandler.GetToken(out string statusCode);
            if(!statusCode.Equals("0"))
            {
                return BadRequest(token);
            }
            string reqStr = JsonConvert.SerializeObject(booking);
            string fullUri = new Uri(new Uri(_URL), "booking").ToString();
            APIDictionary dict = new APIDictionary
            {
                Method = "POST",
                RequestEndpoint = fullUri,
                RequestBody = reqStr,
                Token = token
            };
            var resStr = await _requestHandler.CallRest(dict);
            if (!resStr.Item2.Equals("0"))
            {
                return Ok("Something went wrong while the api call");
            }
            dynamic resObj = JsonConvert.DeserializeObject<dynamic>(resStr.Item1);
            string bookingId = resObj.bookingid;
            return Ok(bookingId);
        }
        [HttpPost("/update/{id}")]
        public async Task<ActionResult> UpdateBooking(CreateBooking booking, [FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string token = _requestHandler.GetToken(out string statusCode);
            if (!statusCode.Equals("0"))
            {
                return BadRequest(token);
            }
            string reqStr = JsonConvert.SerializeObject(booking);
            string fullUri = new Uri(new Uri(_URL), $"booking/{id}").ToString();
            APIDictionary dict = new APIDictionary
            {
                Method = "PUT",
                RequestEndpoint = fullUri,
                RequestBody = reqStr,
                Token = token
            };
            var resStr = await _requestHandler.CallRest(dict);
            if (!resStr.Item2.Equals("0"))
            {
                return Ok("Something went wrong while the api call");
            }
            dynamic resObj = JsonConvert.DeserializeObject<dynamic>(resStr.Item1);
            string bookingId = resObj.bookingid;
            return Ok(bookingId);
        }
    }
}
