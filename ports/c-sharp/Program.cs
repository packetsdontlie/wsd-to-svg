using System.CommandLine;
using System.Text.Json;
using System.Text;

namespace WsdToSvg;

class Program
{
    private static readonly string ApiUrl = "https://www.websequencediagrams.com/index.php";
    private static readonly string[] ValidStyles = { "default", "napkin", "qsd", "rose", "modern-blue", "mscgen", "vs2010" };

    static async Task<int> Main(string[] args)
    {
        var wsdFileArgument = new Argument<FileInfo>(
            name: "wsd-file",
            description: "Path to .wsd file to render"
        );

        var outputOption = new Option<FileInfo?>(
            aliases: new[] { "-o", "--output" },
            description: "Output SVG file path (default: stdout)"
        );

        var styleOption = new Option<string>(
            aliases: new[] { "-s", "--style" },
            getDefaultValue: () => "default",
            description: "Diagram style (default: default)"
        );
        styleOption.AddValidator(result =>
        {
            var style = result.GetValueOrDefault<string>();
            if (style != null && !ValidStyles.Contains(style))
            {
                result.ErrorMessage = $"Style must be one of: {string.Join(", ", ValidStyles)}";
            }
        });

        var apiKeyOption = new Option<string?>(
            aliases: new[] { "--api-key" },
            description: "API key for premium features (optional)"
        );

        var timeoutOption = new Option<int>(
            aliases: new[] { "--timeout" },
            getDefaultValue: () => 30,
            description: "Request timeout in seconds (default: 30)"
        );

        var rootCommand = new RootCommand("Render Web Sequence Diagram (.wsd) files to SVG using websequencediagrams.com API")
        {
            wsdFileArgument,
            outputOption,
            styleOption,
            apiKeyOption,
            timeoutOption
        };

        rootCommand.SetHandler(async (wsdFile, output, style, apiKey, timeout) =>
        {
            try
            {
                await RenderWsdToSvg(wsdFile, output, style, apiKey, timeout);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, wsdFileArgument, outputOption, styleOption, apiKeyOption, timeoutOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task RenderWsdToSvg(
        FileInfo wsdFile,
        FileInfo? outputPath,
        string style,
        string? apiKey,
        int timeout)
    {
        // Read WSD file
        if (!wsdFile.Exists)
        {
            throw new FileNotFoundException($"File not found: {wsdFile.FullName}");
        }

        string wsdContent;
        try
        {
            wsdContent = await File.ReadAllTextAsync(wsdFile.FullName, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading WSD file: {ex.Message}", ex);
        }

        // Render to SVG
        byte[] svgContent;
        try
        {
            svgContent = await RenderWsdToSvgAsync(wsdContent, style, apiKey, timeout);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to call websequencediagrams.com API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Invalid JSON response from API: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error rendering diagram: {ex.Message}", ex);
        }

        // Save to file or output to stdout
        if (outputPath != null)
        {
            try
            {
                var directory = outputPath.Directory;
                if (directory != null && !directory.Exists)
                {
                    directory.Create();
                }

                await File.WriteAllBytesAsync(outputPath.FullName, svgContent);
                await Console.Error.WriteLineAsync($"SVG saved to: {outputPath.FullName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing output file: {ex.Message}", ex);
            }
        }
        else
        {
            // Output to stdout
            await Console.OpenStandardOutput().WriteAsync(svgContent);
        }
    }

    static async Task<byte[]> RenderWsdToSvgAsync(
        string wsdContent,
        string style,
        string? apiKey,
        int timeout)
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        // Prepare request data
        var formData = new List<KeyValuePair<string, string>>
        {
            new("message", wsdContent),
            new("style", style),
            new("format", "svg"),
            new("apiVersion", "1")
        };

        if (!string.IsNullOrEmpty(apiKey))
        {
            formData.Add(new KeyValuePair<string, string>("apikey", apiKey));
        }

        var formContent = new FormUrlEncodedContent(formData);

        // Make POST request to API
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(ApiUrl, formContent);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Failed to call websequencediagrams.com API: {ex.Message}", ex);
        }

        // Parse JSON response
        string responseContent;
        try
        {
            responseContent = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to read API response: {ex.Message}", ex);
        }

        JsonDocument? jsonDoc = null;
        try
        {
            jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            // Check for errors
            if (root.TryGetProperty("errors", out var errorsElement))
            {
                var errors = errorsElement.GetString() ?? "Unknown error";
                throw new Exception($"Diagram errors: {errors}");
            }

            // Get image path
            if (!root.TryGetProperty("img", out var imgElement))
            {
                throw new Exception("No image path in API response");
            }

            var imgPath = imgElement.GetString();
            if (string.IsNullOrEmpty(imgPath))
            {
                throw new Exception("No image path in API response");
            }

            // Construct full image URL
            string imageUrl;
            if (imgPath.StartsWith("?"))
            {
                imageUrl = $"https://www.websequencediagrams.com/{imgPath}";
            }
            else
            {
                imageUrl = $"https://www.websequencediagrams.com/?{imgPath}";
            }

            // Fetch the SVG
            HttpResponseMessage svgResponse;
            try
            {
                svgResponse = await httpClient.GetAsync(imageUrl);
                svgResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to download SVG: {ex.Message}", ex);
            }

            byte[] svgContent;
            try
            {
                svgContent = await svgResponse.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to read SVG content: {ex.Message}", ex);
            }

            return svgContent;
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Invalid JSON response from API: {ex.Message}", ex);
        }
        finally
        {
            jsonDoc?.Dispose();
        }
    }
}

