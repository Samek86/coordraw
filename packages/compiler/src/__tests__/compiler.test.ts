import { describe, it, expect } from 'vitest';
import { parse } from '@coordraw/dsl';
import { compile } from '../compiler.js';

describe('Compiler', () => {
  it('should compile a simple box to Eraser format', () => {
    const source = `
      diagram "Test"
      box box1 label="Hello" x=10 y=20 w=100 h=50 color=blue
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.entities).toHaveLength(1);
    expect(eraser.connections).toHaveLength(0);

    const shape = eraser.entities[0];
    expect(shape.type).toBe('shape');
    expect(shape.id).toBe('box1');
    expect(shape.x).toBe(10);
    expect(shape.y).toBe(20);
    if ('width' in shape) {
      expect(shape.width).toBe(100);
      expect(shape.height).toBe(50);
    }
    expect(shape.texts[0].text).toBe('Hello');
  });

  it('should compile Japanese labels', () => {
    const source = `
      diagram "日本語"
      box client label="クライアント" x=40 y=140 w=140 h=70 color=green
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.entities).toHaveLength(1);
    const shape = eraser.entities[0];
    expect(shape.texts[0].text).toBe('クライアント');
    expect(shape.color).toBe('#51cf66'); // green
  });

  it('should compile groups', () => {
    const source = `
      diagram "Group Test"
      group grp1 title="Container" x=100 y=100 w=300 h=200 color=blue
        icon api icon=server label="API" x=150 y=150
      end
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.entities).toHaveLength(2);
    
    const group = eraser.entities[0];
    expect(group.type).toBe('group');
    expect(group.texts[0].text).toBe('Container');

    const icon = eraser.entities[1];
    expect(icon.type).toBe('icon');
    if ('containerId' in icon) {
      expect(icon.containerId).toBe('grp1');
    }
  });

  it('should compile edges to connections', () => {
    const source = `
      diagram "Edge Test"
      box a label="A" x=10 y=10 w=50 h=50
      box b label="B" x=100 y=10 w=50 h=50
      edge a -> b label="link" style=dashed
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.connections).toHaveLength(1);
    const conn = eraser.connections[0];
    expect(conn.from).toBe('a');
    expect(conn.to).toBe('b');
    expect(conn.label).toBe('link');
    expect(conn.lineStyle).toBe('dashed');
    expect(conn.endArrowhead).toBe('arrow');
  });

  it('should map colors correctly', () => {
    const source = `
      diagram "Colors"
      box red label="Red" x=0 y=0 w=50 h=50 color=red
      box blue label="Blue" x=60 y=0 w=50 h=50 color=blue
      box green label="Green" x=120 y=0 w=50 h=50 color=green
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.entities).toHaveLength(3);
    expect(eraser.entities[0].color).toBe('#ff6b6b'); // red
    expect(eraser.entities[1].color).toBe('#4dabf7'); // blue
    expect(eraser.entities[2].color).toBe('#51cf66'); // green
  });

  it('should compile icons with correct properties', () => {
    const source = `
      diagram "Icons"
      icon db icon=postgres label="Database" x=100 y=100 color=purple
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.entities).toHaveLength(1);
    const icon = eraser.entities[0];
    expect(icon.type).toBe('icon');
    if ('icon' in icon) {
      expect(icon.icon).toBe('postgres');
    }
    expect(icon.texts[0].text).toBe('Database');
    expect(icon.color).toBe('#9775fa'); // purple
  });

  it('should set styleMode to clean', () => {
    const source = `
      diagram "Style"
      box box1 label="Clean" x=0 y=0 w=50 h=50
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.entities[0].styleMode).toBe('clean');
  });

  it('should handle complex nested structures', () => {
    const source = `
      diagram "Complex"
      group prod title="Production" x=200 y=40 w=520 h=280 color=blue
        icon api icon=server label="API" x=280 y=130
        icon db icon=postgres label="DB" x=520 y=130
      end
      box client label="Client" x=40 y=140 w=140 h=70 color=green
      edge client -> api label="HTTPS"
      edge api -> db label="SQL"
    `;

    const parseResult = parse(source);
    const eraser = compile(parseResult.diagram);

    expect(eraser.entities).toHaveLength(4); // group + 2 icons + client
    expect(eraser.connections).toHaveLength(2);

    const icons = eraser.entities.filter(e => e.type === 'icon');
    expect(icons).toHaveLength(2);
    expect(icons.every(i => 'containerId' in i && i.containerId === 'prod')).toBe(true);
  });
});
