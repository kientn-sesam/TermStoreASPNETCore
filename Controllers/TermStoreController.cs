using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Text;


using TermStoreAPI.Middleware;
using TermStoreAPI.Models;

namespace TermStoreAPI.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class TermStoreController : ControllerBase
    {
        public string _username, _password, _baseurl;
        public TermStoreController()
        {
            using (var file = System.IO.File.OpenText(".auth.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);
                _baseurl = jObject.GetValue("_baseurl").ToString();
                _username = jObject.GetValue("_username").ToString();
                _password = jObject.GetValue("_password").ToString();
            }
        }
        [HttpGet]
        [Produces("application/json")]
        [Consumes("application/json")]
        /// <summary>
        /// Fetch all terms from term set
        /// Required query termsstore guid, termgroup and term set.
        /// </summary>
        /// <param name=""termstore""></param>
        /// <returns></returns>
        public async Task<IActionResult> AllTerms([FromQuery(Name = "termstore")] string _termstore, [FromQuery(Name = "termgroup")] string _termgroup, [FromQuery(Name = "termset")] string _termset)
        {

            using(ClientContext cc = AuthHelper.GetClientContextForUsernameAndPassword(_baseurl, _username, _password))
            try
            {

                List<JObject> listTerms = new List<JObject>();
                TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
                var termStores = taxonomySession.TermStores;
                TermStore termStore = termStores.GetById(new Guid(_termstore));
                //TermStore termStore = termStores.GetByName(_termstore);
                var termGroups = termStore.Groups;
                var termGroup = termGroups.GetByName(_termgroup);
                var termSets = termGroup.TermSets;
                //var termSet = termSets.GetByName("AEN Prosjekter");
                
                cc.Load(termGroup,
                               group => group.Id,
                               group => group.Name,
                               group => group.TermSets.Include(
                                   set => set.Name,
                                   set => set.Id,
                                   set => set.Terms.Include(
                                       term => term.Id,
                                       term => term.Name,
                                       term => term.Description,
                                       term => term.Labels,
                                term  => term.Terms.Include(
                                    term => term.Name,
                                    term => term.Description,
                                    term => term.Id,
                                    term=> term.Labels,
                                    term => term.Terms.Include(
                                        term => term.Name,
                                        term => term.Description,
                                        term=> term.Id,
                                        term => term.Labels,
                                        term => term.Terms.Include(
                                            term => term.Name,
                                            term => term.Description,
                                            term=> term.Id,
                                            term=> term.Labels,
                                            term => term.Terms.Include(
                                                term => term.Name,
                                                term => term.Description,
                                                term=> term.Id,
                                                term=> term.Labels,
                                                term => term.Terms.Include(
                                                    term => term.Name,
                                                    term => term.Description,
                                                    term=> term.Id,
                                                    term=> term.Labels
                                                )
                                            
                                        )
                                    )
                                )
                            )
                           )
                       )
                );

                await cc.ExecuteQueryAsync();
                for (int i = 0; i < termSets.Count; i++)
                {
                    if (termSets[i].Name.Equals(_termset))
                    {
                        TermSet termSet = termSets[i];
                        var terms = termSet.Terms;
                        foreach (var term in terms)
                        {
                            var json = TermStoreHelper.JBaseTerm(term, "A");
                            json.Add(new JProperty("termSetId", termSet.Id));
                            json.Add(new JProperty("termSetName", termSet.Name));
                            json.Add(new JProperty("termGroup", termGroup.Name));
                            json.Add(new JProperty("termGroupId", termGroup.Id));

                            JArray jsonChildTerms = new JArray();
                            TermCollection childTerms = term.Terms;
                            if (childTerms.Count > 0)
                            {
                                foreach (var childTerm in childTerms)
                                {
                                    JObject jsonChildTerm = TermStoreHelper.JBaseTerm(childTerm, "B");
                                    JArray jsonGrandChildTerms = new JArray();
                                    TermCollection grandChildTerms = childTerm.Terms;

                                    if (grandChildTerms.Count > 0)
                                    {
                                        foreach (var grandChildTerm in grandChildTerms)
                                        {
                                            JObject jsonGrandChildTerm = TermStoreHelper.JBaseTerm(grandChildTerm, "C");
                                            JArray jsonDTerms = new JArray();
                                            TermCollection dTerms = grandChildTerm.Terms;
                                            
                                            if (dTerms.Count > 0)
                                            {
                                                foreach (var dTerm in dTerms)
                                                {
                                                    JObject jsonDTerm = TermStoreHelper.JBaseTerm(dTerm, "D");

                                                    
                                                    jsonDTerms.Add(new JObject(jsonDTerm));
                                                }
                                            }
                                            jsonGrandChildTerms.Add(new JObject(jsonGrandChildTerm));


                                        }
                                        jsonChildTerm.Add(new JProperty("CChildTerms", jsonGrandChildTerms));
                                    }
                                    jsonChildTerms.Add(new JObject(jsonChildTerm));
                                }        
                                json.Add(new JProperty("BTerms", jsonChildTerms));             
                            }
                           
                            listTerms.Add(json);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                    
                
                
                return new OkObjectResult(listTerms);

                
            }
            catch (System.Exception)
            {
                
                throw;
            }

        }

        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        /// <summary>
        /// Create Term or labels.
        ///     POST /api/termstore/create
        ///     
        ///     [{
        ///     	"TermStore": <TermStore GUID>,
        ///     	"TermGroup": <TermGroup Name>,
        ///     	"Termset": <TermSet Name>,
        ///     	"Term": <Mother Term Name>,
        ///     	"Lcid": <Language id>,
        ///     	"children": {
        ///     		"aTermName": "another child",
        ///     		"bTermName": "another child in child",
        ///     		"cTermName": "Sverres tre",
        ///     		"dTermName": "Sverres tre2",
        ///     		"eTermName": "Sverres tre5",
        ///             ...
        ///     	},
        ///     	"Labels": [
        ///     		{
        ///     		"isDefaultForLanguage": false,
        ///     		"language": 1033,
        ///     		"value": "Test23"
        ///     		},
        ///     		{
        ///     		"isDefaultForLanguage": false,
        ///     		"language": 1033,
        ///     		"value": "Test21231111111"
        ///     		},
        ///             ...
        ///     	]
        ///     
        ///     	
        ///     }]
        /// </summary>
        /// <param name="listT"></param>
        /// <returns></returns>
        public async Task<IActionResult> Create([FromBody] TermModel[] listT)
        {
            
            using (ClientContext cc = AuthHelper.GetClientContextForUsernameAndPassword(_baseurl, _username, _password))
            try
            {
                
                List<JObject> listTerms = new List<JObject>();
                
                //List<JObject> docs = param.ToObject<List<JObject>>();

                foreach (var t in listT)
                {
                    string termStoreGUID = t.Termstore;
                    var termGroupName = t.TermGroup;
                    var termSetName = t.Termset;
                    IDictionary<string, string> children = t.children;

                    TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
                    
                    TermStore termStore = taxonomySession.TermStores.GetById(new Guid(termStoreGUID));
                    TermGroup termGroup = termStore.Groups.GetByName(termGroupName);
                    TermSet termSet = termGroup.TermSets.GetByName(termSetName);
                    Term term = termSet.Terms.GetByName(t.Term);
                    char c = 'a';
                    string termName = "";
                    string name = "";
                    

                    for (int j = 0; j < children.Count - 1; j++)
                    {
                        //string id = c + "TermId";
                        name = c + "TermName";
                        termName = children[name];
                        
                        term = term.Terms.GetByName(termName);
                        
                        c++;
                    }

                    cc.Load(term, t => t.Name, t => t.Labels, t => t.Terms.Include(
                        ct => ct.Name,
                        ct => ct.Labels
                    ));
                    await cc.ExecuteQueryAsync();

                    name = c + "TermName";
                    termName = children[name];
                    if (term.Terms.Any(x => x.Name != children[name]))
                    {
                        term = term.CreateTerm(termName, t.Lcid, Guid.NewGuid());
                        if (t.Labels != null)
                        {
                            foreach (var label in t.Labels) 
                            {
                                term.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                    }
                    else
                    {
                        //term = term.Terms.GetByName(termName);
                        var terms = term.Terms;
                        for (int j = 0; j < terms.Count; j++)
                        {
                            if (terms[j].Name.Equals(termName))
                                term = terms[j];
                            else
                                continue;
                        }
                        //cc.Load(term, t => t.Name, t => t.Labels);
                        //await cc.ExecuteQueryAsync();
                        if (t.Labels != null)
                        {
                            foreach (var label in t.Labels)
                            {
                                if (!term.Labels.Any(x => x.Value == label.Value))
                                {
                                    term.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    if (label.IsDefaultForLanguage == true)
                                    {
                                        term.Name = label.Value;
                                    }
                                    
                                }
                            }
                        }

                    }

                    
                    if (t.Description != null)
                        term.SetDescription(t.Description, t.Lcid);
                    

                    if (t.LocalCustomProperty != null) 
                    {
                        foreach (var customLocalProperty in t.LocalCustomProperty) 
                        {
                            term.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                        }
                    }
                    if (t.CustomProperty != null) 
                    {
                        foreach (var customProperty in t.CustomProperty) 
                        {
                            term.SetCustomProperty(customProperty.Key, customProperty.Value);
                        }
                    }
                    
                    
                    
                    Console.WriteLine("Writing term : " + t.Name);
                    termStore.CommitAll();
                    cc.ExecuteQuery();
                    


                }
                
                return new OkObjectResult(listTerms);
                
            }
            catch (System.Exception)
            {
                
                throw;
            }
           
        }


    }
}
