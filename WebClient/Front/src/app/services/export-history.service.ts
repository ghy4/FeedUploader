import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ExportStatus = 'completed' | 'failed' | 'processing';

export interface LogEntry {
  timestamp: Date;
  type: 'success' | 'error' | 'warning' | 'info';
  action?: string;
  productName?: string;
  productSku?: string;
  message: string;
  details?: string;
}

export interface ExportJob {
  id: string;
  name: string;
  marketplace: string; // display name (e.g., eMAG)
  startTime: Date;
  endTime?: Date;
  status: ExportStatus;
  progress: number; // 0..100
  totalProducts: number;
  successfulProducts: number;
  failedProducts: number;
  logEntries?: LogEntry[];
}

@Injectable({ providedIn: 'root' })
export class ExportHistoryService {
  private readonly storageKey = 'exportHistory';
  private subject = new BehaviorSubject<ExportJob[]>(this.load());
  readonly changes$ = this.subject.asObservable();

  getAll(): ExportJob[] { return this.subject.getValue(); }

  add(job: ExportJob): void {
    const all = [job, ...this.getAll()];
    this.save(all);
  }

  update(id: string, patch: Partial<ExportJob>): void {
    const all = this.getAll().map(j => j.id === id ? this.mergeJob(j, patch) : j);
    this.save(all);
  }

  appendLog(id: string, entry: LogEntry): void {
    const all = this.getAll().map(j => {
      if (j.id !== id) return j;
      const logs = [...(j.logEntries ?? [])];
      logs.push({ ...entry, timestamp: new Date(entry.timestamp) });
      return { ...j, logEntries: logs };
    });
    this.save(all);
  }

  remove(id: string): void {
    const all = this.getAll().filter(j => j.id !== id);
    this.save(all);
  }

  clear(): void { this.save([]); }

  private mergeJob(orig: ExportJob, patch: Partial<ExportJob>): ExportJob {
    // Normalize date fields if present in patch
    const merged: any = { ...orig, ...patch };
    if (patch.startTime) merged.startTime = new Date(patch.startTime);
    if (patch.endTime) merged.endTime = patch.endTime ? new Date(patch.endTime) : undefined;
    return merged as ExportJob;
  }

  private load(): ExportJob[] {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return [];
      const arr = JSON.parse(raw) as any[];
      return (arr ?? []).map(j => ({
        ...j,
        startTime: j.startTime ? new Date(j.startTime) : new Date(),
        endTime: j.endTime ? new Date(j.endTime) : undefined,
        logEntries: (j.logEntries ?? []).map((e: any) => ({ ...e, timestamp: new Date(e.timestamp) }))
      })) as ExportJob[];
    } catch {
      return [];
    }
  }

  private save(all: ExportJob[]): void {
    localStorage.setItem(this.storageKey, JSON.stringify(all));
    this.subject.next(all);
  }
}
