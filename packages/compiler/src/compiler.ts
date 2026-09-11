/**
 * Coordraw to Eraser Diagrams Compiler
 */

import type { DiagramNode, BoxNode, GroupNode, IconNode, EdgeNode, Color } from '@coordraw/dsl';
import type { EraserDiagram, EraserShape, EraserGroup, EraserIcon, EraserConnection } from './eraser-types.js';

const COLOR_MAP: Record<Color, string> = {
  red: '#ff6b6b',
  orange: '#ffa94d',
  yellow: '#ffd43b',
  green: '#51cf66',
  blue: '#4dabf7',
  purple: '#9775fa',
  pink: '#f06595',
  gray: '#868e96',
  black: '#212529',
  white: '#ffffff'
};

export class Compiler {
  private groupMap = new Map<string, GroupNode>();

  compile(diagram: DiagramNode): EraserDiagram {
    this.groupMap.clear();

    // First pass: build group map
    for (const child of diagram.children) {
      if (child.type === 'group') {
        this.groupMap.set(child.id, child);
      }
    }

    const entities: (EraserShape | EraserGroup | EraserIcon)[] = [];
    const connections: EraserConnection[] = [];

    for (const child of diagram.children) {
      switch (child.type) {
        case 'box':
          entities.push(this.compileBox(child));
          break;
        case 'group':
          entities.push(this.compileGroup(child));
          break;
        case 'icon':
          entities.push(this.compileIcon(child));
          break;
        case 'edge':
          connections.push(this.compileEdge(child));
          break;
      }
    }

    return { entities, connections };
  }

  private compileBox(node: BoxNode): EraserShape {
    return {
      id: node.id,
      type: 'shape',
      x: node.position.x,
      y: node.position.y,
      width: node.size.width,
      height: node.size.height,
      texts: [{ text: node.label, typeface: 'clean', fontSize: 14 }],
      color: node.color ? COLOR_MAP[node.color] : undefined,
      styleMode: 'clean'
    };
  }

  private compileGroup(node: GroupNode): EraserGroup {
    const group: EraserGroup = {
      id: node.id,
      type: 'group',
      x: node.position.x,
      y: node.position.y,
      width: node.size.width,
      height: node.size.height,
      texts: [{ text: node.title, typeface: 'clean', fontSize: 16 }],
      color: node.color ? COLOR_MAP[node.color] : undefined,
      styleMode: 'clean'
    };

    return group;
  }

  private compileIcon(node: IconNode): EraserIcon {
    // Determine containerId by checking if this icon is in a group
    let containerId: string | undefined;
    for (const [groupId, group] of this.groupMap) {
      if (group.children.some(c => c.id === node.id)) {
        containerId = groupId;
        break;
      }
    }

    return {
      id: node.id,
      type: 'icon',
      icon: node.icon,
      x: node.position.x,
      y: node.position.y,
      texts: [{ text: node.label, typeface: 'clean', fontSize: 14 }],
      containerId,
      color: node.color ? COLOR_MAP[node.color] : undefined,
      styleMode: 'clean'
    };
  }

  private compileEdge(node: EdgeNode): EraserConnection {
    return {
      id: node.id,
      from: node.from,
      to: node.to,
      label: node.label,
      endArrowhead: node.arrow ? 'arrow' : 'none',
      lineStyle: node.style || 'solid',
      typeface: 'clean',
      fontSize: 12
    };
  }
}

export function compile(diagram: DiagramNode): EraserDiagram {
  const compiler = new Compiler();
  return compiler.compile(diagram);
}
