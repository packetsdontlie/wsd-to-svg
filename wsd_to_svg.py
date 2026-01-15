#!/usr/bin/env python3
"""
WSD to SVG Renderer
Renders Web Sequence Diagram (.wsd) files to SVG using websequencediagrams.com API
"""

import argparse
import sys
import json
import requests
from pathlib import Path
from typing import Optional


def render_wsd_to_svg(
    wsd_content: str,
    output_path: Optional[Path] = None,
    style: str = "default",
    api_key: Optional[str] = None,
    timeout: int = 30
) -> bytes:
    """
    Render WSD content to SVG using websequencediagrams.com API.
    
    Args:
        wsd_content: The WSD diagram text content
        output_path: Optional path to save SVG file
        style: Diagram style (default, napkin, qsd, rose, modern-blue, etc.)
        api_key: Optional API key for premium features
        timeout: Request timeout in seconds
        
    Returns:
        SVG content as bytes
        
    Raises:
        requests.RequestException: If API request fails
        ValueError: If diagram has errors
    """
    # Prepare request data
    data = {
        "message": wsd_content,
        "style": style,
        "format": "svg",
        "apiVersion": "1"
    }
    
    if api_key:
        data["apikey"] = api_key
    
    # Make POST request to API
    api_url = "https://www.websequencediagrams.com/index.php"
    
    try:
        response = requests.post(
            api_url,
            data=data,
            timeout=timeout
        )
        response.raise_for_status()
    except requests.RequestException as e:
        raise requests.RequestException(f"Failed to call websequencediagrams.com API: {e}")
    
    # Parse JSON response
    try:
        result = response.json()
    except json.JSONDecodeError as e:
        raise ValueError(f"Invalid JSON response from API: {e}")
    
    # Check for errors
    if result.get("errors"):
        errors = result["errors"]
        raise ValueError(f"Diagram errors: {errors}")
    
    # Get image path
    img_path = result.get("img")
    if not img_path:
        raise ValueError("No image path in API response")
    
    # Construct full image URL
    # The img path might be like "?svg=xxxxxx" or just "?svg=xxxxxx"
    if img_path.startswith("?"):
        image_url = f"https://www.websequencediagrams.com/{img_path}"
    else:
        image_url = f"https://www.websequencediagrams.com/?{img_path}"
    
    # Fetch the SVG
    try:
        svg_response = requests.get(image_url, timeout=timeout)
        svg_response.raise_for_status()
    except requests.RequestException as e:
        raise requests.RequestException(f"Failed to download SVG: {e}")
    
    svg_content = svg_response.content
    
    # Save to file if output path provided
    if output_path:
        output_path = Path(output_path)
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_bytes(svg_content)
        print(f"SVG saved to: {output_path}", file=sys.stderr)
    
    return svg_content


def main():
    """Main CLI entry point."""
    parser = argparse.ArgumentParser(
        description="Render Web Sequence Diagram (.wsd) files to SVG using websequencediagrams.com API",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s diagram.wsd -o diagram.svg
  %(prog)s diagram.wsd -o diagram.svg --style modern-blue
  %(prog)s diagram.wsd -o diagram.svg --api-key YOUR_API_KEY
  %(prog)s diagram.wsd --style napkin  # Output to stdout
        """
    )
    
    parser.add_argument(
        "wsd_file",
        type=str,
        help="Path to .wsd file to render"
    )
    
    parser.add_argument(
        "-o", "--output",
        type=str,
        metavar="FILE",
        help="Output SVG file path (default: stdout)"
    )
    
    parser.add_argument(
        "-s", "--style",
        type=str,
        default="default",
        choices=["default", "napkin", "qsd", "rose", "modern-blue", "mscgen", "vs2010"],
        help="Diagram style (default: default)"
    )
    
    parser.add_argument(
        "--api-key",
        type=str,
        metavar="KEY",
        help="API key for premium features (optional)"
    )
    
    parser.add_argument(
        "--timeout",
        type=int,
        default=30,
        metavar="SECONDS",
        help="Request timeout in seconds (default: 30)"
    )
    
    args = parser.parse_args()
    
    # Read WSD file
    wsd_path = Path(args.wsd_file)
    if not wsd_path.exists():
        print(f"Error: File not found: {wsd_path}", file=sys.stderr)
        sys.exit(1)
    
    try:
        wsd_content = wsd_path.read_text(encoding="utf-8")
    except Exception as e:
        print(f"Error reading WSD file: {e}", file=sys.stderr)
        sys.exit(1)
    
    # Render to SVG
    try:
        output_path = Path(args.output) if args.output else None
        svg_content = render_wsd_to_svg(
            wsd_content=wsd_content,
            output_path=output_path,
            style=args.style,
            api_key=args.api_key,
            timeout=args.timeout
        )
        
        # Output to stdout if no output file specified
        if not args.output:
            sys.stdout.buffer.write(svg_content)
        
    except requests.RequestException as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)
    except ValueError as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()

