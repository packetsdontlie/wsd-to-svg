# WSD to SVG Renderer

A utility to render Web Sequence Diagram (`.wsd`) files to SVG using the websequencediagrams.com API.

## Installation

### Using pip (Recommended)

Install from the project directory:

```bash
pip install .
```

Or install in editable/development mode:

```bash
pip install -e .
```

After installation, you can use the `wsd-to-svg` command directly:

```bash
wsd-to-svg diagram.wsd -o diagram.svg
```

**Note:** If the `wsd-to-svg` command is not found after installation, you may need to add the Python user bin directory to your PATH, or use:

```bash
python3 -m wsd_to_svg diagram.wsd -o diagram.svg
```

### Manual Installation

If you prefer to run the script directly:

```bash
pip install -r requirements.txt
python3 wsd_to_svg.py diagram.wsd -o diagram.svg
```

## Usage

### Basic Usage

```bash
# Render WSD file to SVG (after pip install)
wsd-to-svg _docs/artifact-15--websequence-diagram.wsd -o _docs/artifact-15--websequence-diagram.svg

# Or using Python directly (if not installed)
python3 wsd_to_svg.py _docs/artifact-15--websequence-diagram.wsd -o _docs/artifact-15--websequence-diagram.svg

# Output to stdout (for piping)
wsd-to-svg _docs/artifact-15--websequence-diagram.wsd > diagram.svg
```

### Options

```bash
# Use different diagram style
wsd-to-svg diagram.wsd -o diagram.svg --style modern-blue

# Available styles: default, napkin, qsd, rose, modern-blue, mscgen, vs2010

# With API key (for premium features)
wsd-to-svg diagram.wsd -o diagram.svg --api-key YOUR_API_KEY

# Custom timeout
wsd-to-svg diagram.wsd -o diagram.svg --timeout 60
```

## Examples

### Render fvMirror Sequence Diagram

```bash
cd /Users/brucekiefer/projects/fvmirror
wsd-to-svg _docs/artifact-15--websequence-diagram.wsd \
  -o _docs/artifact-15--websequence-diagram.svg \
  --style modern-blue
```

### Batch Render All WSD Files

```bash
for wsd in _docs/*.wsd; do
  svg="${wsd%.wsd}.svg"
  wsd-to-svg "$wsd" -o "$svg"
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

## Other Implementations

This project also includes ports to other languages:

- **C#/.NET**: See [ports/c-sharp/README.md](ports/c-sharp/README.md)

## License

MIT
