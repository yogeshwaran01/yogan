import { Injectable, signal } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class StateService {
    // Config state
    readonly selectedModel = signal<string>('llama3.1:8b');
    readonly selectedStore = signal<string>('store');

    // UI state
    readonly isSidebarVisible = signal<boolean>(false);
    readonly isLoading = signal<boolean>(false);
    readonly isRagEnabled = signal<boolean>(true);

    constructor() { }

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
}
