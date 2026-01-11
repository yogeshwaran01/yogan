import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { TooltipModule } from 'primeng/tooltip';
import { AiService } from '../../core/services/ai.service';
import { StateService } from '../../core/services/state.service';

interface Message {
  role: 'user' | 'assistant';
  content: string;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    CardModule,
    AvatarModule,
    ProgressSpinnerModule,
    ScrollPanelModule,
    InputTextareaModule,
    TooltipModule
  ],
  template: `
    <div class="chat-container">
      <div class="chat-header">
          <span class="chat-title">Chat Session</span>
          <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" pTooltip="Clear History" (onClick)="clearChat()"></p-button>
      </div>
      <div class="chat-content">
          <p-scrollPanel [style]="{ width: '100%', height: '100%' }" styleClass="custom-scrollbar">
        <div class="messages-list">
          <div *ngIf="messages.length === 0" class="empty-state">
            <i class="pi pi-bolt" style="font-size: 2rem; color: var(--primary-color)"></i>
            <h3>How can I help you today?</h3>
          </div>

          <div *ngFor="let msg of messages" class="message-item" [ngClass]="{'user-message': msg.role === 'user', 'ai-message': msg.role === 'assistant'}">
            <div class="message-content">
              <div class="avatar-container">
                 <p-avatar *ngIf="msg.role === 'assistant'" icon="pi pi-android" shape="circle" styleClass="ai-avatar"></p-avatar>
                 <p-avatar *ngIf="msg.role === 'user'" icon="pi pi-user" shape="circle" styleClass="user-avatar"></p-avatar>
              </div>
              <div class="text-bubble">
                <span style="white-space: pre-wrap">{{ msg.content }}</span>
              </div>
            </div>
          </div>



          <div #bottomAnchor></div>
        </div>
          </p-scrollPanel>
      </div>

      <div class="input-area">
        <div class="input-wrapper">
          <textarea
            pInputTextarea
            [autoResize]="true"
            [(ngModel)]="currentInput"
            (keydown.enter)="sendMessage($event)"
            placeholder="Type your message..."
            rows="1"
            class="chat-input"
            [disabled]="isGenerating()"
          ></textarea>
          <p-button
            icon="pi pi-send"
            [rounded]="true"
            [text]="true"
            (onClick)="sendMessage()"
            [disabled]="!currentInput.trim() || isGenerating()"
          ></p-button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .chat-container {
      display: flex;
      flex-direction: column;
      height: 100%;
      background-color: var(--surface-card);
      overflow: hidden;
    }

    .chat-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.5rem 1rem;
        background: var(--surface-50);
        border-bottom: 1px solid var(--surface-border);
        flex-shrink: 0;

        .chat-title {
            font-weight: 600;
            color: var(--text-color);
        }
    }

    .chat-content {
        flex: 1;
        overflow: hidden;
        position: relative;
    }

    .messages-list {
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 400px;
      opacity: 0.7;

      h3 {
        margin-top: 1rem;
        font-weight: 500;
      }
    }

    .message-item {
      display: flex;
      width: 100%;
    }

    .user-message {
      justify-content: flex-end;

      .message-content {
        flex-direction: row-reverse;
      }

      .text-bubble {
        background-color: var(--primary-color);
        color: var(--primary-color-text);
        border-bottom-right-radius: 4px;
      }
    }

    .ai-message {
      justify-content: flex-start;

      .text-bubble {
        background-color: var(--surface-100);
        color: var(--text-color);
        border-bottom-left-radius: 4px;
      }
    }

    .message-content {
      display: flex;
      gap: 0.75rem;
      max-width: 80%;
      align-items: flex-end;
    }

    .text-bubble {
      padding: 0.75rem 1rem;
      border-radius: 12px;
      line-height: 1.5;
      font-size: 0.95rem;
    }

    .input-area {
      padding: 1rem;
      border-top: 1px solid var(--surface-border);
      background-color: var(--surface-card);
      flex-shrink: 0;
    }

    .input-wrapper {
      display: flex;
      align-items: flex-end;
      gap: 0.5rem;
      background-color: var(--surface-50);
      padding: 0.5rem;
      border-radius: 24px;
      border: 1px solid var(--surface-border);
      transition: border-color 0.2s;

      &:focus-within {
        border-color: var(--primary-color);
      }
    }

    .chat-input {
      flex: 1;
      border: none;
      background: transparent;
      padding: 0.5rem 1rem;
      max-height: 120px;
      resize: none;

      &:focus {
        box-shadow: none;
        outline: none;
      }
    }

    ::ng-deep .ai-avatar {
      background-color: var(--teal-500);
      color: white;
    }

    ::ng-deep .user-avatar {
      background-color: var(--indigo-500);
      color: white;
    }
  `]
})
export class ChatComponent {
  messages: Message[] = [];
  currentInput = '';

  private aiService = inject(AiService);
  private stateService = inject(StateService);
  private cdr = inject(ChangeDetectorRef);

  isGenerating = this.stateService.isLoading;

  @ViewChild('bottomAnchor') bottomAnchor!: ElementRef;

  async sendMessage(event?: Event) {
    if (event) {
      event.preventDefault();
    }

    if (!this.currentInput.trim() || this.isGenerating()) return;

    const userMsg = this.currentInput.trim();
    this.messages.push({ role: 'user', content: userMsg });
    this.currentInput = '';
    this.scrollToBottom();

    this.stateService.setLoading(true);

    const aiMsgIndex = this.messages.push({ role: 'assistant', content: '' }) - 1;

    try {
      const isRag = this.stateService.isRagEnabled();
      const store = this.stateService.selectedStore();

      if (isRag && !store) {
        this.messages.push({ role: 'assistant', content: 'Please select a Store (Collection) in settings to use RAG mode, or disable RAG mode.' });
        this.stateService.setLoading(false);
        return;
      }

      const param = {
        Prompt: userMsg,
        Model: this.stateService.selectedModel(),
        Client: this.stateService.selectedClient(),
        StoreName: store,
        IsRagEnabled: isRag
      };

      for await (const chunk of this.aiService.generateStream(param)) {
        this.messages[aiMsgIndex].content += chunk;
        this.scrollToBottom();
        this.cdr.detectChanges(); // Manually trigger detection for async generator updates
      }
    } catch (error) {
      console.error(error);
      this.messages[aiMsgIndex].content += '\n[Error generating response]';
    } finally {
      this.stateService.setLoading(false);
    }
  }

  scrollToBottom() {
    setTimeout(() => {
      this.bottomAnchor?.nativeElement?.scrollIntoView({ behavior: 'smooth' });
    }, 50);
  }

  clearChat() {
    this.messages = [];
  }
}
