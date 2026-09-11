import { describe, it, expect } from 'vitest';
import { parse } from '../parser.js';

describe('DSL Parser', () => {
  it('should parse a simple diagram', () => {
    const source = `
      diagram "Test Diagram"
      box box1 label="Box 1" x=10 y=20 w=100 h=50 color=blue
    `;

    const result = parse(source);
    
    expect(result.errors).toHaveLength(0);
    expect(result.diagram.title).toBe('Test Diagram');
    expect(result.diagram.children).toHaveLength(1);
    expect(result.diagram.children[0].type).toBe('box');
  });

  it('should parse Japanese labels', () => {
    const source = `
      diagram "日本語テスト"
      box client label="クライアント" x=50 y=100 w=120 h=60 color=green
    `;

    const result = parse(source);
    
    expect(result.errors).toHaveLength(0);
    expect(result.diagram.title).toBe('日本語テスト');
    const box = result.diagram.children[0];
    expect(box.type).toBe('box');
    if (box.type === 'box') {
      expect(box.label).toBe('クライアント');
      expect(box.color).toBe('green');
    }
  });

  it('should parse groups with children', () => {
    const source = `
      diagram "Group Test"
      group grp1 title="Container" x=100 y=100 w=300 h=200 color=blue
        icon api icon=server label="API" x=150 y=150
        icon db icon=database label="DB" x=250 y=150
      end
    `;

    const result = parse(source);
    
    expect(result.errors).toHaveLength(0);
    expect(result.diagram.children).toHaveLength(1);
    
    const group = result.diagram.children[0];
    expect(group.type).toBe('group');
    if (group.type === 'group') {
      expect(group.title).toBe('Container');
      expect(group.children).toHaveLength(2);
      expect(group.children[0].type).toBe('icon');
      expect(group.children[1].type).toBe('icon');
    }
  });

  it('should parse edges', () => {
    const source = `
      diagram "Edge Test"
      box box1 label="A" x=10 y=10 w=50 h=50
      box box2 label="B" x=100 y=10 w=50 h=50
      edge box1 -> box2 label="connects" style=dashed
    `;

    const result = parse(source);
    
    expect(result.errors).toHaveLength(0);
    expect(result.diagram.children).toHaveLength(3);
    
    const edge = result.diagram.children[2];
    expect(edge.type).toBe('edge');
    if (edge.type === 'edge') {
      expect(edge.from).toBe('box1');
      expect(edge.to).toBe('box2');
      expect(edge.label).toBe('connects');
      expect(edge.style).toBe('dashed');
    }
  });

  it('should handle parse errors gracefully', () => {
    const source = `
      diagram "Error Test"
      invalid command here
      box box1 label="Valid" x=10 y=10 w=50 h=50
    `;

    const result = parse(source);
    
    expect(result.errors.length).toBeGreaterThan(0);
    expect(result.errors[0].message).toContain('Unknown command');
  });

  it('should skip comments', () => {
    const source = `
      # This is a comment
      diagram "Comment Test"
      # Another comment
      box box1 label="Box" x=10 y=10 w=50 h=50
    `;

    const result = parse(source);
    
    expect(result.errors).toHaveLength(0);
    expect(result.diagram.children).toHaveLength(1);
  });

  it('should parse icons', () => {
    const source = `
      diagram "Icon Test"
      icon srv icon=server label="Server" x=100 y=100 color=red
    `;

    const result = parse(source);
    
    expect(result.errors).toHaveLength(0);
    const icon = result.diagram.children[0];
    expect(icon.type).toBe('icon');
    if (icon.type === 'icon') {
      expect(icon.icon).toBe('server');
      expect(icon.label).toBe('Server');
      expect(icon.color).toBe('red');
    }
  });

  it('should validate edge references', () => {
    const source = `
      diagram "Edge Validation"
      box box1 label="A" x=10 y=10 w=50 h=50
      edge box1 -> nonexistent
    `;

    const result = parse(source);
    
    expect(result.errors.length).toBeGreaterThan(0);
    expect(result.errors.some(e => e.message.includes('unknown node'))).toBe(true);
  });
});
