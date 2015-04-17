open Microsoft.ServiceBus.Messaging;
open System.Threading.Tasks;
open System.Text;
open System;

let sbNamespace = "your service bus namespace"
let saName = "your service bus SAS name" 
let saKey = "your service bus SAS key"

let eventHubPath = "the name of your event hub"

let storageName = "your storage account for checkpointing"
let storageKey = "your storage account key"

let eventHubCS = sprintf "Endpoint=sb://%s.servicebus.windows.net/;SharedAccessKeyName=%s;SharedAccessKey=%s" sbNamespace saName saKey
let checkpointCS = sprintf "DefaultEndpointsProtocol=http;AccountName=%s;AccountKey=%s;" storageName storageKey

let hostName = Guid.NewGuid().ToString()
let consumgerGroup = EventHubConsumerGroup.DefaultGroupName

let emptyTask() = Task.FromResult(true) :> Task

type MyConsumer() =
    interface IEventProcessor with
        member x.OpenAsync (context) = 
            printfn "open partition %s" context.Lease.PartitionId
            emptyTask()

        member x.CloseAsync (context, reason) = 
            printfn "close partition %s" context.Lease.PartitionId
            printfn "reason %A" reason
            emptyTask()

        member x.ProcessEventsAsync(context, events) = 
            printfn "processing partition %s" context.Lease.PartitionId
            events 
                |> Seq.map (fun e -> e.GetBytes() |> Encoding.ASCII.GetString )
                |> Seq.iter (printfn "%s")
            emptyTask()

[<EntryPoint>]
let main argv = 
    printfn "consume events"

    let options = EventProcessorOptions()
    options.ExceptionReceived.AddHandler(fun s a -> printfn "boom!" )

    let host = EventProcessorHost (hostName, eventHubPath, consumgerGroup, eventHubCS, checkpointCS)
    host.RegisterEventProcessorAsync<MyConsumer>(options) |> ignore
    printfn "finished registering"
    Console.ReadKey() |> ignore
    0 // return an integer exit code
