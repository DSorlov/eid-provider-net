## Examples

Very simple examples. All methods are available in Async versions also and supporting IProgress for long running operations.

### Simple C# example for frejaeid
This is a very simple example of calling authentication via frejaeid for the ssn 200101011212 and when final results are in dump them out on the console.
```csharp
EIDClientInitializationData config = new frejaeid.InitializationData(EIDEnvironment.Testing);
EIDClient client = new frejaeid.Client((frejaeid.InitializationData)config);
EIDResult = client.AuthRequest("200101011212");
Console.WriteLine(EIDResult.ToString());
```

### Simple C# example for bankid
This is a very simple example of calling authentication via bankid for the ssn 200101011212 and when final results are in dump them out on the console.
```csharp
EIDClientInitializationData config = new bankid.InitializationData(EIDEnvironment.Testing);
EIDClient client = new bankid.Client((bankid.InitializationData)config);
EIDResult = client.AuthRequest("200101011212");
Console.WriteLine(EIDResult.ToString());
```

### Simple C# example for frejaeid with event callback
This is a very simple example of calling authentication via frejaeid for the ssn 200101011212 and when final results are in dump them out on the console and also listen to events while it is processing
```csharp
EIDClientInitializationData config = new frejaeid.InitializationData(EIDEnvironment.Testing);
EIDClient client = new frejaeid.Client((frejaeid.InitializationData)config);

//Attach a event listener
client.RequestEvent = (e) => { Console.WriteLine(e.EIDResult.ToString(); }; 

EIDResult = client.AuthRequest("200101011212");
Console.WriteLine(EIDResult.ToString());
```

### Simple C# example configuring options in config
This is a very simple example of calling authentication via frejaeid for the ssn 200101011212 and when final results are in dump them out on the console.
```csharp
EIDClientInitializationData config = new bankid.InitializationData(EIDEnvironment.Testing);
config["client_cert"] = YourX509Certificate2();

EIDClient client = new bankid.Client((bankid.InitializationData)config);
EIDResult = client.AuthRequest("200101011212");
Console.WriteLine(EIDResult.ToString());
```

