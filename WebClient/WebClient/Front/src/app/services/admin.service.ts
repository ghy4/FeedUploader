import { Injectable } from '@angular/core';

interface User {
  id: string;
  username: string;
  email: string;
  role: 'admin' | 'manager' | 'user';
  status: 'active' | 'inactive';
  createdAt: Date;
  lastLogin?: Date;
}

interface ApiToken {
  id: string;
  name: string;
  marketplace: string;
  token: string;
  status: 'active' | 'inactive';
  createdAt: Date;
  lastUsed?: Date;
}

interface SystemSettings {
  maxFileSize: number;
  allowedFileTypes: string[];
  autoExportEnabled: boolean;
  exportRetentionDays: number;
  notificationEmail: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private users: User[] = [];

  private apiTokens: ApiToken[] = [];

  private systemSettings: SystemSettings = {
    maxFileSize: 10 * 1024 * 1024, // 10MB
    allowedFileTypes: ['.csv', '.xlsx', '.xml', '.json'],
    autoExportEnabled: true,
    exportRetentionDays: 30,
    notificationEmail: 'admin@company.com'
  };

  constructor() { }

  // User Management
  getUsers() {
    return [...this.users];
  }

  addUser(user: Omit<User, 'id' | 'createdAt'>) {
    const newUser: User = {
      ...user,
      id: Date.now().toString(),
      createdAt: new Date()
    };
    this.users.push(newUser);
    return newUser;
  }

  updateUser(id: string, updates: Partial<User>) {
    const userIndex = this.users.findIndex(u => u.id === id);
    if (userIndex !== -1) {
      this.users[userIndex] = { ...this.users[userIndex], ...updates };
      return this.users[userIndex];
    }
    return null;
  }

  deleteUser(id: string) {
    this.users = this.users.filter(u => u.id !== id);
  }

  // API Token Management
  getApiTokens() {
    return [...this.apiTokens];
  }

  addApiToken(token: Omit<ApiToken, 'id' | 'createdAt'>) {
    const newToken: ApiToken = {
      ...token,
      id: Date.now().toString(),
      createdAt: new Date()
    };
    this.apiTokens.push(newToken);
    return newToken;
  }

  updateApiToken(id: string, updates: Partial<ApiToken>) {
    const tokenIndex = this.apiTokens.findIndex(t => t.id === id);
    if (tokenIndex !== -1) {
      this.apiTokens[tokenIndex] = { ...this.apiTokens[tokenIndex], ...updates };
      return this.apiTokens[tokenIndex];
    }
    return null;
  }

  deleteApiToken(id: string) {
    this.apiTokens = this.apiTokens.filter(t => t.id !== id);
  }

  // System Settings
  getSystemSettings() {
    return { ...this.systemSettings };
  }

  updateSystemSettings(settings: Partial<SystemSettings>) {
    this.systemSettings = { ...this.systemSettings, ...settings };
    return this.systemSettings;
  }

  // Statistics
  getSystemStats() {
    return {
      totalUsers: this.users.length,
      activeUsers: this.users.filter(u => u.status === 'active').length,
      totalTokens: this.apiTokens.length,
      activeTokens: this.apiTokens.filter(t => t.status === 'active').length
    };
  }
} 