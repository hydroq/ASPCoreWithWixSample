using System.Text;
using System.Xml.Linq;

namespace REFLEKT.ONEAuthor.Application.Utilities
{
    public static class XmlExtensions
    {
        public static string SaveToString(this XDocument sourceDocument)
        {
            var stringWriter = new ExtendedStringWriter(new StringBuilder(), Encoding.UTF8);
            sourceDocument.Save(stringWriter);

            return stringWriter.ToString();
        }
    }
}