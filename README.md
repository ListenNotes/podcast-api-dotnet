# Podcast API .Net Library

[![NuGet](https://img.shields.io/nuget/v/PodcastAPI.svg)](https://www.nuget.org/packages/PodcastAPI/) ![GitHub Workflow](https://github.com/ListenNotes/podcast-api-dotnet/actions/workflows/dotnet.yml/badge.svg) 

The Podcast API .Net library provides convenient access to the [Listen Notes Podcast API](https://www.listennotes.com/api/) from
applications written in C#.

Simple and no-nonsense podcast search & directory API. Search the meta data of all podcasts and episodes by people, places, or topics. It's the same API that powers [the best podcast search engine Listen Notes](https://www.listennotes.com/).

If you have any questions, please contact [hello@listennotes.com](hello@listennotes.com?subject=Questions+about+the+dotnet+SDK+of+Listen+API)

## Installation

You can install our [NuGet PodcastAPI package](https://www.nuget.org/packages/PodcastAPI/).

Using the .NET Core command-line interface (CLI) tools:

```sh
dotnet add package PodcastAPI
```

Using the NuGet Command Line Interface (CLI):

```sh
nuget install PodcastAPI
```

Using the Package Manager Console:

```powershell
Install-Package PodcastAPI
```

From within Visual Studio:

1. Open the Solution Explorer.
2. Right-click on a project within your solution.
3. Click on *Manage NuGet Packages...*
4. Click on the *Browse* tab and search for "PodcastAPI".
5. Click on the PodcastAPI package, select the appropriate version in the
   right-tab and click *Install*.


### Requirements

- .Net 5.0+

## Usage

The library needs to be configured with your account's API key which is
available in your [Listen API Dashboard](https://www.listennotes.com/api/dashboard/#apps). Set `apiKey` to its
value:

```c#
using System;
using System.Collections.Generic;
using PodcastAPI;
using PodcastAPI.Exceptions;

namespace PodcastApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var apiKey = Environment.GetEnvironmentVariable("LISTEN_API_KEY");

            try
            {
                var client = new Client();

                var parameters = new Dictionary<string, string>();
                parameters.Add("q", "startup");
                parameters.Add("type", "episodes");

                var result = client.Search(parameters).Result;
                var jsonObject = result.ToJSON<dynamic>();
                Console.WriteLine($"Json Object: {jsonObject}");

                var freeQuota = result.GetFreeQuota();
                Console.WriteLine($"Free Quota: {freeQuota}");

                var usage = result.GetUsage();
                Console.WriteLine($"Usage: {usage}");

                var nextBillingDate = result.GetNextBillingDate();
                Console.WriteLine($"Next Billing Date: {nextBillingDate}");
            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine($"Authentication Issue: {ex.Message}");
            }
            catch (InvalidRequestException ex)
            {
                Console.WriteLine($"Invalid Request: {ex.Message}");
            }
            catch (RateLimitException ex)
            {
                Console.WriteLine($"Rate Limit: {ex.Message}");
            }
            catch (NotFoundException ex)
            {
                Console.WriteLine($"Not Found: {ex.Message}");
            }
            catch (ListenApiException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application Exception: {ex}");
            }
        }
    }
}
```

If `apiKey` is null or "", then we'll connect to a [mock server](https://www.listennotes.com/api/tutorials/#faq0) that returns fake data for testing purposes.


### Handling exceptions

Unsuccessful requests raise exceptions. The class of the exception will reflect
the sort of error that occurred.

| Exception Class  | Description |
| ------------- | ------------- |
|  AuthenticationException | wrong api key or your account is suspended  |
| ApiConnectionException  | fail to connect to API servers  |
| InvalidRequestException  | something wrong on your end (client side errors), e.g., missing required parameters  |
| RateLimitException  | you are using FREE plan and you exceed the quota limit  |
| NotFoundException  | endpoint not exist, or podcast / episode not exist  |
| ListenApiException  | something wrong on our end (unexpected server errors)  |

All exception classes can be found in [this folder](https://github.com/ListenNotes/podcast-api-dotnet/tree/main/src/PodcastAPI/Exceptions).

And you can see some sample code [here](https://github.com/ListenNotes/podcast-api-dotnet/blob/main/src/SampleApp/Sample.cs).

