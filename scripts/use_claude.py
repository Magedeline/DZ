"""Small wrapper showing how to use the CLAUDE_API_KEY from environment.

This script does not store any secret values. It reads CLAUDE_API_KEY from the
environment and demonstrates a POST request to the Anthropic/Claude endpoint.

Replace ENDPOINT and payload with the call you need. For local development,
create a .env file (ignored) and place CLAUDE_API_KEY=your_key there.

Requires: pip install python-dotenv requests
"""
import os
import sys
import json
import requests
from pathlib import Path
from dotenv import load_dotenv

# Load .env from the repo root
repo_root = Path(__file__).parent.parent
dotenv_path = repo_root / ".env"
load_dotenv(dotenv_path)

API_KEY = os.environ.get("CLAUDE_API_KEY")
if not API_KEY:
	print("Error: CLAUDE_API_KEY not set in environment.")
	print(f"Create a .env file at {dotenv_path} with CLAUDE_API_KEY=your_key")
	sys.exit(1)

# Example endpoint and headers. Adjust per your Claude API version and docs.
ENDPOINT = "https://api.anthropic.com/v1/complete"
HEADERS = {
	"Content-Type": "application/json",
	"x-api-key": API_KEY,
}

payload = {
	"model": "claude-2.1",  # adjust as needed
	"prompt": "Say hello from the repo.",
	"max_tokens": 50,
}

resp = requests.post(ENDPOINT, headers=HEADERS, json=payload)
if resp.status_code != 200:
	print(f"Request failed: {resp.status_code} -> {resp.text}")
	sys.exit(2)

print("Response:")
print(json.dumps(resp.json(), indent=2))
