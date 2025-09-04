using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ApiTest.SerializerHelper;

public class SoapSerializerDeserializer
{
    public static string Create<T>(T request)
    {
        // Creates a serializer based on the type of the request object
        var serializer = new XmlSerializer(typeof(T));
        var builder = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,// Encode XML in UTF-8
            Indent = true,// Makes XML readable (adds line breaks & spaces)
            OmitXmlDeclaration = true // Removes the <?xml version="1.0"?> header
        };
        using (var writer = XmlWriter.Create(builder, settings))
        {
            serializer.Serialize(writer, request,
                typeof(T).GetProperty("xmlns")?.GetValue(request, null) as XmlSerializerNamespaces);
        }

        return builder.ToString();
    }
    public static T Read<T>(string xml) where T : class
    {
        using (var reader = new StringReader(xml))
        {
            try
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
            }
            catch
            {
                return null;
            }
        }
    }
}
