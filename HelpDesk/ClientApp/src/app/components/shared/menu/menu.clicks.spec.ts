import { describe, expect, it } from 'vitest';
import { MenuComponent } from './menu.component';
import { openScreen, USERS } from '../../../testing/screen-testing';

describe('MenuComponent, pressing the buttons', () => {
  it('choosing an entry or the dark area closes the menu on the phone', () => {
    const s = openScreen(MenuComponent, USERS.admin); s.detect();
    const shell = (s.component as any).shell;
    shell.toggle();
    s.detect();
    s.element.querySelector<HTMLElement>('.hd-nav-item')!.click(); s.detect();
    expect(shell.menuOpen).toBe(false);
    shell.toggle(); s.detect();
    s.element.querySelector<HTMLElement>('.hd-scrim')!.click(); s.detect();
    expect(shell.menuOpen).toBe(false);
  });

  it('the terms link opens its window and does not follow the address', () => {
    const s = openScreen(MenuComponent, USERS.admin); s.detect();
    const event = new MouseEvent('click', { cancelable: true, bubbles: true });
    s.element.querySelector<HTMLElement>('.hd-legal a')!.dispatchEvent(event);
    expect(event.defaultPrevented).toBe(true);
  });
});
