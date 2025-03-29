using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SOAPApiTest.Model;
//number conversion
// Define classes that match your XML structure
#region NumberToWords
[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class SoapEnvelope
{
    [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public SoapBody Body { get; set; }
}
public class SoapBody
{
    [XmlElement(Namespace = "http://www.dataaccess.com/webservicesserver/")]
    public NumberToWords NumberToWords { get; set; }
}
public class NumberToWords
{
    [XmlElement("ubiNum")]
    public int UbiNum { get; set; }
}
#endregion NumberToWords

#region NumberToDollars

//number to dollars
[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class NumberToDollarsBodyEnvelope
{
    [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public NumberToDollarsBody Body { get; set; }
}
public class NumberToDollarsBody
{
    [XmlElement(Namespace = "http://www.dataaccess.com/webservicesserver/")]
    public NumberToDollars NumberToDollars { get; set; }
}
public class NumberToDollars
{
    [XmlElement("dNum")]
    public int DNum { get; set; }
}
#endregion NumberToDollars

#region Calculator
// Root element with 'soap' namespace
[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class CalculatorEnvelope
{
    //public string Header { get; set; }
    [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public CalculatorBody Body { get; set; }
}
// Body element under 'soap' namespace
public class CalculatorBody
{
    [XmlElement("Divide", Namespace = "http://tempuri.org/")]
    public Calculator Divide { get; set; }
}
// Operation element under 'tempuri' namespacess
public class Calculator
{
    [XmlElement("intA")]
    public int IntA { get; set; }
    [XmlElement("intB")]
    public int IntB { get; set; }    
}
#endregion Calculator

#region Rest

public class Login
{
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
}// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Bookingdates
{
    [JsonProperty("checkin")]
    [Required]
    public string checkin { get; set; }
    [JsonProperty("checkout")]
    [Required]
    public string checkout { get; set; }
}

public class CreateBooking
{
    [JsonProperty("firstname")]
    [Required]
    public string firstname { get; set; }
    [JsonProperty("lastname")]
    [Required]
    public string lastname { get; set; }
    [JsonProperty("totalprice")]
    public int totalprice { get; set; }
    [JsonProperty("depositpaid")]
    [Required]
    public bool depositpaid { get; set; }
    [JsonProperty("bookingdates")]
    [Required]
    public Bookingdates bookingdates { get; set; }
    [JsonProperty("additionalneeds")]
    public string additionalneeds { get; set; }
}


//create request


#endregion Rest