import { Injectable, signal, inject } from '@angular/core';
import { AiService } from './ai.service';

@Injectable({
  providedIn: 'root'
})
export class StateService {
  // Config state
  readonly selectedModel = signal<string>('gemini-2.5-flash-lite');
  readonly selectedStore = signal<string>('personal');
  readonly selectedClient = signal<string>('google');
  readonly systemPrompt = signal<string>('');

  // Dynamic lists from backend
  readonly clients = signal<string[]>([]);
  readonly modelsMap = signal<Record<string, string[]>>({});
  readonly availableModels = signal<string[]>([]);

  // UI state
  readonly isSidebarVisible = signal<boolean>(false);
  readonly isLoading = signal<boolean>(false);
  readonly isRagEnabled = signal<boolean>(true);

  private aiService = inject(AiService);

  constructor() {
    this.loadBackendConfig();
  }

  loadBackendConfig() {
    this.aiService.getConfig().subscribe({
      next: (config) => {
        this.clients.set(config.clients || []);
        this.modelsMap.set(config.models || {});
        
        // Use defaults from backend if present
        if (config.defaultClient) this.selectedClient.set(config.defaultClient);
        if (config.defaultModel) this.selectedModel.set(config.defaultModel);
        if (config.defaultStoreName) this.selectedStore.set(config.defaultStoreName);
        if (config.defaultSystemPrompt) this.systemPrompt.set(config.defaultSystemPrompt);

        this.updateAvailableModels(this.selectedClient());
      },
      error: (err) => {
        console.error('Failed to load backend config', err);
        // Sensible fallbacks if API is down
        this.clients.set(['google', 'ollama', 'ollamatool']);
        this.modelsMap.set({
          google: ['gemini-2.5-flash-lite', 'gemini-2.0-flash'],
          ollama: ['llama3.1:8b', 'gemma:2b'],
          ollamatool: ['llama3.1:8b', 'gemma:2b']
        });
        this.updateAvailableModels(this.selectedClient());
      }
    });
  }

  updateAvailableModels(client: string) {
    const models = this.modelsMap()[client] || [];
    this.availableModels.set(models);
    
    // Auto-select first model if the current one isn't available for the new client
    if (models.length > 0 && !models.includes(this.selectedModel())) {
      this.selectedModel.set(models[0]);
    }
  }

  setModel(model: string) {
    this.selectedModel.set(model);
  }

  setStore(store: string) {
    this.selectedStore.set(store);
  }

  setRagEnabled(enabled: boolean) {
    this.isRagEnabled.set(enabled);
  }

  toggleSidebar() {
    this.isSidebarVisible.update(v => !v);
  }

  setLoading(loading: boolean) {
    this.isLoading.set(loading);
  }

  setClient(client: string) {
    this.selectedClient.set(client);
    this.updateAvailableModels(client);
  }

  setSystemPrompt(prompt: string) {
    this.systemPrompt.set(prompt);
  }
}
