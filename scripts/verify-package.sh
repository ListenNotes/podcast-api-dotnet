#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

dotnet_sdk_verify_dir=$(mktemp -d)
trap 'rm -rf "$dotnet_sdk_verify_dir"' EXIT

dotnet pack src/PodcastAPI --no-restore -c Release -o "$dotnet_sdk_verify_dir/packages"
packages=("$dotnet_sdk_verify_dir"/packages/PodcastAPI.*.nupkg)
if [ "${#packages[@]}" -ne 1 ] || [ ! -f "${packages[0]}" ]; then
  echo 'Expected one PodcastAPI NuGet package' >&2
  exit 1
fi
package_name=$(basename "${packages[0]}")
package_version=${package_name#PodcastAPI.}
package_version=${package_version%.nupkg}
mkdir "$dotnet_sdk_verify_dir/consumer"
cat > "$dotnet_sdk_verify_dir/consumer/Consumer.csproj" <<XML
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="PodcastAPI" Version="$package_version" /></ItemGroup>
</Project>
XML
cat > "$dotnet_sdk_verify_dir/consumer/NuGet.Config" <<XML
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$dotnet_sdk_verify_dir/packages" />
    <add key="nuget" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local"><package pattern="PodcastAPI" /></packageSource>
    <packageSource key="nuget"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
XML
# Compile every generated README example against the packed binary, not a project reference.
cp src/SampleApp/GeneratedExamples.cs "$dotnet_sdk_verify_dir/consumer/GeneratedExamples.cs"
cat > "$dotnet_sdk_verify_dir/consumer/Program.cs" <<'CS'
using System.Net;
using PodcastAPI;

using var http = new HttpClient(new OfflineHandler());
using var client = new Client(httpClient: http);
var response = await client.UpdatePlaylistItemNotes(new Dictionary<string, string>
{
    ["id"] = "playlist/encoded", ["item_id"] = "23", ["notes"] = "",
});
if (response.StatusCode != HttpStatusCode.OK || (int)response.ToJSON<dynamic>()!.id != 23)
    throw new Exception("Packaged response did not deserialize");
var deleted = await client.DeletePlaylist(new Dictionary<string, string> { ["id"] = "playlist/encoded" });
if (deleted.StatusCode != HttpStatusCode.OK || !(bool)deleted.ToJSON<dynamic>()!.deleted ||
    (string)deleted.ToJSON<dynamic>()!.id != "playlist/encoded")
    throw new Exception("Packaged deletion response did not deserialize");
Console.WriteLine("Packaged SDK and all README examples verified without API requests.");

sealed class OfflineHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        if (request.Method == HttpMethod.Delete)
        {
            if (request.RequestUri!.AbsolutePath != "/api/v2/playlists/playlist%2Fencoded" ||
                request.RequestUri.Query != "" || request.Content is not null ||
                request.Headers.Contains("X-ListenAPI-Key"))
                throw new Exception("Packaged deletion request contract mismatch");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"playlist/encoded\",\"deleted\":true}"),
            };
        }
        if (request.Method != HttpMethod.Put ||
            request.RequestUri!.AbsolutePath != "/api/v2/playlists/playlist%2Fencoded/items/23" ||
            request.Headers.Contains("X-ListenAPI-Key") ||
            await request.Content!.ReadAsStringAsync(token) != "notes=")
            throw new Exception("Packaged request contract mismatch");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":23}") };
    }
}
CS
# A fresh cache and source mapping guarantee PodcastAPI is the newly packed local artifact.
NUGET_PACKAGES="$dotnet_sdk_verify_dir/cache" dotnet restore "$dotnet_sdk_verify_dir/consumer/Consumer.csproj"
NUGET_PACKAGES="$dotnet_sdk_verify_dir/cache" dotnet run --project "$dotnet_sdk_verify_dir/consumer/Consumer.csproj" --no-restore -c Release
