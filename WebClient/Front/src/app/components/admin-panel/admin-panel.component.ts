import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface User {
  id: string;
  name: string;
  email: string;
  role: 'admin' | 'manager' | 'user';
  status: 'active' | 'inactive' | 'suspended';
  lastActivity: Date;
}

interface Marketplace {
  id: string;
  name: string;
  description: string;
  token: string;
  apiUrl: string;
  connected: boolean;
  lastTest?: Date;
  testResult?: 'success' | 'error';
}

interface ActiveToken {
  id: string;
  name: string;
  marketplace: string;
  created: Date;
  daysUntilExpiry: number;
}

interface SystemStats {
  totalUsers: number;
  activeTokens: number;
  totalExports: number;
  storageUsed: number;
}

interface SystemConfig {
  appName: string;
  maxProductsPerExport: number;
  exportTimeout: number;
  autoRetryFailedExports: boolean;
  emailNotifications: boolean;
}

interface EmailConfig {
  smtpServer: string;
  port: number;
  username: string;
  password: string;
  useSSL: boolean;
}

interface HistoryEntry {
  timestamp: Date;
  userName: string;
  userEmail: string;
  action: 'upload' | 'export' | 'user_add' | 'user_edit' | 'config_change';
  details: string;
  itemCount?: number;
  status: 'success' | 'error' | 'warning';
  duration?: string;
}

interface UserForm {
  name: string;
  email: string;
  role: string;
  password: string;
  active: boolean;
}

@Component({
  selector: 'app-admin-panel',
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ]
})
export class AdminPanelComponent implements OnInit {
  activeTab = 'users';
  
  // System Stats
  systemStats: SystemStats = {
    totalUsers: 0,
    activeTokens: 0,
    totalExports: 0,
    storageUsed: 0
  };

  // Users Management
  users: User[] = [];
  filteredUsers: User[] = [];
  userSearchTerm = '';
  roleFilter = '';
  showUserModal = false;
  editingUser: User | null = null;
  userForm: UserForm = {
    name: '',
    email: '',
    role: 'user',
    password: '',
    active: true
  };

  // API & Tokens
  marketplaces: Marketplace[] = [
    {
      id: 'emag',
      name: 'eMAG',
      description: 'Platform de e-commerce eMAG',
      token: '',
      apiUrl: 'https://api.emag.ro/v1',
      connected: false
    },
    {
      id: 'altex',
      name: 'Altex',
      description: 'Marketplace Altex',
      token: '',
      apiUrl: 'https://api.altex.ro/v2',
      connected: false
    },
    {
      id: 'okazii',
      name: 'Okazii',
      description: 'Platforma Okazii',
      token: '',
      apiUrl: 'https://api.okazii.ro/v1',
      connected: false
    },
    {
      id: 'olx',
      name: 'OLX',
      description: 'Anunturi OLX',
      token: '',
      apiUrl: 'https://api.olx.ro/v1',
      connected: false
    }
  ];

  activeTokens: ActiveToken[] = [];

  // System Configuration
  systemConfig: SystemConfig = {
    appName: 'Export Marketplace Platform',
    maxProductsPerExport: 1000,
    exportTimeout: 300,
    autoRetryFailedExports: true,
    emailNotifications: true
  };

  emailConfig: EmailConfig = {
    smtpServer: 'smtp.gmail.com',
    port: 587,
    username: 'noreply@company.com',
    password: '',
    useSSL: true
  };

  // History & Audit
  history: HistoryEntry[] = [];
  filteredHistory: HistoryEntry[] = [];
  historyDateFrom = '';
  historyDateTo = '';

  ngOnInit(): void {
    this.loadMockData();
    this.loadHistory();
  }

  loadMockData(): void {
    this.users = [];
    this.filteredUsers = [...this.users];
  }

  loadHistory(): void {
    this.history = [];
    this.filteredHistory = [...this.history];
  }

  // Tab Management
  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  // User Management
  filterUsers(): void {
    this.filteredUsers = this.users.filter(user => {
      const matchesSearch = !this.userSearchTerm || 
        user.name.toLowerCase().includes(this.userSearchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.userSearchTerm.toLowerCase());

      const matchesRole = !this.roleFilter || user.role === this.roleFilter;

      return matchesSearch && matchesRole;
    });
  }

  openAddUserModal(): void {
    this.editingUser = null;
    this.userForm = {
      name: '',
      email: '',
      role: 'user',
      password: '',
      active: true
    };
    this.showUserModal = true;
  }

  editUser(user: User): void {
    this.editingUser = user;
    this.userForm = {
      name: user.name,
      email: user.email,
      role: user.role,
      password: '',
      active: user.status === 'active'
    };
    this.showUserModal = true;
  }

  closeUserModal(): void {
    this.showUserModal = false;
    this.editingUser = null;
  }

