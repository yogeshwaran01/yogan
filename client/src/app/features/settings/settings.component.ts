import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { DropdownModule } from 'primeng/dropdown';
import { FileUploadModule } from 'primeng/fileupload';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { TabViewModule } from 'primeng/tabview';
import { ToastModule } from 'primeng/toast';
import { AiService } from '../../core/services/ai.service';
import { StateService } from '../../core/services/state.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DropdownModule,
    InputTextModule,
    ButtonModule,
    FileUploadModule,
    InputSwitchModule,
    TabViewModule,
    FloatLabelModule,
    InputTextareaModule,
    ToastModule,
    DividerModule
  ],
  providers: [MessageService],
  template: `
    <div class="settings-container">
      <p-toast></p-toast>

      <section class="config-section">
        <h3>Configuration</h3>
        <div class="form-grid">
           <div class="field">
              <label for="ragToggle">RAG Mode</label>
              <div class="flex align-items-center gap-2">
                  <p-inputSwitch id="ragToggle" [(ngModel)]="isRagEnabled" (onChange)="updateRag()"></p-inputSwitch>
                  <small>{{ isRagEnabled ? 'Enabled (Context Retrieval)' : 'Disabled (Direct Chat)' }}</small>
              </div>
           </div>

            <div class="field">
              <label for="client">AI Client</label>
              <p-dropdown
                id="client"
                [options]="clients"
                [(ngModel)]="selectedClient"
                (onChange)="updateClient()"
                [style]="{'width':'100%'}"
              ></p-dropdown>
           </div>

           <div class="field">
              <label for="model">Model</label>
              <p-dropdown
                id="model"
                [options]="models"
                [(ngModel)]="selectedModel"
                (onChange)="updateModel()"
                [style]="{'width':'100%'}"
              ></p-dropdown>
           </div>

           <div class="field" *ngIf="isRagEnabled">
              <label for="store">Collection / Store</label>
              <p-dropdown
                [options]="stores"
                [(ngModel)]="selectedStore"
                (onChange)="updateStore()"
                [editable]="true"
                placeholder="Select or create a store"
                [style]="{'width':'100%'}"
              ></p-dropdown>
              <small class="text-secondary">Select an existing collection or type a new name to create one.</small>
           </div>
        </div>
      </section>

      <p-divider></p-divider>

      <section class="ingest-section">
        <h3>Add Context</h3>
        <p-tabView>
           <p-tabPanel header="Text">
              <div class="flex flex-column gap-2">
                 <textarea pInputTextarea [(ngModel)]="textContext" rows="5" placeholder="Paste text here..." class="w-full"></textarea>
                 <p-button label="Add to Memory" icon="pi pi-check" (onClick)="addTextContext()" [disabled]="!textContext" [loading]="isUploading"></p-button>
              </div>
           </p-tabPanel>

           <p-tabPanel header="File (PDF)">
              <div class="flex flex-column gap-2">
                 <p-fileUpload
                    mode="basic"
                    chooseLabel="Choose PDF"
                    accept=".pdf"
                    [maxFileSize]="10000000"
                    [auto]="false"
                    (onSelect)="onFileSelect($event)"
                    styleClass="w-full"
                 ></p-fileUpload>
                 <small *ngIf="selectedFile">{{ selectedFile.name }}</small>
                 <p-button label="Upload & Ingest" icon="pi pi-upload" severity="secondary" (onClick)="uploadFile()" [disabled]="!selectedFile" [loading]="isUploading"></p-button>
              </div>
           </p-tabPanel>
        </p-tabView>
      </section>
    </div>
  `,
  styles: [`
    .settings-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .config-section, .ingest-section {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .form-grid {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .field {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    h3 {
      margin: 0;
      font-size: 1.1rem;
      color: var(--text-color-secondary);
    }

    .w-full {
      width: 100%;
    }

    .flex-column {
      display: flex;
      flex-direction: column;
    }

    .gap-2 {
      gap: 0.5rem;
    }
  `]
})
export class SettingsComponent implements OnInit {
  models = ['llama3.1:8b', 'gemma:2b', 'mistral']; // Could fetch from API if available
  stores: string[] = [];
  clients = ['ollama', 'openai (Not Supported)', 'google', "ollamatool"]; // Example clients

  private aiService = inject(AiService);
  private stateService = inject(StateService);
  private messageService = inject(MessageService);

  selectedModel = this.stateService.selectedModel();
  selectedStore = this.stateService.selectedStore();
  isRagEnabled = this.stateService.isRagEnabled();
  selectedClient = this.stateService.selectedClient();

  textContext = '';
  selectedFile: File | null = null;
  isUploading = false;

  constructor() {
    // Sync local state if signal changes externaly (optional, but good practice in constructor effects)
  }

  ngOnInit() {
    this.loadStores();
  }

  loadStores() {
    this.aiService.getStores().subscribe(stores => {
      this.stores = stores;
      if (!this.selectedStore && this.stores.length > 0) {
        // Optionally default to first store if none selected
        // this.selectedStore = this.stores[0];
        // this.updateStore();
      }
    });
  }

  updateModel() {
    this.stateService.setModel(this.selectedModel);
  }

  updateClient() {
    this.stateService.setClient(this.selectedClient);
  }

  updateStore() {
    this.stateService.setStore(this.selectedStore);
  }

  updateRag() {
    this.stateService.setRagEnabled(this.isRagEnabled);
  }

  addTextContext() {
    if (!this.textContext) return;
    this.isUploading = true;

    const param = {
      Prompt: 'ingest',
      Model: this.selectedModel,
      StoreName: this.selectedStore,
      Context: this.textContext
    };

    this.aiService.addTextContext(param).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Text added to memory' });
        this.textContext = '';
        this.isUploading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to add context' });
        this.isUploading = false;
        console.error(err);
      }
    });
  }

  onFileSelect(event: any) {
    if (event.files && event.files.length > 0) {
      this.selectedFile = event.files[0];
    }
  }

  uploadFile() {
    if (!this.selectedFile) return;
    this.isUploading = true;

    const param = {
      Prompt: 'ingest',
      Model: this.selectedModel,
      StoreName: this.selectedStore,
    };

    this.aiService.addFileContext(param, this.selectedFile).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'File ingested successfully' });
        this.selectedFile = null;
        this.isUploading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to ingest file' });
        this.isUploading = false;
        console.error(err);
      }
    });
  }
}
