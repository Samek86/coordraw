/**
 * Eraser Diagrams JSON Types
 * Based on @eraserlabs/diagrams schema
 */

export interface EraserDiagram {
  entities: EraserEntity[];
  connections: EraserConnection[];
}

export type EraserEntity = EraserShape | EraserGroup | EraserIcon;

export interface EraserShape {
  id: string;
  type: 'shape';
  x: number;
  y: number;
  width: number;
  height: number;
  texts: EraserText[];
  containerId?: string;
  color?: string;
  styleMode?: 'rough' | 'clean';
}

export interface EraserGroup {
  id: string;
  type: 'group';
  x: number;
  y: number;
  width: number;
  height: number;
  texts: EraserText[];
  color?: string;
  styleMode?: 'rough' | 'clean';
}

export interface EraserIcon {
  id: string;
  type: 'icon';
  icon: string;
  x: number;
  y: number;
  texts: EraserText[];
  containerId?: string;
  color?: string;
  styleMode?: 'rough' | 'clean';
}

export interface EraserText {
  text: string;
  typeface?: 'rough' | 'clean' | 'mono';
  fontSize?: number;
}

export interface EraserConnection {
  id: string;
  from: string;
  to: string;
  label?: string;
  endArrowhead?: 'arrow' | 'none';
  lineStyle?: 'solid' | 'dashed' | 'dotted';
  typeface?: 'rough' | 'clean' | 'mono';
  fontSize?: number;
}
