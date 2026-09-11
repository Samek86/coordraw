/**
 * Coordraw DSL Types
 */

export interface Position {
  x: number;
  y: number;
}

export interface Size {
  width: number;
  height: number;
}

export type Color = 'red' | 'orange' | 'yellow' | 'green' | 'blue' | 'purple' | 'pink' | 'gray' | 'black' | 'white';

export type IconType = 'server' | 'database' | 'postgres' | 'redis' | 'cloud' | 'user' | 'lock' | 'code';

export interface DiagramNode {
  id: string;
  type: 'diagram';
  title: string;
  children: (BoxNode | GroupNode | IconNode | EdgeNode)[];
}

export interface BoxNode {
  id: string;
  type: 'box';
  label: string;
  position: Position;
  size: Size;
  color?: Color;
}

export interface GroupNode {
  id: string;
  type: 'group';
  title: string;
  position: Position;
  size: Size;
  color?: Color;
  children: (BoxNode | IconNode)[];
}

export interface IconNode {
  id: string;
  type: 'icon';
  icon: IconType;
  label: string;
  position: Position;
  color?: Color;
}

export interface EdgeNode {
  id: string;
  type: 'edge';
  from: string;
  to: string;
  label?: string;
  style?: 'solid' | 'dashed' | 'dotted';
  arrow?: boolean;
}

export interface ParseResult {
  diagram: DiagramNode;
  errors: ParseError[];
}

export interface ParseError {
  line: number;
  message: string;
}
