import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LogFileInfo {
  name: string;
  size: number;
  lastWriteUtc: string;
}

@Injectable({ providedIn: 'root' })
export class LogsService {
  private apiUrl = 'http://localhost:5122/api/logs';

  constructor(private http: HttpClient) {}

  listFiles(): Observable<LogFileInfo[]> {
    return this.http.get<LogFileInfo[]>(`${this.apiUrl}/files`);
  }

  getRecent(maxLines = 500): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/recent`, { params: { maxLines } as any });
  }

  download(date?: string): Observable<Blob> {
    const params: any = {};
    if (date) params.date = date;
    return this.http.get(`${this.apiUrl}/download`, { params, responseType: 'blob' });
  }

  downloadAll(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download-all`, { responseType: 'blob' });
  }

  clear(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/clear`);
  }
}
