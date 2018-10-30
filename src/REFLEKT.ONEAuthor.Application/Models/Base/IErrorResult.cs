using System.Collections.Generic;

namespace REFLEKT.ONEAuthor.Application.Models.Base
{
    public interface IErrorResult
    {
        IEnumerable<string> errors { get; set; }
    }
}