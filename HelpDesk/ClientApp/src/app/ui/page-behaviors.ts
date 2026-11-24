// Modals, tabs and collapsible panels driven by the data-* attributes the templates already carry
// (data-toggle, data-target, data-dismiss, data-backdrop). One click listener on the document, no jQuery.
// The class names (modal, fade, in, modal-backdrop, modal-open, active, collapse) are the ones the styles and the tests use.

const FADE_MS = 150;
const openModals: HTMLElement[] = [];
const backdrops = new Map<HTMLElement, HTMLElement>();
const openers = new Map<HTMLElement, HTMLElement | null>();

export function showModal(modal: HTMLElement | null, opener: HTMLElement | null = null): void {
  if (!modal || openModals.includes(modal)) return;
  openModals.push(modal);
  openers.set(modal, opener);
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop fade';
  document.body.appendChild(backdrop);
  backdrops.set(modal, backdrop);
  document.body.classList.add('modal-open');
  modal.style.display = 'block';
  modal.removeAttribute('aria-hidden');
  modal.setAttribute('aria-modal', 'true');
  modal.scrollTop = 0;
  void modal.offsetWidth; // let the browser see display:block before the fade starts
  modal.classList.add('in');
  backdrop.classList.add('in');
  modal.focus({ preventScroll: true });
}

export function hideModal(modal: HTMLElement | null): void {
  if (!modal) return;
  const at = openModals.indexOf(modal);
  if (at < 0) return;
  openModals.splice(at, 1);
  modal.classList.remove('in');
  const backdrop = backdrops.get(modal);
  backdrop?.classList.remove('in');
  const opener = openers.get(modal);
  setTimeout(() => {
    modal.style.display = 'none';
    modal.setAttribute('aria-hidden', 'true');
    modal.removeAttribute('aria-modal');
    backdrop?.remove();
    backdrops.delete(modal);
    openers.delete(modal);
    if (!openModals.length) document.body.classList.remove('modal-open');
    if (opener && document.contains(opener)) opener.focus({ preventScroll: true });
  }, FADE_MS);
}

function targetOf(el: HTMLElement): HTMLElement | null {
  const sel = el.getAttribute('data-target') || el.getAttribute('href');
  if (!sel || sel === '#' || sel[0] !== '#') return null;
  return document.getElementById(sel.slice(1));
}

function toggleTab(link: HTMLElement): void {
  const item = link.closest('li');
  const pane = targetOf(link);
  if (!item || !pane) return;
  item.parentElement?.querySelectorAll(':scope > li.active').forEach(li => li.classList.remove('active'));
  item.classList.add('active');
  pane.parentElement?.querySelectorAll(':scope > .tab-pane.active').forEach(p => p.classList.remove('active', 'in'));
  pane.classList.add('active', 'in');
}

function toggleCollapse(button: HTMLElement): void {
  const panel = targetOf(button);
  if (!panel) return;
  const open = !panel.classList.contains('in');
  const parent = panel.getAttribute('data-parent');
  if (open && parent) {
    document.querySelectorAll(parent + ' .collapse.in').forEach(other => {
      if (other !== panel) other.classList.remove('in');
    });
  }
  panel.classList.toggle('in', open);
  button.classList.toggle('collapsed', !open);
  button.setAttribute('aria-expanded', String(open));
}

export function installPageBehaviors(): void {
  document.addEventListener('click', (event) => {
    const start = event.target as HTMLElement | null;
    if (!start || !start.closest) return;

    const dismiss = start.closest('[data-dismiss="modal"]') as HTMLElement | null;
    if (dismiss) { hideModal(dismiss.closest('.modal') as HTMLElement | null); return; }

    const toggle = start.closest('[data-toggle]') as HTMLElement | null;
    if (toggle) {
      const kind = toggle.getAttribute('data-toggle');
      if (kind === 'modal') {
        if (toggle.tagName === 'A') event.preventDefault();
        showModal(targetOf(toggle), toggle);
      } else if (kind === 'tab') {
        event.preventDefault();
        toggleTab(toggle);
      } else if (kind === 'collapse') {
        event.preventDefault();
        toggleCollapse(toggle);
      }
      return;
    }

    // A click on the dimmed area (the modal element itself) closes it unless the modal is static
    if (start.classList.contains('modal') && openModals.includes(start) && start.getAttribute('data-backdrop') !== 'static') {
      hideModal(start);
    }
  });

  document.addEventListener('keydown', (event) => {
    if (event.key !== 'Escape' || !openModals.length) return;
    const top = openModals[openModals.length - 1];
    if (top.getAttribute('data-keyboard') !== 'false') hideModal(top);
  });
}
