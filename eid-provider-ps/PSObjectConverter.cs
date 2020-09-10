using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Management.Automation;

namespace com.sorlov.eidprovider.ps
{
    public static class PSObjectConverter
    {

        private static bool KeyExist(EIDResult resultObject, string keyName)
        {
            return !String.IsNullOrEmpty(resultObject[keyName]?.ToString());
        }

        public static PSObject EIDResult(EIDResult objectToDecorate)
        {
            PSPropertySet defaltDisplayProperties;
            PSObject resultObject = new PSObject(objectToDecorate);

            //The one param we will always have!
            resultObject.Members.Add(new PSNoteProperty("Status", objectToDecorate.Status));

            //Setup response specific display properties
            switch (objectToDecorate.Status)
            {
                case eidprovider.EIDResult.ResultStatus.initialized:
                    resultObject.Members.Add(new PSNoteProperty("Id", objectToDecorate["id"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("Code", "initialized"));
                    resultObject.Members.Add(new PSNoteProperty("AutostartToken", objectToDecorate["extra"]["autostart_token"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("AutostartUrl", objectToDecorate["extra"]["autostart_url"].ToString()));
                    defaltDisplayProperties = new PSPropertySet("DefaultDisplayPropertySet", new[] { "Status", "Code", "Id" });
                    break;
                case eidprovider.EIDResult.ResultStatus.error:
                    resultObject.Members.Add(new PSNoteProperty("Code", objectToDecorate["code"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("Description", objectToDecorate["description"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("Details", KeyExist(objectToDecorate,"details") ? objectToDecorate["details"].ToString() : string.Empty));
                    defaltDisplayProperties = new PSPropertySet("DefaultDisplayPropertySet", new[] { "Status", "Code" });
                    break;
                case eidprovider.EIDResult.ResultStatus.pending:
                    resultObject.Members.Add(new PSNoteProperty("Code", objectToDecorate["code"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("Description", objectToDecorate["description"].ToString()));
                    defaltDisplayProperties = new PSPropertySet("DefaultDisplayPropertySet", new[] { "Status", "Code" });
                    break;
                case eidprovider.EIDResult.ResultStatus.completed:
                    resultObject.Members.Add(new PSNoteProperty("Id", objectToDecorate["user"]["id"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("Code", "completed"));
                    resultObject.Members.Add(new PSNoteProperty("Firstname", objectToDecorate["user"]["firstname"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("Lastname", objectToDecorate["user"]["lastname"].ToString()));
                    resultObject.Members.Add(new PSNoteProperty("Fullname", objectToDecorate["user"]["fullname"].ToString()));

                    PSObject psCustomObject = new PSObject();
                    if (KeyExist(objectToDecorate, "extra"))
                        ((JObject)objectToDecorate["extra"]).Properties().Select(p => p.Name).ToList().ForEach((item)=> { psCustomObject.Members.Add(new PSNoteProperty(item, objectToDecorate["extra"][item].ToString())); });
                    resultObject.Members.Add(new PSNoteProperty("Extra", psCustomObject));

                    defaltDisplayProperties = new PSPropertySet("DefaultDisplayPropertySet", new[] { "Status", "Id", "Firstname", "Lastname", "Fullname"});
                    break;
                default:
                    defaltDisplayProperties = new PSPropertySet("DefaultDisplayPropertySet", new[] { "Status" });
                    break;
            }

            //Add default display designator
            resultObject.Members.Add(new PSMemberSet("PSStandardMembers", new[] { defaltDisplayProperties }));
            return resultObject;
        }

    }
}
