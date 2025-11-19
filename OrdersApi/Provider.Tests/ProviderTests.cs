// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Text.Json;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.Hosting;
// using PactNet;
// using PactNet.Infrastructure.Outputters;
// using PactNet.Output.Xunit;
// using PactNet.Verifier;
// using Provider.Orders;
// using Xunit;
// using Xunit.Abstractions;

// namespace Provider.Tests
// {
//     public class ProviderTests : IDisposable
//     {
//         private static readonly Uri ProviderUri = new("http://localhost:6000");

//         private static readonly JsonSerializerOptions Options = new()
//         {
//             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//             PropertyNameCaseInsensitive = true
//         };

//         private readonly IHost server;
//         private readonly PactVerifier verifier;

//         public ProviderTests(ITestOutputHelper output)
//         {
//             this.server = Host.CreateDefaultBuilder()
//                               .ConfigureWebHostDefaults(webBuilder =>
//                               {
//                                   webBuilder.UseUrls(ProviderUri.ToString());
//                                   webBuilder.UseStartup<TestStartup>();
//                               })
//                               .Build();

//             this.server.Start();
            
//             this.verifier = new PactVerifier("Orders API", new PactVerifierConfig
//             {
//                 LogLevel = PactLogLevel.Debug,
//                 Outputters = new List<IOutput>
//                 {
//                     new XunitOutput(output)
//                 }
//             });
//         }

//         public void Dispose()
//         {
//             this.server.Dispose();
//             this.verifier.Dispose();
//         }

//         [Fact]
//         public void VerifyWithPactBroker()
//         {
//             this.verifier
//                 .WithHttpEndpoint(ProviderUri)
//                 .WithMessages(scenarios =>
                
//                 {
//                     scenarios.Add("an event indicating that an order has been created", () => new OrderCreatedEvent(1));
//                 }, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
//                     .WithPactBrokerSource(new Uri("http://localhost:9292"), options =>
//                 {
//                     options.ConsumerVersionSelectors(new[]
//                     {
//                         new ConsumerVersionSelector { Latest = true }
//                     });
//                 })
//                 .WithProviderStateUrl(new Uri(ProviderUri, "/provider-states"))
//                 .Verify();
//         }
//     }
// }



using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Provider.Orders;
using Xunit;
using Xunit.Abstractions;

namespace Provider.Tests
{
    public class ProviderTests : IDisposable
    {
        private static readonly Uri ProviderUri = new("http://localhost:6000");

        // Shared JSON options for both HTTP and messaging
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHost server;
        private readonly PactVerifier verifier;

        public ProviderTests(ITestOutputHelper output)
        {
            this.server = Host.CreateDefaultBuilder()
                              .ConfigureWebHostDefaults(webBuilder =>
                              {
                                  webBuilder.UseUrls(ProviderUri.ToString());
                                  webBuilder.UseStartup<TestStartup>();
                              })
                              .Build();

            this.server.Start();

            this.verifier = new PactVerifier("Orders API", new PactVerifierConfig
            {
                LogLevel = PactLogLevel.Debug,
                Outputters = new List<IOutput>
                {
                    new XunitOutput(output)
                }
            });
        }

        public void Dispose()
        {
            this.server.Dispose();
            this.verifier.Dispose();
        }

        [Fact]
        public void VerifyWithPactBroker()
        {
            this.verifier
                .WithHttpEndpoint(ProviderUri)
                .WithMessages(scenarios =>
                {
                    scenarios.Add("an event indicating that an order has been created", () => new OrderCreatedEvent(1));
                }, Options)
                .WithPactBrokerSource(new Uri("http://localhost:9292"), options =>
                {
                    options.ConsumerVersionSelectors(new[]
                    {
                        new ConsumerVersionSelector { Latest = true }
                    });
                    options.PublishResults("1.0.0");
                })
                
                .WithProviderStateUrl(new Uri(ProviderUri, "/provider-states"))
                .Verify();
        }
    }
}