import { useState, useCallback } from 'react';
import ReactFlow, {
  Node,
  Edge,
  Controls,
  Background,
  useNodesState,
  useEdgesState,
  addEdge,
  Connection,
  NodeChange,
  EdgeChange,
  NodeTypes
} from 'reactflow';
import 'reactflow/dist/style.css';
import type { EraserDiagram, EraserEntity, EraserConnection } from '@coordraw/compiler';
import { parse, DiagramNode } from '@coordraw/dsl';
import { compile } from '@coordraw/compiler';
import { DiagramNode as CustomNode } from './components/DiagramNode';
import './App.css';

const nodeTypes: NodeTypes = {
  custom: CustomNode
};

function App() {
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  const [dslSource, setDslSource] = useState('');
  const [showSource, setShowSource] = useState(true);

  const loadFromDSL = useCallback((source: string) => {
    try {
      const parseResult = parse(source);
      if (parseResult.errors.length > 0) {
        console.error('Parse errors:', parseResult.errors);
        alert(`Parse errors:\n${parseResult.errors.map(e => `Line ${e.line}: ${e.message}`).join('\n')}`);
        return;
      }

      const eraserDiagram = compile(parseResult.diagram);
      loadFromEraser(eraserDiagram);
      setDslSource(source);
    } catch (error) {
      console.error('Failed to load DSL:', error);
      alert(`Failed to load DSL: ${error}`);
    }
  }, []);

  const loadFromEraser = useCallback((diagram: EraserDiagram) => {
    const newNodes: Node[] = diagram.entities.map((entity: EraserEntity) => ({
      id: entity.id,
      type: 'custom',
      position: { x: entity.x, y: entity.y },
      data: {
        label: entity.texts?.[0]?.text || entity.id,
        entityType: entity.type,
        color: entity.color,
        width: 'width' in entity ? entity.width : 150,
        height: 'height' in entity ? entity.height : 60
      },
      style: {
        width: 'width' in entity ? entity.width : 150,
        height: 'height' in entity ? entity.height : 60
      }
    }));

    const newEdges: Edge[] = diagram.connections.map((conn: EraserConnection) => ({
      id: conn.id,
      source: conn.from,
      target: conn.to,
      label: conn.label,
      type: conn.lineStyle === 'dashed' ? 'smoothstep' : 'default',
      animated: false,
      style: {
        strokeDasharray: conn.lineStyle === 'dashed' ? '5,5' : undefined
      }
    }));

    setNodes(newNodes);
    setEdges(newEdges);
  }, [setNodes, setEdges]);

  const handleFileUpload = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const content = e.target?.result as string;
      
      if (file.name.endsWith('.crd')) {
        loadFromDSL(content);
      } else if (file.name.endsWith('.json')) {
        try {
          const diagram = JSON.parse(content);
          loadFromEraser(diagram);
        } catch (error) {
          alert(`Failed to parse JSON: ${error}`);
        }
      } else {
        alert('Unsupported file type. Use .crd or .json');
      }
    };
    reader.readAsText(file);
  }, [loadFromDSL, loadFromEraser]);

  const exportToJSON = useCallback(() => {
    const diagram: EraserDiagram = {
      entities: nodes.map(node => {
        const baseEntity = {
          id: node.id,
          x: node.position.x,
          y: node.position.y,
          texts: [{ text: node.data.label, typeface: 'clean' as const, fontSize: 14 }],
          color: node.data.color,
          styleMode: 'clean' as const
        };

        if (node.data.entityType === 'group') {
          return {
            ...baseEntity,
            type: 'group' as const,
            width: node.data.width,
            height: node.data.height
          };
        } else if (node.data.entityType === 'icon') {
          return {
            ...baseEntity,
            type: 'icon' as const,
            icon: 'server'
          };
        } else {
          return {
            ...baseEntity,
            type: 'shape' as const,
            width: node.data.width,
            height: node.data.height
          };
        }
      }),
      connections: edges.map(edge => ({
        id: edge.id,
        from: edge.source,
        to: edge.target,
        label: edge.label as string | undefined,
        endArrowhead: 'arrow' as const,
        lineStyle: edge.style?.strokeDasharray ? 'dashed' as const : 'solid' as const,
        typeface: 'clean' as const,
        fontSize: 12
      }))
    };

    const blob = new Blob([JSON.stringify(diagram, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'diagram.json';
    a.click();
    URL.revokeObjectURL(url);
  }, [nodes, edges]);

  const onConnect = useCallback(
    (connection: Connection) => setEdges((eds) => addEdge(connection, eds)),
    [setEdges]
  );

  return (
    <div className="app">
      <div className="toolbar">
        <h1>Coordraw Editor</h1>
        <div className="toolbar-actions">
          <label className="file-upload">
            Load File
            <input type="file" accept=".crd,.json" onChange={handleFileUpload} />
          </label>
          <button onClick={exportToJSON}>Export JSON</button>
          <button onClick={() => setShowSource(!showSource)}>
            {showSource ? 'Hide' : 'Show'} Source
          </button>
        </div>
      </div>
      
      <div className="content">
        <div className="canvas">
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            nodeTypes={nodeTypes}
            fitView
          >
            <Background />
            <Controls />
          </ReactFlow>
        </div>
        
        {showSource && (
          <div className="source-panel">
            <h3>DSL Source</h3>
            <textarea
              value={dslSource}
              onChange={(e) => setDslSource(e.target.value)}
              placeholder="Paste or edit DSL source here..."
            />
            <button onClick={() => loadFromDSL(dslSource)}>
              Apply DSL
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