  saveUser(): void {
    if (this.editingUser) {
      // Update existing user
      this.editingUser.name = this.userForm.name;
      this.editingUser.email = this.userForm.email;
      this.editingUser.role = this.userForm.role as any;
      this.editingUser.status = this.userForm.active ? 'active' : 'inactive';
    } else {
      // Add new user
      const newUser: User = {
        id: Date.now().toString(),
        name: this.userForm.name,
        email: this.userForm.email,
        role: this.userForm.role as any,
        status: this.userForm.active ? 'active' : 'inactive',
        lastActivity: new Date()
      };
      this.users.push(newUser);
    }

    this.filterUsers();
    this.closeUserModal();
    alert('Utilizatorul a fost salvat cu succes!');
  }

  toggleUserStatus(user: User): void {
    user.status = user.status === 'active' ? 'inactive' : 'active';
    alert(`Statusul utilizatorului ${user.name} a fost schimbat la ${user.status}.`);
  }

  deleteUser(user: User): void {
    if (user.role === 'admin') {
      alert('Nu poți șterge un administrator!');
      return;
    }

    if (confirm(`Ești sigur că vrei să ștergi utilizatorul ${user.name}?`)) {
      this.users = this.users.filter(u => u.id !== user.id);
      this.filterUsers();
    }
  }

  // API & Token Management
  testConnection(marketplace: Marketplace): void {
    marketplace.lastTest = new Date();
    
    // Simulate connection test
    setTimeout(() => {
      marketplace.testResult = Math.random() > 0.2 ? 'success' : 'error';
      marketplace.connected = marketplace.testResult === 'success';
      
      const message = marketplace.connected 
        ? `Conexiunea la ${marketplace.name} a fost testată cu succes!`
        : `Conexiunea la ${marketplace.name} a eșuat. Verifică token-ul și URL-ul.`;
      
      alert(message);
    }, 1000);
  }

  saveMarketplaceConfig(marketplace: Marketplace): void {
    alert(`Configurarea pentru ${marketplace.name} a fost salvată!`);
  }

  revokeToken(token: ActiveToken): void {
    if (confirm(`Ești sigur că vrei să revoci token-ul ${token.name}?`)) {
      this.activeTokens = this.activeTokens.filter(t => t.id !== token.id);
      alert('Token-ul a fost revocat cu succes!');
    }
  }

  // System Configuration
  saveSystemConfig(): void {
    alert('Configurările sistemului au fost salvate cu succes!');
  }

  resetToDefaults(): void {
    if (confirm('Ești sigur că vrei să resetezi toate configurările la valorile implicite?')) {
      this.systemConfig = {
        appName: 'Export Marketplace Platform',
        maxProductsPerExport: 1000,
        exportTimeout: 300,
        autoRetryFailedExports: true,
        emailNotifications: true
      };

      this.emailConfig = {
        smtpServer: 'smtp.gmail.com',
        port: 587,
        username: 'noreply@company.com',
        password: '',
        useSSL: true
      };

      alert('Configurările au fost resetate la valorile implicite!');
    }
  }

  testEmailConfig(): void {
    alert('Email de test trimis cu succes! Verifică inbox-ul.');
  }

  // History & Audit
  filterHistory(): void {
    // In real app, filter by date range
    this.filteredHistory = [...this.history];
  }

  backupSystem(): void {
    const filename = `backup_system_${new Date().toISOString().split('T')[0]}.zip`;
    alert(`Backup-ul sistemului a fost creat: ${filename}`);
  }

  // Utility Methods
  getUserInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
  }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'admin': return 'bg-danger';
      case 'manager': return 'bg-warning';
      case 'user': return 'bg-primary';
      default: return 'bg-secondary';
    }
  }

  getRoleLabel(role: string): string {
    switch (role) {
      case 'admin': return 'Administrator';
      case 'manager': return 'Manager';
      case 'user': return 'Utilizator';
      default: return role;
    }
  }

  getUserStatusClass(status: string): string {
    switch (status) {
      case 'active': return 'status-success';
      case 'inactive': return 'status-warning';
      case 'suspended': return 'status-error';
      default: return '';
    }
  }

  getUserStatusLabel(status: string): string {
    switch (status) {
      case 'active': return 'Activ';
      case 'inactive': return 'Inactiv';
      case 'suspended': return 'Suspendat';
      default: return status;
    }
  }

  getActionIcon(action: string): string {
    switch (action) {
      case 'upload': return 'bi-upload';
      case 'export': return 'bi-download';
      case 'user_add': return 'bi-person-plus';
      case 'user_edit': return 'bi-person-gear';
      case 'config_change': return 'bi-gear';
      default: return 'bi-circle';
    }
  }

  getActionLabel(action: string): string {
    switch (action) {
      case 'upload': return 'Upload Feed';
      case 'export': return 'Export Produse';
      case 'user_add': return 'Adăugare Utilizator';
      case 'user_edit': return 'Editare Utilizator';
      case 'config_change': return 'Modificare Configurări';
      default: return action;
    }
  }

  getHistoryStatusClass(status: string): string {
    switch (status) {
      case 'success': return 'status-success';
      case 'error': return 'status-error';
      case 'warning': return 'status-warning';
      default: return '';
    }
  }

  getHistoryStatusLabel(status: string): string {
    switch (status) {
      case 'success': return 'Succes';
      case 'error': return 'Eroare';
      case 'warning': return 'Avertisment';
      default: return status;
    }
  }
} 