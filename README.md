![Icon](https://github.com/SeanFeldman/ServiceBus.CompressionPlugin/blob/master/images/project-icon.png)

### This is an add-in for [Microsoft.Azure.ServiceBus client](https://github.com/Azure/azure-service-bus-dotnet/) 

Allows sending and receiving compressed messages.

[![license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/SeanFeldman/ServiceBus.CompressionPlugin/blob/master/LICENSE)
[![develop](https://img.shields.io/appveyor/ci/seanfeldman/ServiceBus-CompressionPlugin/develop.svg?style=flat-square&branch=develop)](https://ci.appveyor.com/project/seanfeldman/ServiceBus-CompressionPlugin)
[![opened issues](https://img.shields.io/github/issues-raw/badges/shields/website.svg)](https://github.com/SeanFeldman/ServiceBus.CompressionPlugin/issues)

### NuGet package

[![NuGet Status](https://buildstats.info/nuget/ServiceBus.CompressionPlugin?includePreReleases=true)](https://www.nuget.org/packages/ServiceBus.CompressionPlugin/)

Available here http://nuget.org/packages/ServiceBus.CompressionPlugin

To Install from the NuGet Package Manager Console 
    
    PM> Install-Package ServiceBus.CompressionPlugin

## Examples

### Using default compression (GZip, body of at least 1500 bytes)

Configuration and registration

```c#
var sender = new MessageSender(connectionString, queueName);
sender.RegisterCompressionPlugin();
```        

Sending

```c#
var payload = new MyMessage { ... }; 
var serialized = JsonConvert.SerializeObject(payload);
var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
var message = new Message(payloadAsBytes);
```


Receiving

```c#
var receiver = new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete);
receiver.RegisterCompressionPlugin();
var msg = await receiver.ReceiveAsync().ConfigureAwait(false);
// msg will contain the original payload
```

### Overriding minimum body size

```c#
var sender = new MessageSender(connectionString, queueName);
sender.RegisterCompressionPlugin(1024); // compress messages using GZip with at least 1024 bytes
```        

### Custom compressions

Configuration and registration

```c#
var configuration = new CompressionConfiguration(compressionMethodName: "noop", compressor: bytes => Task.FromResult, decompressor: bytes => Task.FromResult, minimumSize: 1);

var sender = new MessageSender(connectionString, queueName);
sender.RegisterCompressionPlugin(configuration);
```        

Sending

```c#
var payload = new MyMessage { ... }; 
var serialized = JsonConvert.SerializeObject(payload);
var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
var message = new Message(payloadAsBytes);
```


Receiving

```c#
var receiver = new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete);
receiver.RegisterCompressionPlugin(configuration);
var msg = await receiver.ReceiveAsync().ConfigureAwait(false);
// msg will contain the original payload
```

### Transitioning to a different compression

To transition to a different compression, compression callbacks need to be updated.

1. Update `compressor` callback to register a new compression method name.
1. Update `decompressor` callback to handle the old and the new compression method names.
1. Drain all messages compressed using the old compression method name.
1. Update `decompresson` callback to handle the new compression method only.


## Who's trusting this add-in in production

<!--
![Microsoft](https://github.com/SeanFeldman/ServiceBus.CompressionPlugin/blob/develop/images/using/microsoft.png)
![Codit](https://github.com/SeanFeldman/ServiceBus.CompressionPlugin/blob/master/images/using/Codit.png)
-->

Proudly list your company here if use this add-in in production

## Icon

Created by Eucalyp from the Noun Project.