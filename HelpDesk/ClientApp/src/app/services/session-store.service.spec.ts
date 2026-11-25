import { beforeEach, describe, expect, it } from 'vitest';
import { SessionStore } from './session-store.service';

describe('SessionStore', () => {
  let store: SessionStore;
  beforeEach(() => { sessionStorage.clear(); store = new SessionStore(); });

  it('keeps the format of the library it replaced, so a signed-in person stays signed in after an update', () => {
    store.store('user', { id: 3, nombre: 'Ana' });
    expect(sessionStorage.getItem('ng2-webstorage|user')).toBe('{"id":3,"nombre":"Ana"}');
  });

  it('gives back what was stored, as an object', () => {
    store.store('user', { id: 3 });
    expect(store.retrieve('user')).toEqual({ id: 3 });
  });

  it('answers null when nothing is stored under the name', () => {
    expect(store.retrieve('user')).toBeNull();
  });

  it('gives back the raw text when the stored value is not JSON', () => {
    sessionStorage.setItem('ng2-webstorage|note', 'hola');
    expect(store.retrieve('note')).toBe('hola');
  });

  it('forgets a name without touching the others', () => {
    store.store('a', 1); store.store('b', 2);
    store.clear('a');
    expect(store.retrieve('a')).toBeNull();
    expect(store.retrieve('b')).toBe(2);
  });
});
