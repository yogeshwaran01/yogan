import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface AIClientParam {
    Client?: string;
    Model?: string;
    Prompt: string;
    Context?: string;
    StoreName?: string;
    IsRagEnabled?: boolean;
}

export interface AIClientResponse {
    content: string;
    done: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class AiService {
    private apiUrl = 'http://localhost:5050/api/AI';

    constructor(private http: HttpClient) { }

    async *generateStream(param: AIClientParam): AsyncGenerator<string, void, unknown> {
        const response = await fetch(`${this.apiUrl}/generate`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(param)
        });

        if (!response.body) {
            throw new Error('ReadableStream not yet supported in this browser.');
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = '';

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            const chunk = decoder.decode(value, { stream: true });
            buffer += chunk;

            const lines = buffer.split('\n');
            buffer = lines.pop() || ''; // Keep the last incomplete line in the buffer

            for (const line of lines) {
                const trimmed = line.trim();
                if (!trimmed || trimmed === '[' || trimmed === ']' || trimmed === ',') continue;

                // Remove trailing comma if present (handled by 'continue' above if it's just a comma, but in JSON array stream it might be at the end of the object)
                let jsonStr = trimmed;
                if (jsonStr.endsWith(',')) {
                    jsonStr = jsonStr.slice(0, -1);
                }

                try {
                    const data = JSON.parse(jsonStr) as AIClientResponse;
                    if (data.content) {
                        yield data.content;
                    }
                } catch (e) {
                    console.error('Error parsing JSON chunk', e);
                }
            }
        }
    }

    addTextContext(param: AIClientParam): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/context/text`, param);
    }

    addFileContext(param: AIClientParam, file: File): Observable<void> {
        const formData = new FormData();
        formData.append('Client', param.Client || 'ollama');
        formData.append('Model', param.Model || 'llama3.1:8b');
        formData.append('Prompt', param.Prompt || 'ingest');
        formData.append('StoreName', param.StoreName || 'store');
        formData.append('FormFile', file);

        return this.http.post<void>(`${this.apiUrl}/context/file`, formData);
    }

    getStores(): Observable<string[]> {
        return this.http.get<string[]>(`${this.apiUrl}/stores`);
    }
}
