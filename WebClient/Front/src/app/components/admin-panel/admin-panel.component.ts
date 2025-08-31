import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, AppUser } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';

interface UserVM {
  id: number;
  name: string;
  surname: string;
  email: string;
  role: string;
  status: string;
  lastActivity?: Date;
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
  surname: string;     
  email: string;
  role: string;
  password: string;
  contactNumber: string;
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
  
  systemStats: SystemStats = {
    totalUsers: 0,
    activeTokens: 0,
    totalExports: 0,
    storageUsed: 0
  };

  users: UserVM[] = [];
  filteredUsers: UserVM[] = [];
  userSearchTerm = '';
  roleFilter = '';
  showUserModal = false;
  editingUser: UserVM | null = null;
  userForm: UserForm = {
    name: '',
    surname: '',
    email: '',
    role: 'user',
    password: '',
    contactNumber: '',
    active: true
  };
  formErrors: string[] = [];
  savingUser = false;

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

  constructor(private userService: UserService, private auth: AuthService) {}

  ngOnInit(): void {
    // Safety: ensure only admin; route guard should handle already
    if (!this.auth.isAdmin()) return;
    this.loadUsers();
    this.loadHistory();
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe(list => {
      this.users = list.map(u => ({
        id: u.id,
  name: u.name,
  surname: u.surname,
        email: u.email,
        role: u.role,
        status: u.status || 'active',
        lastActivity: u.lastActivity ? new Date(u.lastActivity) : undefined
      }));
      this.filteredUsers = [...this.users];
      this.filterUsers();
  this.updateSystemStats();
    });
  }

  loadHistory(): void {
    this.history = [];
    this.filteredHistory = [...this.history];
  this.updateSystemStats();
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
  user.surname.toLowerCase().includes(this.userSearchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.userSearchTerm.toLowerCase());

      const matchesRole = !this.roleFilter || user.role === this.roleFilter;

      return matchesSearch && matchesRole;
    });
  }

  openAddUserModal(): void {
    this.editingUser = null;
    this.userForm = {
      name: '',
      surname: '',
      email: '',
      role: 'user',
      password: '',
      contactNumber: '',
      active: true
    };
    this.showUserModal = true;
    this.formErrors = [];
  }

  editUser(user: UserVM): void {
    this.editingUser = user;
    this.userForm = {
      name: user.name,
      surname: user.surname,
      email: user.email,
      role: user.role,
      password: '',
      contactNumber: '',
      active: user.status === 'active'
    };
    this.showUserModal = true;
    this.formErrors = [];
  }

  closeUserModal(): void {
    this.showUserModal = false;
    this.editingUser = null;
  }

  saveUser(): void {
    this.formErrors = this.validateUserForm();
    if (this.formErrors.length) return;
    if (this.savingUser) return;
    this.savingUser = true;

    if (!this.editingUser) {
      // Create user
      const { name, surname, email, password, contactNumber, role } = this.prepareUserPayload();
      this.userService.createUser({ name, surname, email, password, contactNumber, role }).subscribe(created => {
        this.savingUser = false;
        if (!created) {
          this.formErrors = ['Eroare la crearea utilizatorului.'];
          return;
        }
        // Optimistically push; backend returns created user DTO (Id etc.)
        this.users.push({
          id: (created as any).id || created.id,
            name: created.name,
            surname: created.surname,
            email: created.email,
            role: created.role,
            status: this.userForm.active ? 'active' : 'inactive',
            lastActivity: new Date()
        });
        this.filterUsers();
  this.updateSystemStats();
        this.closeUserModal();
      }, _ => {
        this.savingUser = false;
        this.formErrors = ['Eroare la rețea la crearea utilizatorului.'];
      });
    } else {
      // Update local only (no backend endpoint yet)
      this.editingUser.name = this.userForm.name;
  this.editingUser.surname = this.userForm.surname;
      this.editingUser.email = this.userForm.email;
      this.editingUser.role = this.userForm.role;
      this.editingUser.status = this.userForm.active ? 'active' : 'inactive';
      this.filterUsers();
      this.closeUserModal();
      this.savingUser = false;
  this.updateSystemStats();
    }
  }

  private validateUserForm(): string[] {
    const errors: string[] = [];
    if (!this.userForm.name.trim()) errors.push('Numele este obligatoriu');
    if (!this.userForm.email.trim() || !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(this.userForm.email)) errors.push('Email invalid');
    if (!this.editingUser && !this.userForm.password.trim()) errors.push('Parola este obligatorie pentru creare');
    if (this.userForm.password && this.userForm.password.length < 6) errors.push('Parola trebuie să aibă minim 6 caractere');
    return errors;
  }

  private prepareUserPayload() {
    // Split full name into name + surname if surname field left blank
    let first = this.userForm.name.trim();
    let sur = this.userForm.surname.trim();
    if (!sur && first.includes(' ')) {
      const parts = first.split(' ');
      first = parts.shift() || first;
      sur = parts.join(' ');
    }
    return {
      name: first,
      surname: sur,
      email: this.userForm.email.trim(),
      password: this.userForm.password,
      contactNumber: this.userForm.contactNumber || '',
      role: this.userForm.role
    };
  }

  toggleUserStatus(user: UserVM): void {
    user.status = user.status === 'active' ? 'inactive' : 'active';
    alert(`Statusul utilizatorului ${user.name} a fost schimbat la ${user.status}.`);
  this.updateSystemStats();
  }

  deleteUser(user: UserVM): void {
    if (user.role.toLowerCase() === 'admin') {
      alert('Nu poți șterge un administrator!');
      return;
    }
    if (!confirm(`Ești sigur că vrei să ștergi utilizatorul ${user.name}?`)) return;
    this.userService.deleteUser(user.id).subscribe(() => {
      this.users = this.users.filter(u => u.id !== user.id);
      this.filterUsers();
  this.updateSystemStats();
    });
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
  this.updateSystemStats();
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

  fullName(u: UserVM): string {
    return `${u.name} ${u.surname}`.trim();
  }

  private updateSystemStats(): void {
    const activeUsers = this.users.filter(u => u.status === 'active').length;
    // totalUsers field is used in the card labelled "Utilizatori Activi" so store active count there
    this.systemStats.totalUsers = activeUsers;
    this.systemStats.activeTokens = this.activeTokens.length;
    this.systemStats.totalExports = this.history.filter(h => h.action === 'export').length;
    // storageUsed could later be computed; keep existing if already set
  }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'Admin': return 'bg-danger';
      case 'User': return 'bg-primary';
      default: return 'bg-secondary';
    }
  }

  getRoleLabel(role: string): string {
    switch (role) {
      case 'Admin': return 'Admin';
      case 'User': return 'Utilizator';
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