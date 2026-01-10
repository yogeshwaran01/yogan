import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SidebarModule } from 'primeng/sidebar';
import { StateService } from './core/services/state.service';
import { SettingsComponent } from './features/settings/settings.component';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterOutlet, SidebarModule, ButtonModule, CommonModule, SettingsComponent],
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss'
})
export class AppComponent {
    title = 'client';

    private stateService = inject(StateService);

    // Bridge for Two-Way Binding with Signal
    get isSidebarVisible(): boolean {
        return this.stateService.isSidebarVisible();
    }

    set isSidebarVisible(value: boolean) {
        // We only set it if it changes, though usage with [(visible)] implies usually setting to false on close
        if (value !== this.stateService.isSidebarVisible()) {
            this.stateService.toggleSidebar(); // Or explicitly set if we had a specific setter
        }
    }

    toggleSidebar() {
        this.stateService.toggleSidebar();
    }
}
