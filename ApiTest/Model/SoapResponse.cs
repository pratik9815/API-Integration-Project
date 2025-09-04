using System.Xml.Serialization;

[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class ResponseEnvelope
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public ResponseBody Body { get; set; }
}

public class ResponseBody
{
    [XmlElement(ElementName = "Response", Namespace = "Services")]
    public Response AmendmentResponse { get; set; }
}

public class Response
{
    [XmlElement(ElementName = "Status", Namespace = "Services")]
    public string Status { get; set; }

    [XmlElement(ElementName = "Message", Namespace = "Services")]
    public string Message { get; set; }
}
