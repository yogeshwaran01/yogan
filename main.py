import requests
import os
import sys
import httpx
import json

API = "http://localhost:5050/api"

GENERATE_ENDPOINT = f"{API}/ai/generate"
CONTEXT_ENDPOINT = f"{API}/ai/context"


def update_memory(storeName, content):
    payload = {
        "storeName": storeName,
        "context": content
    }
    print(payload)
    response = requests.post(CONTEXT_ENDPOINT, json=payload)
    return response

def generate_prompt():
    prompt = input(">>> ")
    return prompt

def stream_response(prompt):
    payload = {
        "Prompt": prompt,
        "storeName": "personal",
    }
    with requests.post(GENERATE_ENDPOINT, json=payload, stream=True) as r:
        r.raise_for_status()

        for line in r.iter_lines(decode_unicode=True):
            if line:
                decoded_line = line.strip()
                if decoded_line == "[":
                    continue
                if decoded_line == "]":
                    break
                if decoded_line.endswith(","):
                    decoded_line = decoded_line[:-1]
                try:
                    data = json.loads(decoded_line)
                    if isinstance(data, list):
                        for item in data:
                            content = item.get("content", "")
                            print(content, end="", flush=True)
                    else:
                        chunk = data
                        content = chunk.get("content", "")
                        print(content, end="", flush=True)
                except json.JSONDecodeError:
                    continue

def load_context():
    context = {}
    context_dir = "context"
    for filename in os.listdir(context_dir):
        filepath = os.path.join(context_dir, filename)
        if os.path.isfile(filepath):
            with open(filepath, "r") as file:
                context[filename.split(".")[0]] = file.read()
    return context

def main():
    action = sys.argv[1]
    if (action == "load"):
        context = load_context()
        for storeName, content in context.items():
            update_memory(storeName, content)
    else:
        while True:
            prompt = generate_prompt()
            stream_response(prompt)
            print()

main()