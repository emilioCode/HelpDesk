import { afterEach, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest';
import { hideModal, installPageBehaviors, showFirstTab, showModal } from './page-behaviors';

const FADE = 150;
const click = (el: Element) => el.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
const escape = () => document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

describe('page behaviors', () => {
  beforeAll(() => installPageBehaviors());
  beforeEach(() => { vi.useFakeTimers(); });
  afterEach(() => {
    // close whatever a test left open, then clean the page
    document.querySelectorAll<HTMLElement>('.modal').forEach(m => hideModal(m));
    vi.advanceTimersByTime(FADE + 10);
    vi.useRealTimers();
    document.body.innerHTML = '';
  });

  const page = (html: string) => { document.body.innerHTML = html; };
  const modalHtml = (id: string, extra = '') => `<div class="modal fade" id="${id}" tabindex="-1" aria-hidden="true" ${extra}><div class="modal-dialog"><div class="modal-content"><button class="close" data-dismiss="modal">x</button><div class="modal-body">hello</div></div></div></div>`;

  describe('modals', () => {
    it('opens with the attributes of the opener and shows a dimmed backdrop', () => {
      page(`<a href="#" id="go" data-toggle="modal" data-target="#m1">open</a>${modalHtml('m1')}`);
      click(document.getElementById('go')!);
      const modal = document.getElementById('m1')!;
      expect(modal.classList.contains('in')).toBe(true);
      expect(modal.style.display).toBe('block');
      expect(modal.getAttribute('aria-hidden')).toBeNull();
      expect(document.querySelectorAll('.modal-backdrop.in').length).toBe(1);
      expect(document.body.classList.contains('modal-open')).toBe(true);
    });

    it('does not follow the link of an opener', () => {
      page(`<a href="#other" id="go" data-toggle="modal" data-target="#m1">open</a>${modalHtml('m1')}`);
      const ev = new MouseEvent('click', { bubbles: true, cancelable: true });
      document.getElementById('go')!.dispatchEvent(ev);
      expect(ev.defaultPrevented).toBe(true);
    });

    it('opens from a click on an icon inside the opener', () => {
      page(`<button id="go" data-toggle="modal" data-target="#m1"><i id="ico"></i></button>${modalHtml('m1')}`);
      click(document.getElementById('ico')!);
      expect(document.getElementById('m1')!.classList.contains('in')).toBe(true);
    });

    it('closes with the dismiss button, and removes the backdrop once the fade ends', () => {
      page(modalHtml('m1'));
      showModal(document.getElementById('m1'));
      click(document.querySelector('#m1 [data-dismiss="modal"]')!);
      expect(document.getElementById('m1')!.classList.contains('in')).toBe(false);
      vi.advanceTimersByTime(FADE + 10);
      expect(document.getElementById('m1')!.style.display).toBe('none');
      expect(document.getElementById('m1')!.getAttribute('aria-hidden')).toBe('true');
      expect(document.querySelectorAll('.modal-backdrop').length).toBe(0);
      expect(document.body.classList.contains('modal-open')).toBe(false);
    });

    it('closes with Escape', () => {
      page(modalHtml('m1'));
      showModal(document.getElementById('m1'));
      escape();
      expect(document.getElementById('m1')!.classList.contains('in')).toBe(false);
    });

    it('Escape closes only the window on top', () => {
      page(modalHtml('a') + modalHtml('b'));
      showModal(document.getElementById('a')); showModal(document.getElementById('b'));
      escape();
      expect(document.getElementById('b')!.classList.contains('in')).toBe(false);
      expect(document.getElementById('a')!.classList.contains('in')).toBe(true);
      expect(document.body.classList.contains('modal-open')).toBe(true);
    });

    it('Escape does nothing in a window that asks to keep it', () => {
      page(modalHtml('m1', 'data-keyboard="false"'));
      showModal(document.getElementById('m1'));
      escape();
      expect(document.getElementById('m1')!.classList.contains('in')).toBe(true);
    });

    it('a click on the dimmed area closes the window', () => {
      page(modalHtml('m1'));
      showModal(document.getElementById('m1'));
      click(document.getElementById('m1')!);
      expect(document.getElementById('m1')!.classList.contains('in')).toBe(false);
    });

    it('a click on the dimmed area does not close a static window', () => {
      page(modalHtml('m1', 'data-backdrop="static"'));
      showModal(document.getElementById('m1'));
      click(document.getElementById('m1')!);
      expect(document.getElementById('m1')!.classList.contains('in')).toBe(true);
    });

    it('a click inside the window does not close it', () => {
      page(modalHtml('m1'));
      showModal(document.getElementById('m1'));
      click(document.querySelector('#m1 .modal-body')!);
      expect(document.getElementById('m1')!.classList.contains('in')).toBe(true);
    });

    it('opening a window twice makes one backdrop', () => {
      page(modalHtml('m1'));
      showModal(document.getElementById('m1')); showModal(document.getElementById('m1'));
      expect(document.querySelectorAll('.modal-backdrop').length).toBe(1);
    });

    it('returns the focus to what opened it', () => {
      page(`<button id="go" data-toggle="modal" data-target="#m1">open</button>${modalHtml('m1')}`);
      const go = document.getElementById('go')!;
      click(go);
      click(document.querySelector('#m1 [data-dismiss="modal"]')!);
      vi.advanceTimersByTime(FADE + 10);
      expect(document.activeElement).toBe(go);
    });

    it('ignores a target that does not exist', () => {
      page('<a href="#" id="go" data-toggle="modal" data-target="#nope">open</a>');
      expect(() => click(document.getElementById('go')!)).not.toThrow();
      expect(document.querySelectorAll('.modal-backdrop').length).toBe(0);
    });
  });

  describe('tabs', () => {
    const tabs = `
      <ul class="nav nav-tabs">
        <li class="active" id="l1"><a href="#p1" data-toggle="tab">One</a></li>
        <li id="l2"><a href="#p2" data-toggle="tab">Two</a></li>
      </ul>
      <div class="tab-content"><div class="tab-pane active" id="p1">1</div><div class="tab-pane" id="p2">2</div></div>`;

    it('shows the chosen pane and marks its tab', () => {
      page(tabs);
      click(document.querySelector('#l2 a')!);
      expect(document.getElementById('l2')!.classList.contains('active')).toBe(true);
      expect(document.getElementById('l1')!.classList.contains('active')).toBe(false);
      expect(document.getElementById('p2')!.classList.contains('active')).toBe(true);
      expect(document.getElementById('p1')!.classList.contains('active')).toBe(false);
    });

    it('goes back to the first tab', () => {
      page(`<div id="win">${tabs}</div>`);
      click(document.querySelector('#l2 a')!);
      showFirstTab('#win');
      expect(document.getElementById('p1')!.classList.contains('active')).toBe(true);
      expect(document.getElementById('p2')!.classList.contains('active')).toBe(false);
    });

    it('goes back to the first tab even when the window has no tabs', () => {
      page('<div id="win"></div>');
      expect(() => showFirstTab('#win')).not.toThrow();
    });
  });

  describe('collapse', () => {
    const accordion = `
      <button id="b1" data-toggle="collapse" data-target="#c1" aria-expanded="false" class="collapsed">1</button>
      <div id="c1" class="collapse" data-parent="#acc">a</div>
      <button id="b2" data-toggle="collapse" data-target="#c2" aria-expanded="false" class="collapsed">2</button>
      <div id="c2" class="collapse" data-parent="#acc">b</div>`;

    it('opens and closes a panel and keeps its button in step', () => {
      page(`<div id="acc">${accordion}</div>`);
      click(document.getElementById('b1')!);
      expect(document.getElementById('c1')!.classList.contains('in')).toBe(true);
      expect(document.getElementById('b1')!.getAttribute('aria-expanded')).toBe('true');
      expect(document.getElementById('b1')!.classList.contains('collapsed')).toBe(false);
      click(document.getElementById('b1')!);
      expect(document.getElementById('c1')!.classList.contains('in')).toBe(false);
      expect(document.getElementById('b1')!.getAttribute('aria-expanded')).toBe('false');
    });

    it('as an accordion, opening one closes the other', () => {
      page(`<div id="acc">${accordion}</div>`);
      click(document.getElementById('b1')!);
      click(document.getElementById('b2')!);
      expect(document.getElementById('c1')!.classList.contains('in')).toBe(false);
      expect(document.getElementById('c2')!.classList.contains('in')).toBe(true);
    });
  });
});
