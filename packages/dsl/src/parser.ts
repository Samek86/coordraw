/**
 * Coordraw DSL Parser
 * 
 * Simple line-based parser for diagram DSL.
 * 
 * Grammar:
 * - diagram "<title>"
 * - box <id> label="<text>" x=<n> y=<n> w=<n> h=<n> [color=<color>]
 * - group <id> title="<text>" x=<n> y=<n> w=<n> h=<n> [color=<color>]
 * - icon <id> icon=<type> label="<text>" x=<n> y=<n> [color=<color>]
 * - edge <from> -> <to> [label="<text>"] [style=<style>]
 */

import type {
  DiagramNode,
  BoxNode,
  GroupNode,
  IconNode,
  EdgeNode,
  ParseResult,
  ParseError,
  Color,
  IconType
} from './types.js';

export class Parser {
  private lines: string[] = [];
  private currentLine = 0;
  private errors: ParseError[] = [];
  private groupStack: GroupNode[] = [];
  private nodeMap: Map<string, BoxNode | GroupNode | IconNode> = new Map();

  parse(source: string): ParseResult {
    this.lines = source.split('\n').map(l => l.trim()).filter(l => l && !l.startsWith('#'));
    this.currentLine = 0;
    this.errors = [];
    this.groupStack = [];
    this.nodeMap = new Map();

    let diagram: DiagramNode | null = null;

    while (this.currentLine < this.lines.length) {
      const line = this.lines[this.currentLine];
      const lineNum = this.currentLine + 1;

      try {
        if (line.startsWith('diagram ')) {
          diagram = this.parseDiagram(line, lineNum);
        } else if (line.startsWith('box ')) {
          const box = this.parseBox(line, lineNum);
          if (box) {
            this.nodeMap.set(box.id, box);
            if (this.groupStack.length > 0) {
              this.groupStack[this.groupStack.length - 1].children.push(box);
            } else if (diagram) {
              diagram.children.push(box);
            }
          }
        } else if (line.startsWith('group ')) {
          const group = this.parseGroup(line, lineNum);
          if (group) {
            this.nodeMap.set(group.id, group);
            if (this.groupStack.length > 0) {
              this.errors.push({ line: lineNum, message: 'Nested groups not yet supported' });
            } else if (diagram) {
              diagram.children.push(group);
              this.groupStack.push(group);
            }
          }
        } else if (line.startsWith('icon ')) {
          const icon = this.parseIcon(line, lineNum);
          if (icon) {
            this.nodeMap.set(icon.id, icon);
            if (this.groupStack.length > 0) {
              this.groupStack[this.groupStack.length - 1].children.push(icon);
            } else if (diagram) {
              diagram.children.push(icon);
            }
          }
        } else if (line.startsWith('edge ')) {
          const edge = this.parseEdge(line, lineNum);
          if (edge && diagram) {
            diagram.children.push(edge);
          }
        } else if (line === 'end') {
          if (this.groupStack.length > 0) {
            this.groupStack.pop();
          }
        } else {
          this.errors.push({ line: lineNum, message: `Unknown command: ${line}` });
        }
      } catch (e) {
        this.errors.push({
          line: lineNum,
          message: e instanceof Error ? e.message : String(e)
        });
      }

      this.currentLine++;
    }

    if (!diagram) {
      diagram = {
        id: 'root',
        type: 'diagram',
        title: 'Untitled',
        children: []
      };
      this.errors.push({ line: 0, message: 'No diagram declaration found' });
    }

    return { diagram, errors: this.errors };
  }

  private parseDiagram(line: string, lineNum: number): DiagramNode {
    const match = line.match(/^diagram\s+"([^"]+)"/);
    if (!match) {
      throw new Error('Invalid diagram syntax. Expected: diagram "title"');
    }

