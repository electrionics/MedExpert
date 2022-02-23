using System.Collections.Generic;

namespace MedExpert.Web.Results
{
    public class Result<TModel>
    {
        public TModel Model { get; set; }
        
        public bool Success { get; set; }
        
        public List<KeyValuePair<string, string>> Errors { get; set; }
    }

    public class Result
    {
        public bool Success { get; set; }
        
        public List<KeyValuePair<string, string>> Errors { get; set; }
    }
}