## Freja eID (frejaeid)

### Description
This module works directly with the Freja eID REST API and Freja eID Org ID REST API.
It is supplied with working testing credentials and basic production details.

This module exposes extra functions also (and async variants also):
- **AddOrgIdRequest(string id, string title, string attribute, string value)** Creates a new orgidadd and returns after result is received
- **InitAddOrgIdRequest(string id, string title, string attribute, string value)** Initiates orgidadd and returns a initialization object
- **PollAddOrgIdResult(string id)** Checks the status of a orgidadd operation
- **CancelAddOrgIdRequest(string id)** Cancels a pending orgidadd
- **DeleteOrgId(string id)** Removes a orgid from an existing eid
- **CreateCustomIdentifier(string id, string customid)** Creates a custom identifier for a specific eid
- **DeleteCustomIdentifier(string customid)** Removes a custom identifier for a specific eid

### Inputs and outputs

**Extra fields on completion**
* `autostart_token` the token used for autostart
* `autostart_url` code for invoking authorization

### Default Configuration
attribute_list is a comma separated list of EMAIL_ADDRESS,RELYING_PARTY_USER_ID,BASIC_USER_INFO,SSN,ADDRESSES,DATE_OF_BIRTH,ALL_EMAIL_ADDRESSES
minimum_level is one of BASIC,EXTENDED,PLUS
id_type is one of SSN,EMAIL,PHONE
>**Default production configuration (settings.production)**
```
endpoint:  'https://services.prod.frejaeid.com',
client_cert:  '',
ca_cert:  'builtin://certs/frejaeid_prod.ca',
jwt_cert: {
    'aRw9OLn2BhM7hxoc458cIXHfezw': 'builtin://certs/frejaeid_prod_aRw9OLn2BhM7hxoc458cIXHfezw.jwt'),
    'onjnxVgI3oUzWQMLciD7sQZ4mqM': 'builtin://certs/frejaeid_prod_onjnxVgI3oUzWQMLciD7sQZ4mqM.jwt')
},
minimum_level:  'EXTENDED',
password:  '',
default_country: 'SE',
id_type: 'SSN',
attribute_list: 'EMAIL_ADDRESS,RELYING_PARTY_USER_ID,BASIC_USER_INFO'        
```
>**Default testing configuration (settings.testing)**
```
endpoint:  'https://services.test.frejaeid.com',
client_cert:  'builtin://certs/frejaeid_test.ca',
ca_cert:  'builtin://certs/frejaeid_test.pfx',
jwt_cert:  {
    '2LQIrINOzwWAVDhoYybqUcXXmVs': 'builtin://certs/frejaeid_test_2LQIrINOzwWAVDhoYybqUcXXmVs.jwt'),
    'HwMHK_gb3_iuNF1advMtlG0-fUs': 'builtin://certs/frejaeid_test_HwMHK_gb3_iuNF1advMtlG0-fUs.jwt')
},
minimum_evel:  'EXTENDED',
password:  'test',
default_country: 'SE',
id_type: 'SSN',
attribute_list: 'EMAIL_ADDRESS,RELYING_PARTY_USER_ID,BASIC_USER_INFO'        
```