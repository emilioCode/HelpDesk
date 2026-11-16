import { Injectable } from '@angular/core';

// What the application needs from the session storage of the browser: keep the signed-in user while the tab lives.
// It replaces the library ngx-webstorage (tied to each version of Angular, and used only for this) and keeps its format on purpose:
// the key is "ng2-webstorage|<name>" and the value is JSON, so a person who is signed in when the application is updated stays signed in.
@Injectable()
export class SessionStore {
  private prefix = 'ng2-webstorage|';

  store(key: string, value: any): void {
    sessionStorage.setItem(this.prefix + key, JSON.stringify(value));
  }

  // null when there is nothing stored under that name
  retrieve(key: string): any {
    const raw = sessionStorage.getItem(this.prefix + key);
    if (raw === null) {
      return null;
    }
    try {
      return JSON.parse(raw);
    } catch (e) {
      return raw;
    }
  }

  clear(key: string): void {
    sessionStorage.removeItem(this.prefix + key);
  }
}
