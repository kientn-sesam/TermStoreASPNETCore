using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Newtonsoft.Json.Linq;
using System.Text;
namespace TermStoreAPI.Middleware
{
    public class TermStoreHelper
    {
        private static JArray TermLabels(LabelCollection labels){
            JArray labelArray = new JArray();
            foreach (Label label in labels)
            {
                var json = new JObject();
                json.Add(new JProperty("isDefaultForLanguage", label.IsDefaultForLanguage));
                json.Add(new JProperty("language", label.Language));
                json.Add(new JProperty("value", label.Value));
                labelArray.Add(new JObject(json));
            }
            

            return labelArray;
        }

        public static JObject JBaseTerm(Term term, string tree)
        {
            JObject jTerm = new JObject();
            var termLabels = term.Labels;

            jTerm.Add(new JProperty(tree + "TermId", term.Id));
            jTerm.Add(new JProperty(tree + "TermName", term.Name));
            jTerm.Add(new JProperty(tree + "TermDescription", term.Description));
            JArray termLabelArray = TermLabels(termLabels);

            jTerm.Add(new JProperty(tree + "TermLabels", termLabelArray));  



            //Check child true          

            return jTerm;
        }


        
    }
}