    return {
      id: 'root',
      type: 'diagram',
      title: match[1],
      children: []
    };
  }

  private parseBox(line: string, lineNum: number): BoxNode | null {
    const attrs = this.parseAttributes(line);
    const id = this.extractId(line);

    if (!id) {
      this.errors.push({ line: lineNum, message: 'Box must have an id' });
      return null;
    }

    if (!attrs.label) {
      this.errors.push({ line: lineNum, message: 'Box must have a label' });
      return null;
    }

    const x = attrs.x !== undefined ? Number(attrs.x) : 0;
    const y = attrs.y !== undefined ? Number(attrs.y) : 0;
    const w = attrs.w !== undefined ? Number(attrs.w) : 100;
    const h = attrs.h !== undefined ? Number(attrs.h) : 50;

    return {
      id,
      type: 'box',
      label: attrs.label,
      position: { x, y },
      size: { width: w, height: h },
      color: attrs.color as Color | undefined
    };
  }

  private parseGroup(line: string, lineNum: number): GroupNode | null {
    const attrs = this.parseAttributes(line);
    const id = this.extractId(line);

    if (!id) {
      this.errors.push({ line: lineNum, message: 'Group must have an id' });
      return null;
    }

    if (!attrs.title) {
      this.errors.push({ line: lineNum, message: 'Group must have a title' });
      return null;
    }

    const x = attrs.x !== undefined ? Number(attrs.x) : 0;
    const y = attrs.y !== undefined ? Number(attrs.y) : 0;
    const w = attrs.w !== undefined ? Number(attrs.w) : 200;
    const h = attrs.h !== undefined ? Number(attrs.h) : 150;

    return {
      id,
      type: 'group',
      title: attrs.title,
      position: { x, y },
      size: { width: w, height: h },
      color: attrs.color as Color | undefined,
      children: []
    };
  }

  private parseIcon(line: string, lineNum: number): IconNode | null {
    const attrs = this.parseAttributes(line);
    const id = this.extractId(line);

    if (!id) {
      this.errors.push({ line: lineNum, message: 'Icon must have an id' });
      return null;
    }

    if (!attrs.icon) {
      this.errors.push({ line: lineNum, message: 'Icon must have an icon type' });
      return null;
    }

    if (!attrs.label) {
      this.errors.push({ line: lineNum, message: 'Icon must have a label' });
      return null;
    }

    const x = attrs.x !== undefined ? Number(attrs.x) : 0;
    const y = attrs.y !== undefined ? Number(attrs.y) : 0;

    return {
      id,
      type: 'icon',
      icon: attrs.icon as IconType,
      label: attrs.label,
      position: { x, y },
      color: attrs.color as Color | undefined
    };
  }

  private parseEdge(line: string, lineNum: number): EdgeNode | null {
    // edge client -> api label="HTTPS" style=dashed
    const arrowMatch = line.match(/^edge\s+(\w+)\s+->\s+(\w+)/);
    if (!arrowMatch) {
      this.errors.push({ line: lineNum, message: 'Invalid edge syntax' });
      return null;
    }

    const from = arrowMatch[1];
    const to = arrowMatch[2];
    const attrs = this.parseAttributes(line);

    if (!this.nodeMap.has(from)) {
      this.errors.push({ line: lineNum, message: `Edge from unknown node: ${from}` });
      return null;
    }

    if (!this.nodeMap.has(to)) {
      this.errors.push({ line: lineNum, message: `Edge to unknown node: ${to}` });
      return null;
    }

    return {
      id: `${from}-${to}`,
      type: 'edge',
      from,
      to,
      label: attrs.label,
      style: attrs.style as 'solid' | 'dashed' | 'dotted' | undefined,
      arrow: true
    };
  }

  private extractId(line: string): string | null {
    const parts = line.split(/\s+/);
    if (parts.length < 2) return null;
    return parts[1];
  }

  private parseAttributes(line: string): Record<string, string> {
    const attrs: Record<string, string> = {};
    const regex = /(\w+)="([^"]*)"|(\w+)=(\S+)/g;
    let match;

    while ((match = regex.exec(line)) !== null) {
      const key = match[1] || match[3];
      const value = match[2] || match[4];
      attrs[key] = value;
    }

    return attrs;
  }
}

export function parse(source: string): ParseResult {
  const parser = new Parser();
  return parser.parse(source);
}
