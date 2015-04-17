open Microsoft.ServiceBus.Messaging;
open System.Threading.Tasks;
open System.Text;
open System;
open System.Reactive;

let sbNamespace = "your service bus namespace"
let saName = "your service bus SAS name" 
let saKey = "your service bus SAS key"

let eventHubPath = "the name of your event hub"
let eventHubCS = sprintf "Endpoint=sb://%s.servicebus.windows.net/;SharedAccessKeyName=%s;SharedAccessKey=%s" sbNamespace saName saKey

[<EntryPoint>]
let main argv = 
    printfn "send events"

    let client = EventHubClient.CreateFromConnectionString(eventHubCS, eventHubPath)

    let send = fun x ->
        let bytes = (sprintf "some stuff %d" x) |> Encoding.ASCII.GetBytes
        let event = new EventData(bytes)
        client.Send(event) |> ignore
        printfn "sent %i" x

    let timer = Linq.Observable.Generate( 
                    0,
                    (fun _ -> true),
                    (fun x -> x + 1),
                    (fun x -> x),
                    (fun x -> TimeSpan.FromSeconds(1.0)))
    
    let subscription = timer.Subscribe(send)

    Console.ReadKey() |> ignore
    0 // return an integer exit code