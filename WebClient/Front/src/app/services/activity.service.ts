import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ActivityEntry {
  type: 'upload' | 'export' | 'mapping';
  description: string;
  status: 'success' | 'error' | 'warning';
  date: Date;
}

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private activitiesSubject = new BehaviorSubject<ActivityEntry[]>([]);
  activities$ = this.activitiesSubject.asObservable();

  add(entry: ActivityEntry) {
    const current = this.activitiesSubject.getValue();
    this.activitiesSubject.next([entry, ...current].slice(0, 50));
  }

  getSnapshot(): ActivityEntry[] { return this.activitiesSubject.getValue(); }

  clear() { this.activitiesSubject.next([]); }
}
