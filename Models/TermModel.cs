using System.Collections.Generic;

namespace TermStoreAPI.Models
{
    public class TermModel
    {
        
        public int Lcid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Termstore { get; set; }
        public string TermGroup { get; set; }
        public string Termset { get; set; }
        public string Term { get; set; }
        public IDictionary<string, string> LocalCustomProperty { get; set; }
        public IDictionary<string, string> CustomProperty { get; set; }
        public IDictionary<string, string> children { get; set; }
        public List<Label> Labels { get; set; }
        
    }
    public class Label 
    {
        public bool IsDefaultForLanguage { get; set; }
        public int Language { get; set; }
        public string Value { get; set; }
    }
}