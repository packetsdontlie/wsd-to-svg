# WSD to SVG Renderer

A utility to render Web Sequence Diagram (`.wsd`) files to SVG using the websequencediagrams.com API.

## Installation

```bash
pip install -r requirements.txt
```

## Usage

### Basic Usage

```bash
# Render WSD file to SVG
python3 wsd-to-svg.py _docs/artifact-15--websequence-diagram.wsd -o _docs/artifact-15--websequence-diagram.svg

# Output to stdout (for piping)
python3 wsd-to-svg.py _docs/artifact-15--websequence-diagram.wsd > diagram.svg
```

### Options

```bash
# Use different diagram style
python3 wsd-to-svg.py diagram.wsd -o diagram.svg --style modern-blue

# Available styles: default, napkin, qsd, rose, modern-blue, mscgen, vs2010

# With API key (for premium features)
python3 wsd-to-svg.py diagram.wsd -o diagram.svg --api-key YOUR_API_KEY

# Custom timeout
python3 wsd-to-svg.py diagram.wsd -o diagram.svg --timeout 60
```

## Examples

### Render fvMirror Sequence Diagram

```bash
cd /Users/brucekiefer/projects/fvmirror
python3 _tools/wsd-to-svg.py _docs/artifact-15--websequence-diagram.wsd \
  -o _docs/artifact-15--websequence-diagram.svg \
  --style modern-blue
```

### Batch Render All WSD Files

```bash
for wsd in _docs/*.wsd; do
  svg="${wsd%.wsd}.svg"
  python3 _tools/wsd-to-svg.py "$wsd" -o "$svg"
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

## License

MIT
