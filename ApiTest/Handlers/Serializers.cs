using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace SOAPApiTest.Handlers
{
    public class Serializers
    {
        public string XMLSerializer<T>(T item)
        {
            // Initialize the XmlSerializer for the type T
            var serializer = new XmlSerializer(typeof(T));

            // Create a StringWriter to hold the XML string output
            using (var sw = new StringWriter()) 
            {
                // Set up the XmlWriter with indentation and no XML declaration
                var xmlWriterSettings = new XmlWriterSettings
                {
                    Encoding = System.Text.Encoding.UTF8,  // Ensure UTF-8 encoding (false to omit BOM)
                    Indent = true,
                    OmitXmlDeclaration = false
                };
                // Define the namespaces (SOAP, xsi, xsd)
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                namespaces.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                using (var xmlWriter = XmlWriter.Create(sw, xmlWriterSettings))
                {
                    // Add soap prefix to root element
                    serializer.Serialize(xmlWriter, item, namespaces);
                    return sw.ToString();
                }
            }
        }
    }
}
