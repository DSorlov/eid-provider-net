## Methods

This is the general description of the methods availiable to you.
I'm still struggling a bit to make this really readable in a good way, so perhaps easier to check with the [examples](examples.md).
This as close a replication of the original library as possible, it is using language specific features and sports some nice taskbased interfaces.

### constructor(initialization data)

Configures the module according to the object sent in.
Example configs can be obtained by accessing the `settings` properties of each module.

>**Inputs**

object(mandatory): A object containing configuration.

>**Outputs**

None

### PollAuthStatus(string) or PollSignStatus(string)

>**Inputs**

string(mandatory): A string containing the id of the authentication or signing you wish to check

>**Outputs**

A status object as one of the below:

```javascript
{
    status: 'error' or 'pending',
    code: string,
    description: string,
    [details: string]
}
```

The description field is a user friendly error message in english. The details is a optional field that if it exists contains more information about the error. More generic error types often have a details field.

| Status | Possible Codes |
| --- | --- |
| error | system_error<br/>request_id_invalid<br/>api_error<br/>expired_transaction<br/>cancelled_by_user<br/>cancelled_by_idp | 
| pending | pending_notdelivered<br/>pending_user_in_app<br/>pending_delivered | 

```javascript
{
    status: 'completed',
    user: {
        firstname: string,
        lastname: string,
        fullname: string,
        ssn: string
    },
    extra: {..}
}
```

When the status is completed extra information may be in the extra block depending on which module you are using.

### Task AuthRequest(string, ProcessIProgress<EIDResult>, CancellationToken) or Task SignRequest(string, string, ProcessIProgress<EIDResult>, CancellationToken)

>**Inputs**

string: this is the ssn most probably put could be a object with special properties for that module.
ProcessIProgress<EIDResult>: A ProcessIProgress to report back events and updates as they unfold
CancellationToken: Standard CancellationToken to cancel the running task

>**Outputs**

Same as PollAuthStatus(string) or PollSignStatus(string) but wrapped in a awaitable Task

### InitAuthRequest(string) or InitSignRequest(string,string)

>**Inputs**

string(mandatory): this is the ssn most probably put could be a object with special properties for that module.
string(only for signing): this is the text most probably put could be a object with special properties for that module.

>**Outputs**

A status object as one of the below:

```javascript
{
    status: 'error',
    code: string,
    description: string,
    [details: string]
}
```

The description field is a user friendly error message in english. The details is a optional field that if it exists contains more information about the error. More generic error types often have a details field.

| Status | Possible Codes |
| --- | --- |
| error | system_error<br/>already_in_progress<br/>request_ssn_invalid<br/>request_text_invalid<br/>api_error | 

```javascript
{
    status: 'initialized',
    id: string,
    description: string,
    extra: {..}
}
```

When the status is completed extra information may be in the extra block depending on which module you are using.

### CancelAuthRequest(string) or CancelSignRequest(string)

>**Inputs**

string(mandatory): A string containing the id of the authentication or signing you wish to cancel

>**Outputs**

```javascript
{
    status: 'cancelled',
    id: string,
    description: string,
    extra: {..}
}
