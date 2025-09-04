using System.Xml.Serialization;

namespace ApiTest.Model;

[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class RequestEnvelope
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public ReqeustBody Body { get; set; }
    private const string soap = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string web = "Services";
    private static XmlSerializerNamespaces staticxmlns;
    public RequestEnvelope()
    {
        staticxmlns = new XmlSerializerNamespaces();
        staticxmlns.Add("soapenv", soap);
        staticxmlns.Add("web", web);
    }
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces xmlns { get { return staticxmlns; } set { } }
}
public class ReqeustBody
{
    [XmlElement(ElementName = "RequestData", Namespace = "Services")]
    public RequestData RequestData { get; set; }
}
public class RequestData
{
    [XmlElement(ElementName = "FirstData", Namespace = "Services")]
    public string FirstData { get; set; }

    [XmlElement(ElementName = "SecondData", Namespace = "Services")]
    public string SecondData { get; set; }
}

