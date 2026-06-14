import requests
import os
import sys
import json
import uuid

# Configuration through environment variables
API_URL = os.getenv("YOGAN_API_URL", "http://localhost:5050/api")
DEFAULT_CLIENT = os.getenv("YOGAN_CLIENT", "google")
DEFAULT_MODEL = os.getenv("YOGAN_MODEL", "gemini-2.5-flash-lite")
DEFAULT_STORE = os.getenv("YOGAN_STORE", "personal")

GENERATE_ENDPOINT = f"{API_URL}/AI/generate"
CONTEXT_ENDPOINT = f"{API_URL}/AI/context/text"


def update_memory(storeName, content):
    payload = {
        "client": DEFAULT_CLIENT,
        "model": DEFAULT_MODEL,
        "storeName": storeName,
        "context": content,
        "prompt": "ingest"
    }
    print(f"Ingesting context into store '{storeName}'...")
    response = requests.post(CONTEXT_ENDPOINT, json=payload)
    if response.status_code == 200:
        print("Success.")
    else:
        print(f"Error {response.status_code}: {response.text}")
    return response

def generate_prompt():
    try:
        prompt = input(">>> ")
        return prompt
    except (KeyboardInterrupt, EOFError):
        print("\nExiting...")
        sys.exit(0)

def stream_response(prompt, conversation_id):
    payload = {
        "Prompt": prompt,
        "storeName": DEFAULT_STORE,
        "Client": DEFAULT_CLIENT,
        "Model": DEFAULT_MODEL,
        "ConversationId": conversation_id
    }
    
    try:
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
    except requests.exceptions.RequestException as e:
        print(f"\nAPI Request failed: {e}")

def load_context():
    context = {}
    context_dir = "context"
    if not os.path.exists(context_dir):
        print(f"Directory '{context_dir}' not found.")
        return context
        
    for filename in os.listdir(context_dir):
        filepath = os.path.join(context_dir, filename)
        if os.path.isfile(filepath):
            with open(filepath, "r", encoding="utf-8") as file:
                context[filename.split(".")[0]] = file.read()
    return context

def main():
    action = sys.argv[1] if len(sys.argv) > 1 else "chat"
    
    if action == "load":
        context = load_context()
        for storeName, content in context.items():
            update_memory(storeName, content)
    else:
        conversation_id = str(uuid.uuid4())
        print(f"Started session {conversation_id} (Client: {DEFAULT_CLIENT}, Model: {DEFAULT_MODEL}, Store: {DEFAULT_STORE})")
        while True:
            prompt = generate_prompt()
            if prompt.strip() == "":
                continue
            if prompt.lower() in ["exit", "quit"]:
                break
            stream_response(prompt, conversation_id)
            print()

if __name__ == "__main__":
    main()