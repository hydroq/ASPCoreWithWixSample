using System.IO;
using System.Text;

namespace REFLEKT.ONEAuthor.Application.Utilities
{
    public class ExtendedStringWriter : StringWriter
    {
        public ExtendedStringWriter(StringBuilder builder, Encoding desiredEncoding)
            : base(builder)
        {
            Encoding = desiredEncoding;
        }

        public override Encoding Encoding { get; }
    }
}