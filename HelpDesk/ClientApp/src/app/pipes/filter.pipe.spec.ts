import { describe, expect, it } from 'vitest';
import { FilterPipe } from './filter.pipe';

describe('FilterPipe', () => {
  const pipe = new FilterPipe();
  const rows = [
    { noSecuencia: 12, cliente: 'Cafetería Martha', estado: 'Abierto', nota: null },
    { noSecuencia: 7, cliente: 'Colegio Horizonte', estado: 'en Proceso', nota: 'pieza pedida' },
    { noSecuencia: 3, cliente: 'Farmacia San Rafael', estado: 'Completado', nota: null }
  ];

  it('returns the list as it is when there is nothing to search', () => {
    expect(pipe.transform(rows, '')).toBe(rows);
    expect(pipe.transform(rows, undefined)).toBe(rows);
  });

  it('keeps the rows that mention the text in any field, ignoring the case', () => {
    expect(pipe.transform(rows, 'HORIZONTE')).toEqual([rows[1]]);
    expect(pipe.transform(rows, 'proceso')).toEqual([rows[1]]);
  });

  it('searches numbers as text', () => {
    expect(pipe.transform(rows, '12')).toEqual([rows[0]]);
  });

  it('skips empty fields without failing', () => {
    expect(pipe.transform(rows, 'pieza')).toEqual([rows[1]]);
  });

  it('returns an empty list when nothing matches', () => {
    expect(pipe.transform(rows, 'no existe')).toEqual([]);
  });

  it('lists a row once even if several fields match', () => {
    const twice = [{ a: 'orden', b: 'orden de taller' }];
    expect(pipe.transform(twice, 'orden').length).toBe(1);
  });
});
