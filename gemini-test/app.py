from google import genai
import os

# Best practice: Set your key as an environment variable
# Or replace os.environ.get(...) with "YOUR_ACTUAL_API_KEY" for a quick test
client = genai.Client(api_key="AQ.Ab8RN6LwfjipcUwV3EKFaiawpxsQR50zV5hU0xLtYbUYT6l9nw")

response = client.models.generate_content(
    model="gemini-3-pro-preview", 
    contents="Write a 300-word blog post about the future of AI in 2026."
)

print(response.text)