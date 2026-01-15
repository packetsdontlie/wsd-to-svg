# WSD to SVG Renderer (C# Port)

A C#/.NET port of the WSD to SVG renderer utility. Renders Web Sequence Diagram (`.wsd`) files to SVG using the websequencediagrams.com API.

## Requirements

- .NET 10.0 SDK or later

## Building

```bash
dotnet build
```

## Testing

To test the application, create a simple WSD file and run:

```bash
# Create a test file
cat > test.wsd << 'EOF'
Alice->Bob: Hello Bob, how are you?
Bob->Alice: Great!
EOF

# Render it
dotnet run -- test.wsd -o test.svg

# Verify the output
ls -lh test.svg
```

## Usage

### Basic Usage

```bash
# Render WSD file to SVG
dotnet run -- diagram.wsd -o diagram.svg

# Output to stdout (for piping)
dotnet run -- diagram.wsd > diagram.svg
```

### Options

```bash
# Use different diagram style
dotnet run -- diagram.wsd -o diagram.svg --style modern-blue

# Available styles: default, napkin, qsd, rose, modern-blue, mscgen, vs2010

# With API key (for premium features)
dotnet run -- diagram.wsd -o diagram.svg --api-key YOUR_API_KEY

# Custom timeout
dotnet run -- diagram.wsd -o diagram.svg --timeout 60
```

### Publish as Standalone Executable

```bash
# For your current platform
dotnet publish -c Release

# For specific platform (e.g., macOS x64)
dotnet publish -c Release -r osx-x64 --self-contained

# For Linux x64
dotnet publish -c Release -r linux-x64 --self-contained

# For Windows x64
dotnet publish -c Release -r win-x64 --self-contained
```

After publishing, the executable will be in `bin/Release/net10.0/[runtime]/publish/wsd-to-svg` (or `wsd-to-svg.exe` on Windows).

## Examples

### Render Sequence Diagram

```bash
dotnet run -- diagram.wsd -o diagram.svg --style modern-blue
```

### Batch Render All WSD Files

```bash
for wsd in *.wsd; do
  svg="${wsd%.wsd}.svg"
  dotnet run -- "$wsd" -o "$svg"
done
```

## How It Works

1. Reads the `.wsd` file content
2. POSTs to `https://www.websequencediagrams.com/index.php` with the diagram text
3. Receives JSON response with image path
4. Downloads the SVG from the returned URL
5. Saves to output file (or stdout)

## API Details

The utility uses the websequencediagrams.com public API:
- Endpoint: `https://www.websequencediagrams.com/index.php`
- Method: POST (form-encoded)
- Parameters:
  - `message`: WSD diagram text
  - `style`: Visual style
  - `format`: "svg" (or "png", "pdf")
  - `apiVersion`: "1"
  - `apikey`: Optional premium API key

## Limitations

- Free tier has rate limits
- Generated SVG URLs may expire after some time
- Some styles may require premium subscription

## Differences from Python Version

- Uses `System.CommandLine` for CLI argument parsing
- Uses `HttpClient` for HTTP requests
- Uses `System.Text.Json` for JSON parsing
- Requires .NET 10.0 or later

## License

MIT

