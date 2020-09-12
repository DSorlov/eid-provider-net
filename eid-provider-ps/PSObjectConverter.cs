using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            return EIDResult(objectToDecorate, null, null);
        }

        public static PSObject EIDResult(EIDResult objectToDecorate, List<PSNoteProperty> additionalFields)
        {
            return EIDResult(objectToDecorate, additionalFields, null);
        }
        public static PSObject EIDResult(EIDResult objectToDecorate, List<string> additonalDefaults)
        {
            return EIDResult(objectToDecorate, null, additonalDefaults);
        }

        public static PSObject EIDResult(EIDResult objectToDecorate, List<PSNoteProperty> fields, List<string> defaults)
        {
            // List of properties to add and defaults to set
            if (fields is null)
                fields = new List<PSNoteProperty>();

            if (defaults is null)
                defaults = new List<string>();

            //Create the object
            PSObject resultObject = new PSObject(objectToDecorate);
            resultObject.Members.Add(new PSNoteProperty("Status", objectToDecorate.Status));

            //Setup response specific display properties
            switch (objectToDecorate.Status)
            {
                case eidprovider.EIDResult.ResultStatus.initialized:
                    fields.Add(new PSNoteProperty("Id", objectToDecorate["id"].ToString()));
                    fields.Add(new PSNoteProperty("Code", "initialized"));
                    fields.Add(new PSNoteProperty("AutostartToken", objectToDecorate["extra"]["autostart_token"].ToString()));
                    fields.Add(new PSNoteProperty("AutostartUrl", objectToDecorate["extra"]["autostart_url"].ToString()));
                    defaults.InsertRange(0, new string[]{ "Status", "Code", "Id" });
                    break;
                case eidprovider.EIDResult.ResultStatus.error:
                    fields.Add(new PSNoteProperty("Code", objectToDecorate["code"].ToString()));
                    fields.Add(new PSNoteProperty("Description", objectToDecorate["description"].ToString()));
                    fields.Add(new PSNoteProperty("Details", KeyExist(objectToDecorate,"details") ? objectToDecorate["details"].ToString() : string.Empty));
                    defaults.InsertRange(0, new string[] { "Status", "Code" });
                    break;
                case eidprovider.EIDResult.ResultStatus.pending:
                    fields.Add(new PSNoteProperty("Code", objectToDecorate["code"].ToString()));
                    fields.Add(new PSNoteProperty("Description", objectToDecorate["description"].ToString()));
                    defaults.InsertRange(0, new string[] { "Status", "Code" });
                    break;
                case eidprovider.EIDResult.ResultStatus.ok:
                    fields.Add(new PSNoteProperty("Code", objectToDecorate["code"].ToString()));
                    fields.Add(new PSNoteProperty("Description", objectToDecorate["description"].ToString()));
                    defaults.InsertRange(0, new string[] { "Status", "Code" });

                    PSObject okExtraObject = new PSObject();
                    if (KeyExist(objectToDecorate, "extra"))
                        ((JObject)objectToDecorate["extra"]).Properties().Select(p => p.Name).ToList().ForEach((item) => { okExtraObject.Members.Add(new PSNoteProperty(item, objectToDecorate["extra"][item].ToString())); });
                    fields.Add(new PSNoteProperty("Extra", okExtraObject));
                    break;
                case eidprovider.EIDResult.ResultStatus.completed:
                    fields.Add(new PSNoteProperty("Id", objectToDecorate["user"]["id"].ToString()));
                    fields.Add(new PSNoteProperty("Code", "completed"));
                    fields.Add(new PSNoteProperty("Firstname", objectToDecorate["user"]["firstname"].ToString()));
                    fields.Add(new PSNoteProperty("Lastname", objectToDecorate["user"]["lastname"].ToString()));
                    fields.Add(new PSNoteProperty("Fullname", objectToDecorate["user"]["fullname"].ToString()));

                    PSObject psCustomObject = new PSObject();
                    if (KeyExist(objectToDecorate, "extra"))
                        ((JObject)objectToDecorate["extra"]).Properties().Select(p => p.Name).ToList().ForEach((item)=> { psCustomObject.Members.Add(new PSNoteProperty(item, objectToDecorate["extra"][item].ToString())); });
                    fields.Add(new PSNoteProperty("Extra", psCustomObject));

                    defaults.InsertRange(0, new string[] { "Status", "Id", "Firstname", "Lastname", "Fullname"});
                    break;
                default:
                    defaults.InsertRange(0, new string[] { "Status" });
                    break;
            }

            // Add the new properties to the real object
            fields.ForEach((p) => { resultObject.Members.Add(p); });

            //Add default display designator
            PSPropertySet defaltDisplayProperties = new PSPropertySet("DefaultDisplayPropertySet", defaults.ToArray());
            resultObject.Members.Add(new PSMemberSet("PSStandardMembers", new[] { defaltDisplayProperties }));

            return resultObject;
        }

    }
}
