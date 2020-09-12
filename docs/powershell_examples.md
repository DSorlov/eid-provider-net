## Powershell Examples

Very simple examples.

### Simple power example for frejaeid
This is a very simple example of calling authentication via frejaeid for the ssn 200101011212 and print the results console.

```powershell
    $config = Get-EIDConfig frejaeid -Enviroment Testing
    Request-EIDOperation $config -Type auth -Id 200101011212 -Wait
```

### Simple powershell example for bankid
This is a very simple example of calling authentication via frejaeid for the ssn 200101011212 and print the results console.

```powershell
    $config = Get-EIDConfig bankid -Enviroment Testing
    Request-EIDOperation $config -Type auth -Id 200101011212 -Wait
```

### Add an organizational id to a existing eid via freja eid orgid
This is a very simple example of calling authentication via frejaeid for the ssn 200101011212 and print the results console.

```powershell
    $config = Get-EIDConfig frejaeid -Enviroment Testing
    Start-EIDRequest $s -Type orgid -Id 200101011212 -Title "Corp Id" -Attribute "Employee #" -Value "123456" -Wait
```
